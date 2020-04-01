using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompProperties_UseEffectImmunizeAll : CompProperties_UseEffect
    {
#pragma warning disable CS0649
        public HediffDef comaHediff;
#pragma warning restore CS0649

        public CompProperties_UseEffectImmunizeAll()
        {
            base.compClass = typeof(CompUseEffect_ImmunizeAll);
        }
    }
    class CompUseEffect_ImmunizeAll : CompUseEffect
    {
        CompProperties_UseEffectImmunizeAll Props => (CompProperties_UseEffectImmunizeAll)base.props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            CauseComa(usedBy);
            foreach (ImmunityRecord ir in ImmunitiesToSet(usedBy)) ir.immunity = 1f;            
        }

        public void CauseComa(Pawn p)
        {
            p.health.forceIncap = true;
            p.health.AddHediff(Props.comaHediff);
            p.health.forceIncap = false;
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            bool boo = HasAnyDisease(p);
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

        public bool HasAnyDisease(Pawn p)
        {
            foreach (Hediff h in p.health.hediffSet.hediffs)
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