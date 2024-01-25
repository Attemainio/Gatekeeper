using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;

namespace Gatekeeper
{
    public interface IHasDoubleClick
    {
        GH_ObjectResponse OnDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e);
    }
}
