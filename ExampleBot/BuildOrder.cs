using System;
using System.Collections.Generic;
using System.Text;
using B = BuildOrderEvents;

namespace SC2Sharp
{
    public static class BuildOrder
    {
         public static readonly B[] buildOrder = {
            B.BuildOrderBegin,
            B.Probe, // 13/15
            B.Probe, // 14/15
            B.Pylon, // 14/15
            B.Probe, // 15/15
            B.Gateway, // 15/23
            B.ChronoNext,
            B.Probe, // 16/23
            B.Probe, // 17/23
            B.Assimilator, // 17/23
            B.Assimilator, // 17/23
            B.Probe, // 18/23
            B.Pylon,  // 18/23
            B.Probe, // 19/23
            B.CyberCore, // 19/23
            B.Probe, // 20/23
            B.Zealot, // 22/23
            B.Probe, // 23/31
            B.Probe, // 24/31
            B.Stargate, // 24/31
            B.ChronoNext,
            B.Sentry, // 26/31
            B.WarpGate, // 26/31
            B.Probe, // 27/31
            B.Pylon, // 27/31
            B.Stalker, // 29/31
            B.ChronoNext,
            B.VoidRay, // 33/39
            B.Pylon, // 33/39
            B.Zealot, // 35/39
            B.Zealot, // 37/47
            B.Oracle, // 40/47
            B.BuildOrderEnd,
        };
    }
    
    
}
