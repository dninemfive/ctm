using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9CTM
{
    class CompProperties_Healpod : CompProperties
    {
        public int baseRareTicksPerMajorHeal;

        public CompProperties_Healpod()
        {
            compClass = typeof(CompHealpod);
        }
    }
}
