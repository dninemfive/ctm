using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompUseEffect_TendAllWounds : CompUseEffect
    {

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            IEnumerable<Hediff> toTend = HediffsToTend(usedBy);
            foreach (Hediff h in toTend) h.Tended(1.5f); //slightly worse than Glitterworld, but much better than other medicine
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            IEnumerable<Hediff> toTend = HediffsToTend(p);
            bool boo = toTend.Any();
            if (!boo) failReason = "D9NoTendableWounds".Translate();
            else failReason = null;
            return boo;
        }

        public IEnumerable<Hediff> HediffsToTend(Pawn p)
        {
            foreach (Hediff h in p.health.hediffSet.hediffs)
            {
                if (h.TendableNow())
                {
                    HediffWithComps com = (HediffWithComps)h;
                    if (com == null || (com != null && !immunizable(com))) yield return h;
                }
            }
            //return (from h in usedBy.health.hediffSet.hediffs where h.TendableNow() select h);
        }

        private bool immunizable(HediffWithComps h)
        {
            foreach (HediffComp c in h.comps)
            {
                if (c is HediffComp_Immunizable) return true;
            }
            return false;
        }
    }
}
