using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompProperties_UseEffectTendAllWounds : CompProperties_UseEffect
    {
# pragma warning disable CS0649
        public ThingDef filthDef;
        public IntRange filthAmount;
        public FloatRange tendQuality;
# pragma warning restore CS0649

        public CompProperties_UseEffectTendAllWounds()
        {
            base.compClass = typeof(CompUseEffect_TendAllWounds);
        }
    }
    class CompUseEffect_TendAllWounds : CompUseEffect
    {
        CompProperties_UseEffectTendAllWounds Props => (CompProperties_UseEffectTendAllWounds)base.props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            IEnumerable<Hediff> toTend = HediffsToTend(usedBy);
            foreach (Hediff h in toTend) h.Tended(Props.tendQuality.RandomInRange);
            //make foam filth
            if (Props.filthDef != null)
            {
                usedBy.filth.GainFilth(Props.filthDef);
                FilthMaker.TryMakeFilth(usedBy.Position, usedBy.Map, Props.filthDef, Props.filthAmount.RandomInRange);
            }
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            IEnumerable<Hediff> toTend = HediffsToTend(p);
            bool boo = toTend.Any();
            if (!boo) failReason = "D9NoTendableWounds".Translate(p.LabelShort);
            else failReason = null;
            return boo;
        }

        public IEnumerable<Hediff> HediffsToTend(Pawn p)
        {
            foreach (Hediff h in p.health.hediffSet.hediffs)
            {
                if (h.TendableNow()) if (!Utils.Immunizable(h)) yield return h;
            }
        }        
    }
}