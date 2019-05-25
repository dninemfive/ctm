using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class HediffComp_TimePerImmunity : HediffComp
    {
        public HediffCompProperties_TimePerImmunity Props => (HediffCompProperties_TimePerImmunity)base.props;
        private int ticksLeft;
        public override bool CompShouldRemove => base.CompShouldRemove || ticksLeft <= 0;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ticksLeft = Props.ticksPer * numImmunities();
        }

        private int numImmunities()
        {
            int ct = 0;
            Pawn p = base.Pawn;
            if (p != null) //shouldn't be necessary but just to be safe
            {
                foreach (Hediff h in p.health.hediffSet.hediffs)
                {
                    ImmunityRecord ir = p.health.immunity.GetImmunityRecord(h.def);
                    if (ir != null && ir.immunity >= 1f) ct++;
                }
            }
            return ct;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksLeft--;
        }

        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);
            HediffComp_TimePerImmunity hctpi = other.TryGetComp<HediffComp_TimePerImmunity>();
            if (hctpi != null && hctpi.ticksLeft > ticksLeft)
            {
                ticksLeft = hctpi.ticksLeft;
            }
        }
        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0, false);
        }
        public override string CompDebugString()
        {
            return "ticksLeft: " + ticksLeft;
        }
    }
}
