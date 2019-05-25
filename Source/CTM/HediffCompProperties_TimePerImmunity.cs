using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class HediffCompProperties_TimePerImmunity : HediffCompProperties
    {
        public int ticksPer = GenDate.TicksPerHour * 6;
        public HediffCompProperties_TimePerImmunity()
        {
            base.compClass = typeof(HediffComp_TimePerImmunity);
        }
    }
}
