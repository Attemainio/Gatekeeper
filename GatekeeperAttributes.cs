using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel;
using Gatekeeper;
using System.Drawing;
using System.ComponentModel;
using static Gatekeeper.GH_GatekeeperComponent;
using System;


namespace Gatekeeper
{
    public class GatekeeperAttributes : GH_ComponentAttributes
    {
        readonly IGH_Component component;


        public GH_PaletteStyle palette_normal_standard;
        public GH_PaletteStyle palette_normal_selected;
        public GH_PaletteStyle palette_hidden_standard;
        public GH_PaletteStyle palette_hidden_selected;



        private Font font;



        public GH_PaletteStyle GetStyle(Phases phase)
        {

            GH_PaletteStyle OpenSelectedStyle = new GH_PaletteStyle(Color.FromArgb(76, 148, 122)); // (76, 128, 122));
            GH_PaletteStyle OpenUnselectedStyle = new GH_PaletteStyle(Color.FromArgb(95, 135, 113));

            // GATE CLOSED AND UPDATE
            GH_PaletteStyle ClosedUpdatedSelectedStyle = new GH_PaletteStyle(Color.FromArgb(65, 74, 75));  // (135, 134, 115));
            GH_PaletteStyle ClosedUpdatedUnselectedStyle = new GH_PaletteStyle(Color.FromArgb(93, 98, 91));

            // GATECLOSEDOUTDATED
            GH_PaletteStyle ClosedOutdatedSelectedStyle = new GH_PaletteStyle(Color.FromArgb(94, 36, 36));
            GH_PaletteStyle ClosedOutdatedUnselectedStyle = new GH_PaletteStyle(Color.FromArgb(138, 70, 70));



            if (Selected)
            {
                switch (phase)
                {
                    case Phases.Open:
                        return OpenSelectedStyle;
                    case Phases.CloseAndUpdated:
                        return ClosedUpdatedSelectedStyle;
                    case Phases.CloseAndOutdated:
                        return ClosedOutdatedSelectedStyle;
                    default:
                        return palette_normal_selected;
                }
            }
            else
            {
                switch (phase)
                {
                    case Phases.Open:
                        return OpenUnselectedStyle;
                    case Phases.CloseAndUpdated:
                        return ClosedUpdatedUnselectedStyle;
                    case Phases.CloseAndOutdated:
                        return ClosedOutdatedUnselectedStyle;
                    default:
                        return palette_normal_standard;
                }
            }
        }



        public GatekeeperAttributes(IGH_Component component)
          : base(component)
        {
            this.component = component;

            FontFamily fontFamily = new FontFamily("Arial");
            font = new Font(
                 fontFamily,
                   8,
                 FontStyle.Bold,
                 GraphicsUnit.Pixel);
        }



        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {

            if (component is IHasDoubleClick clickComponent)
            {
                clickComponent.OnDoubleClick(sender, e);
                return GH_ObjectResponse.Handled;
            }

            return base.RespondToMouseDoubleClick(sender, e);

        }


        /// <summary>
        /// Renders the running components in another color
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="graphics"></param>
        /// <param name="channel"></param>
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            IHasPhase comp = component as IHasPhase;

            if (comp == null)
            {
                base.Render(canvas, graphics, channel);
                return;
            }


            string s = string.Empty;

            switch (comp.Phase)
            {
                case Phases.Open:
                    s = "GATE: LIVE";
                    break;
                case Phases.CloseAndUpdated:
                    s = $"GATE: CACHED\nUpdated {(comp.LastRun-DateTime.Now).ToShortString()} ago";
                    break;
                case Phases.CloseAndOutdated:
                    s = $"GATE: OUTDATED\nUpdated {{(comp.LastRun-DateTime.Now).ToShortString()}} ago\"";
                    break;
            }


            switch (channel)
            {
                case GH_CanvasChannel.Wires:

                    var zoom = Grasshopper.Instances.ActiveCanvas.Viewport.Zoom;

                    if (comp != null && zoom >= 1.0)
                    {
                        RenderText(
                            s: s,
                            style: GetStyle(comp.Phase),
                            graphics: graphics);
                    }
                    base.Render(canvas, graphics, channel);
                    break;


                case GH_CanvasChannel.Objects:

                    // Save original
                    palette_normal_standard = GH_Skin.palette_normal_standard;
                    palette_hidden_standard = GH_Skin.palette_hidden_standard;
                    palette_normal_selected = GH_Skin.palette_normal_selected;
                    palette_hidden_selected = GH_Skin.palette_hidden_selected;

                    GH_Skin.palette_normal_standard = GetStyle(comp.Phase);
                    GH_Skin.palette_hidden_standard = GetStyle(comp.Phase);
                    GH_Skin.palette_normal_selected = GetStyle(comp.Phase);
                    GH_Skin.palette_hidden_selected = GetStyle(comp.Phase);

                    base.Render(canvas, graphics, channel);

                    // Put the original style back.
                    GH_Skin.palette_normal_standard = palette_normal_standard;
                    GH_Skin.palette_normal_selected = palette_normal_selected;
                    GH_Skin.palette_hidden_standard = palette_hidden_standard;
                    GH_Skin.palette_hidden_selected = palette_hidden_selected;
                    break;


                default:
                    base.Render(canvas, graphics, channel);
                    break;


            }

        }


        /// <summary>
        /// Inspired from TUNNY plugin
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="s">text to display</param>
        /// <param name="style">style/color</param>
        private void RenderText(string s, GH_PaletteStyle style, Graphics graphics)
        {
            if (string.IsNullOrEmpty(s))
                return;

            const int MAXLEN = 25;
            GH_Document doc = Owner.OnPingDocument();

            if (doc == null) return;

            RectangleF rectangle = Bounds;
            rectangle.Y += Bounds.Height + 2;
            rectangle.Width += 60;
            if (s.Length > MAXLEN)
            {
                s = s.Substring(0, MAXLEN - 1) + "...";
            }

            //rectangle.Inflate(6, 6);
            graphics.DrawString(s, font, new SolidBrush(style.Fill), rectangle);
            //graphics.FillRectangle(fill, rectangle);
            //graphics.DrawRectangle(edge, rectangle);
        }


    }
}