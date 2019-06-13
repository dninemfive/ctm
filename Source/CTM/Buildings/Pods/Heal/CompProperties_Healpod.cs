using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9CTM
{
    class CompProperties_Healpod : CompProperties
    {
        public int baseRareTicksPerMajorHeal = 60;
        public float minorFuelUsagePercent = 0.1f, woundSealFuelUsagePercent = 0.5f, tendQuality = 1.8f, hp = 0.05f;

        public CompProperties_Healpod()
        {
            compClass = typeof(CompHealpod);
        }
    }
}
