using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace D9CTM
{
    class HealingUtility
    {
        public static IEnumerable<Hediff_Injury> GetNonPermanentInjuries(Pawn p)
        {
            IEnumerable<Hediff> hediffs = p.health.hediffSet.hediffs;
            foreach (Hediff h in hediffs)
            {
                Hediff_Injury hi = h as Hediff_Injury;
                if (hi != null && !h.IsPermanent()) yield return hi;
            }
        }
        public static IEnumerable<ImmunityRecord> GetImmunityRecords(Pawn p)
        {
            IEnumerable<Hediff> hediffs = p.health.hediffSet.hediffs;
            foreach(Hediff h in hediffs)
            {
                ImmunityRecord ir = p.health.immunity.GetImmunityRecord(h.def);
                if (ir != null) yield return ir;
            }
        }
        public static void doMinorHeal(Pawn p)
        {
            List<Hediff> hediffs = p.health.hediffSet.hediffs;
            if (hediffs != null && hediffs.Count > 0)
            {
                foreach (Hediff h in hediffs)
                {
                    if (h is Hediff_Injury && !h.IsTended())
                    {
                        h.Severity--;
                    }
                }

            }
        }
        public static void PartiallyHealAllNonPermInjuries(Pawn p, float hp)
        {
            foreach(Hediff_Injury hi in GetNonPermanentInjuries(p))
            {
                hi.Severity -= hp;
            }
        }
        public static void doMajorHeal(Pawn p, String label) //copy of TryHealRandomPermanentWound from HediffComp_HealPermanentWounds
        {
            if (p.health.hediffSet.hediffs.Where(HediffUtility.IsPermanent).TryRandomElement(out Hediff hediff))
            {
                hediff.Severity = 0f;
                if (PawnUtility.ShouldSendNotificationAbout(p))
                {
                    Messages.Message("MessagePermanentWoundHealed".Translate(label, p.LabelShort, hediff.Label, p.Named("PAWN")), p, MessageTypeDefOf.PositiveEvent, true);
                }
            }
        }
    }
}
