using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper
{
    public interface IHasDoubleClick
    {
        GH_ObjectResponse OnDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e);
    }
}
