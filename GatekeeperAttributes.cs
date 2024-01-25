using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System.Drawing;
using static Gatekeeper.GH_GatekeeperComponent;


namespace Gatekeeper
{
    public class GatekeeperAttributes : GH_ComponentAttributes
    {
        readonly IGH_Component component;

        public readonly static GH_PaletteStyle OpenSelectedStyle = new GH_PaletteStyle(Color.FromArgb(76, 148, 122));
        public readonly static GH_PaletteStyle OpenUnselectedStyle = new GH_PaletteStyle(Color.FromArgb(95, 135, 113));
        public readonly static GH_PaletteStyle ClosedUpdatedSelectedStyle = new GH_PaletteStyle(Color.FromArgb(65, 74, 75));
        public readonly static GH_PaletteStyle ClosedUpdatedUnselectedStyle = new GH_PaletteStyle(Color.FromArgb(93, 98, 91));
        public readonly static GH_PaletteStyle ClosedOutdatedSelectedStyle = new GH_PaletteStyle(Color.FromArgb(94, 36, 36));
        public readonly static GH_PaletteStyle ClosedOutdatedUnselectedStyle = new GH_PaletteStyle(Color.FromArgb(138, 70, 70));

        public GH_PaletteStyle palette_normal_standard;
        public GH_PaletteStyle palette_normal_selected;
        public GH_PaletteStyle palette_hidden_standard;
        public GH_PaletteStyle palette_hidden_selected;

        string message = string.Empty;

        public GH_PaletteStyle GetStyle(Phases phase)
        {
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

            switch (comp.Phase)
            {
                case Phases.Open:
                    message = "LIVE";
                    break;
                case Phases.CloseAndUpdated:
                    message = $"CACHED";
                    break;
                case Phases.CloseAndOutdated:
                    message = $"OUTDATED";
                    break;
            }


            switch (channel)
            {
                case GH_CanvasChannel.Wires:

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
    }
}