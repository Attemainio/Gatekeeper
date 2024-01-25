using System;
using static Gatekeeper.GH_GatekeeperComponent;

namespace Gatekeeper
{
    public interface IHasPhase
    {
        Phases Phase { get; set; }

        DateTime LastRun { get; set; }
    }
}
