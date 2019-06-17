using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompProperties_UseEffect_AddHediff : CompProperties_UseEffect
    {
        public List<BodyPartDef> partsToAddTo = null;
        public HediffDef hediffToAdd;

        public CompProperties_UseEffect_AddHediff()
        {
            base.compClass = typeof(CompUseEffect_AddHediff);
        }
    }
}
