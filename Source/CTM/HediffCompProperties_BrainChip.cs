using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class HediffCompProperties_BrainChip : HediffCompProperties
    {
        public float severityDelta = 0.05f;
        public HediffCompProperties_BrainChip()
        {
            base.compClass = typeof(HediffComp_BrainChip);
        }
    }
}
