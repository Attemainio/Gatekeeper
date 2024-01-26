using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace Gatekeeper
{
    public class GatekeeperAttributes : GH_ComponentAttributes
    {
        readonly GH_GatekeeperComponent component;

        public GatekeeperAttributes(IGH_Component component) : base(component)
        {
            this.component = (GH_GatekeeperComponent)component;
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            component.OnDoubleClick(sender, e);
            return GH_ObjectResponse.Handled;
        }
    }
}