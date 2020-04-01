using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace D9CTM
{
    class HediffCompProperties_TimePerImmunity : HediffCompProperties
    {
# pragma warning disable CS0649
        public SimpleCurve curve;
# pragma warning restore CS0649
    }
    class HediffComp_TimePerImmunity : HediffComp
    {
        public HediffCompProperties_TimePerImmunity Props => (HediffCompProperties_TimePerImmunity)base.props;
        public int TicksLeft;
        public override bool CompShouldRemove => base.CompShouldRemove || TicksLeft <= 0;

        public override void CompPostMake()
        {
            TicksLeft = (int)(GenDate.TicksPerHour * Props.curve.Evaluate(ImmunityRecoveryTime()));
        }

        public float ImmunityRecoveryTime()
        {
            return 0f;
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
