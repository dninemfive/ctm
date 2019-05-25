using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompUseEffect_ImmunizeAll : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            foreach (ImmunityRecord ir in ImmunitiesToSet(usedBy)) ir.immunity = 1f;
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            bool boo = hasAnyDisease(p);
            if (!boo) failReason = "D9NoDiseases".Translate(p.LabelShort);
            else failReason = null;
            return boo;
        }

        public IEnumerable<ImmunityRecord> ImmunitiesToSet(Pawn p)
        {
            foreach (Hediff h in p.health.hediffSet.hediffs)
            {
                ImmunityRecord ir = p.health.immunity.GetImmunityRecord(h.def);
                if (ir != null) yield return ir;
            }
        }

        public bool hasAnyDisease(Pawn p)
        {
            foreach(Hediff h in p.health.hediffSet.hediffs)
            {
                HediffWithComps hwc = h as HediffWithComps;
                HediffComp_Immunizable hci = null;
                if (hwc != null) hci = hwc.TryGetComp<HediffComp_Immunizable>();
                if (hci != null) return true;
            }
            return false;
        }
    }
}
