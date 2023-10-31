using Gatekeeper.Properties;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Gatekeeper
{
    public class GH_GatekeeperAttributes : GH_ComponentAttributes
    {
        #region Fields
        private Rectangle iconButtonBounds;
        private bool iconButton;
        private static Bitmap icon;
        #endregion Fields

        #region Constructor
        public GH_GatekeeperAttributes(IGH_Component component) : base(component)
        {
            this.iconButton = false;
            component.IconDisplayMode = GH_IconDisplayMode.icon;
        }
        #endregion Constructor

        #region Methods
        protected override void Layout()
        {
            this.Pivot = (PointF)GH_Convert.ToPoint(this.Pivot);
            int
                paramHeight = 20,
                paramPadding = 13,
                minComponentWidth = 32,
                minComponentHeight = 32,
                componentHeight = Math.Max(minComponentHeight, Math.Max(this.Owner.Params.Input.Count, this.Owner.Params.Output.Count) * paramHeight);

            Rectangle layoutRectangle = new Rectangle(GH_Convert.ToPoint(this.Pivot), new Size(minComponentWidth, componentHeight));
            RectangleF iconRectangle = new RectangleF()
            {
                X = 0.5f * (float)(layoutRectangle.Left + layoutRectangle.Right) - paramPadding,
                Y = 0.5f * (float)(layoutRectangle.Top + layoutRectangle.Bottom) - paramPadding,
                Width = 2 * paramPadding,
                Height = 2 * paramPadding
            };
            this.iconButtonBounds = GH_Convert.ToRectangle(iconRectangle);
            this.m_innerBounds = (RectangleF)layoutRectangle;
            GH_ComponentAttributes.LayoutInputParams(this.Owner, this.m_innerBounds);
            GH_ComponentAttributes.LayoutOutputParams(this.Owner, this.m_innerBounds);
            this.Bounds = GH_ComponentAttributes.LayoutBounds(this.Owner, this.m_innerBounds);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            GH_ObjectResponse mouseMoveResponse;
            if (e.Button == MouseButtons.None)
            {
                if (((RectangleF)this.iconButtonBounds).Contains(e.CanvasLocation))
                {
                    if (!this.iconButton)
                    {
                        this.iconButton = true;
                        sender.Invalidate();
                        mouseMoveResponse = GH_ObjectResponse.Handled;
                        return mouseMoveResponse;
                    }
                }
                else if (this.iconButton)
                {
                    this.iconButton = false;
                    sender.Invalidate();
                    mouseMoveResponse = GH_ObjectResponse.Handled;
                    return mouseMoveResponse;
                }
            }
            return base.RespondToMouseMove(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && ((RectangleF)this.iconButtonBounds).Contains(e.CanvasLocation))
            {
                GH_GatekeeperComponent.buttonTriggered = true;
                this.Owner.ExpireSolution(true);
                return GH_ObjectResponse.Handled;
            }
            else
            {
                return base.RespondToMouseUp(sender, e);
            }
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            try
            {
                if (channel is GH_CanvasChannel.Wires)
                {
                    foreach (IGH_DocumentObject param in this.Owner.Params.Input)
                    {
                        param.Attributes.RenderToCanvas(canvas, GH_CanvasChannel.Wires);
                    }
                }
                else if (channel is GH_CanvasChannel.Objects)
                {
                    this.RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true);
                    GH_GatekeeperAttributes.icon = Resources.GH_Gatekeeper.ToBitmap();
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    if (this.iconButton)
                    {
                        Point
                            clientPoint = canvas.PointToClient(Cursor.Position),
                            screenPoint = GH_Convert.ToPoint(canvas.Viewport.UnprojectPoint(clientPoint));
                        if (this.iconButtonBounds.Contains(screenPoint))
                        {
                            GH_GraphicsUtil.RenderHighlightBox(graphics, this.iconButtonBounds, 0);
                        }
                    }
                }
                GH_GraphicsUtil.RenderCenteredIcon(graphics, this.iconButtonBounds, Resources.GH_Gatekeeper.ToBitmap() as Image, ((double)GH_Canvas.ZoomFadeLow) / ((double)byte.MaxValue));
            }
            catch (Exception ex)
            {
                base.Render(canvas, graphics, channel);
                MessageBox.Show(ex.Message, "ScriptError: Gatekeeper Component Render", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion Methods
    }
}
