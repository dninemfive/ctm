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
        //used by the stolen CompUseEffect_HealWorstHediff code
        private static float HandCoverageAbsWithChildren => ThingDefOf.Human.race.body.GetPartsWithDef(BodyPartDefOf.Hand).First().coverageAbsWithChildren;

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
            foreach (Hediff h in hediffs)
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
            foreach (Hediff_Injury hi in GetNonPermanentInjuries(p))
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

        //all next copied from CompUseEffect_FixWorstHealthCondition
        public static void FixWorstHealthCondition(Pawn usedBy)
        {
            Hediff hediff = FindLifeThreateningHediff(usedBy);
            if (hediff != null)
            {
                Cure(hediff);
            }
            else
            {
                if (HealthUtility.TicksUntilDeathDueToBloodLoss(usedBy) < 2500)
                {
                    Hediff hediff2 = FindMostBleedingHediff(usedBy);
                    if (hediff2 != null)
                    {
                        Cure(hediff2);
                        return;
                    }
                }
                if (usedBy.health.hediffSet.GetBrain() != null)
                {
                    Hediff_Injury hediff_Injury = FindPermanentInjury(usedBy, Gen.YieldSingle(usedBy.health.hediffSet.GetBrain()));
                    if (hediff_Injury != null)
                    {
                        Cure(hediff_Injury);
                        return;
                    }
                }
                BodyPartRecord bodyPartRecord = FindBiggestMissingBodyPart(usedBy, HandCoverageAbsWithChildren);
                if (bodyPartRecord != null)
                {
                    Cure(bodyPartRecord, usedBy);
                }
                else
                {
                    Hediff_Injury hediff_Injury2 = FindPermanentInjury(usedBy, from x in usedBy.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                                                                               where x.def == BodyPartDefOf.Eye
                                                                               select x);
                    if (hediff_Injury2 != null)
                    {
                        Cure(hediff_Injury2);
                    }
                    else
                    {
                        Hediff hediff3 = FindImmunizableHediffWhichCanKill(usedBy);
                        if (hediff3 != null)
                        {
                            Cure(hediff3);
                        }
                        else
                        {
                            Hediff hediff4 = FindNonInjuryMiscBadHediff(usedBy, true);
                            if (hediff4 != null)
                            {
                                Cure(hediff4);
                            }
                            else
                            {
                                Hediff hediff5 = FindNonInjuryMiscBadHediff(usedBy, false);
                                if (hediff5 != null)
                                {
                                    Cure(hediff5);
                                }
                                else
                                {
                                    if (usedBy.health.hediffSet.GetBrain() != null)
                                    {
                                        Hediff_Injury hediff_Injury3 = FindInjury(usedBy, Gen.YieldSingle(usedBy.health.hediffSet.GetBrain()));
                                        if (hediff_Injury3 != null)
                                        {
                                            Cure(hediff_Injury3);
                                            return;
                                        }
                                    }
                                    BodyPartRecord bodyPartRecord2 = FindBiggestMissingBodyPart(usedBy, 0f);
                                    if (bodyPartRecord2 != null)
                                    {
                                        Cure(bodyPartRecord2, usedBy);
                                    }
                                    else
                                    {
                                        Hediff_Addiction hediff_Addiction = FindAddiction(usedBy);
                                        if (hediff_Addiction != null)
                                        {
                                            Cure(hediff_Addiction);
                                        }
                                        else
                                        {
                                            Hediff_Injury hediff_Injury4 = FindPermanentInjury(usedBy, null);
                                            if (hediff_Injury4 != null)
                                            {
                                                Cure(hediff_Injury4);
                                            }
                                            else
                                            {
                                                Hediff_Injury hediff_Injury5 = FindInjury(usedBy, null);
                                                if (hediff_Injury5 != null)
                                                {
                                                    Cure(hediff_Injury5);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static Hediff FindLifeThreateningHediff(Pawn pawn)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].Visible && hediffs[i].def.everCurableByItem && !hediffs[i].FullyImmune())
                {
                    HediffStage curStage = hediffs[i].CurStage;
                    bool flag = curStage?.lifeThreatening ?? false;
                    bool flag2 = hediffs[i].def.lethalSeverity >= 0f && hediffs[i].Severity / hediffs[i].def.lethalSeverity >= 0.8f;
                    if (flag || flag2)
                    {
                        float num2 = (hediffs[i].Part == null) ? 999f : hediffs[i].Part.coverageAbsWithChildren;
                        if (hediff == null || num2 > num)
                        {
                            hediff = hediffs[i];
                            num = num2;
                        }
                    }
                }
            }
            return hediff;
        }
        private static void Cure(Hediff hediff)
        {
            Pawn pawn = hediff.pawn;
            pawn.health.RemoveHediff(hediff);
            if (hediff.def.cureAllAtOnceIfCuredByItem)
            {
                int num = 0;
                while (true)
                {
                    num++;
                    if (num > 10000)
                    {
                        Log.Error("Too many iterations.", false);
                    }
                    else
                    {
                        Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def, false);
                        if (firstHediffOfDef != null)
                        {
                            pawn.health.RemoveHediff(firstHediffOfDef);
                            continue;
                        }
                    }
                    break;
                }
            }
            Messages.Message("MessageHediffCuredByItem".Translate(hediff.LabelBase.CapitalizeFirst()), pawn, MessageTypeDefOf.PositiveEvent, true);
        }
        private static Hediff FindMostBleedingHediff(Pawn pawn)
        {
            float num = 0f;
            Hediff hediff = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].Visible && hediffs[i].def.everCurableByItem)
                {
                    float bleedRate = hediffs[i].BleedRate;
                    if (bleedRate > 0f && (bleedRate > num || hediff == null))
                    {
                        num = bleedRate;
                        hediff = hediffs[i];
                    }
                }
            }
            return hediff;
        }
        private static Hediff_Injury FindPermanentInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            Hediff_Injury hediff_Injury = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff_Injury hediff_Injury2 = hediffs[i] as Hediff_Injury;
                if (hediff_Injury2 != null && hediff_Injury2.Visible && hediff_Injury2.IsPermanent() && hediff_Injury2.def.everCurableByItem && (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part)) && (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity))
                {
                    hediff_Injury = hediff_Injury2;
                }
            }
            return hediff_Injury;
        }
        private static BodyPartRecord FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
        {
            BodyPartRecord bodyPartRecord = null;
            foreach (Hediff_MissingPart missingPartsCommonAncestor in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (!(missingPartsCommonAncestor.Part.coverageAbsWithChildren < minCoverage) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(missingPartsCommonAncestor.Part) && (bodyPartRecord == null || missingPartsCommonAncestor.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren))
                {
                    bodyPartRecord = missingPartsCommonAncestor.Part;
                }
            }
            return bodyPartRecord;
        }
        private static void Cure(BodyPartRecord part, Pawn pawn)
        {
            pawn.health.RestorePart(part, null, true);
            Messages.Message("MessageBodyPartCuredByItem".Translate(part.LabelCap), pawn, MessageTypeDefOf.PositiveEvent, true);
        }
        private static Hediff FindImmunizableHediffWhichCanKill(Pawn pawn)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].Visible && hediffs[i].def.everCurableByItem && hediffs[i].TryGetComp<HediffComp_Immunizable>() != null && !hediffs[i].FullyImmune() && CanEverKill(hediffs[i]))
                {
                    float severity = hediffs[i].Severity;
                    if (hediff == null || severity > num)
                    {
                        hediff = hediffs[i];
                        num = severity;
                    }
                }
            }
            return hediff;
        }
        private static Hediff FindNonInjuryMiscBadHediff(Pawn pawn, bool onlyIfCanKill)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].Visible && hediffs[i].def.isBad && hediffs[i].def.everCurableByItem && !(hediffs[i] is Hediff_Injury) && !(hediffs[i] is Hediff_MissingPart) && !(hediffs[i] is Hediff_Addiction) && !(hediffs[i] is Hediff_AddedPart) && (!onlyIfCanKill || CanEverKill(hediffs[i])))
                {
                    float num2 = (hediffs[i].Part == null) ? 999f : hediffs[i].Part.coverageAbsWithChildren;
                    if (hediff == null || num2 > num)
                    {
                        hediff = hediffs[i];
                        num = num2;
                    }
                }
            }
            return hediff;
        }
        private static Hediff_Injury FindInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            Hediff_Injury hediff_Injury = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff_Injury hediff_Injury2 = hediffs[i] as Hediff_Injury;
                if (hediff_Injury2 != null && hediff_Injury2.Visible && hediff_Injury2.def.everCurableByItem && (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part)) && (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity))
                {
                    hediff_Injury = hediff_Injury2;
                }
            }
            return hediff_Injury;
        }
        private static Hediff_Addiction FindAddiction(Pawn pawn)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff_Addiction hediff_Addiction = hediffs[i] as Hediff_Addiction;
                if (hediff_Addiction != null && hediff_Addiction.Visible && hediff_Addiction.def.everCurableByItem)
                {
                    return hediff_Addiction;
                }
            }
            return null;
        }
        private static bool CanEverKill(Hediff hediff)
        {
            if (hediff.def.stages != null)
            {
                for (int i = 0; i < hediff.def.stages.Count; i++)
                {
                    if (hediff.def.stages[i].lifeThreatening)
                    {
                        return true;
                    }
                }
            }
            return hediff.def.lethalSeverity >= 0f;
        }
    }
}
