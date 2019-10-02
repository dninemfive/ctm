using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9CTM
{
    class CompProperties_Archotech : CompProperties
    {
#pragma warning disable CS0649
        public IntRange TicksPerResearchPulse, TicksPerHostilePulse, HostilityAgeRange, HostilityEndRange, RaidDelay, TicksToBootUp, TicksToBootDown;

        public CompProperties_Archotech()
        {
            base.compClass = typeof(CompArchotech);
        }
    }
}
