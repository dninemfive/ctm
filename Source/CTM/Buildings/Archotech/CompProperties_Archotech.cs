using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9CTM
{
    class CompProperties_Archotech : CompProperties
    {
        public IntRange TicksPerResearchPulse;
        public IntRange TicksPerHostilePulse;
        public IntRange HostilityAgeRange;
        public IntRange HostilityEndRange;
        public IntRange RaidDelay;

        public CompProperties_Archotech()
        {
            base.compClass = typeof(CompArchotech);
        }
    }
}
