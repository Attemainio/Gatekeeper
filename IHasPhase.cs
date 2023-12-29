using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gatekeeper.GH_GatekeeperComponent;

namespace Gatekeeper
{
    public interface IHasPhase
    {
        Phases Phase { get; set; }

        DateTime LastRun { get; set; }
    }
}
