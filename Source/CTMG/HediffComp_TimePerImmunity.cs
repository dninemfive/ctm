using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace D9CTM
{
    class HediffCompProperties_TimePerImmunity : HediffCompProperties
    {
# pragma warning disable CS0649
        public SimpleCurve hoursPerImmunityCurve;
# pragma warning restore CS0649

        public HediffCompProperties_TimePerImmunity()
        {
            base.compClass = typeof(HediffComp_TimePerImmunity);
        }
    }
    class HediffComp_TimePerImmunity : HediffComp
    {
        public HediffCompProperties_TimePerImmunity Props => (HediffCompProperties_TimePerImmunity)base.props;
        public int TicksLeft;
        public override bool CompShouldRemove => base.CompShouldRemove || TicksLeft <= 0;

        public override void CompPostMake()
        {
            int ticks = (int)(GenDate.TicksPerHour * Props.hoursPerImmunityCurve.Evaluate(ImmunityRecoveryTime()));
            TicksLeft = ticks;
        }

        public float ImmunityRecoveryTime()
        {            
            float ret = 0f;
            foreach(Hediff h in base.parent.pawn.health.hediffSet.hediffs.Where(x => Utils.Immunizable(x as HediffWithComps)))
            {
                HediffComp_Immunizable hci = h.TryGetComp<HediffComp_Immunizable>();
                if (hci != null) ret += (1 - Mathf.Clamp01(hci.Immunity)); // was going to scale it to severityPerDayNotImmune, but base defs would make that inconsistent (some have negative values, for example)
            }
            return ret;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            TicksLeft--;
        }

        public override void CompPostMerged(Hediff other)
        {
            HediffComp_TimePerImmunity hctpi = other.TryGetComp<HediffComp_TimePerImmunity>();
            if (hctpi == null && hctpi.TicksLeft > TicksLeft) TicksLeft = hctpi.TicksLeft;
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref TicksLeft, "TicksLeft", 0, false);
        }

        public override string CompDebugString()
        {
            return "TicksLeft: " + TicksLeft;
        }
    }
}
