using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace D9CTM
{
    /// <summary>
    /// Removes all nanite hediffs and applies a health penalty (vomiting and reduced blood filtration) for a duration depending on their number and severity.
    /// Nanite hediffs will have to be specified with a DefModExtension afaict.
    /// Hediffs which can never be removed by an item (i.e. luciferium, in vanilla) will cause severe complications very likely leading to death.
    /// </summary>
    class CompUseEffect_PurgeNanites : CompUseEffect
    {
        public CompProperties_UseEffectPurgeNanites Props => (CompProperties_UseEffectPurgeNanites)base.props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (HasConflictingHediff(usedBy)) Messages.Message("D9CTM_PawnHasConflictingHediffs".Translate(usedBy.LabelShort, FirstConflictingHediff(usedBy)), new LookTargets(usedBy), MessageTypeDefOf.CautionInput);
            
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            if (HasNaniteHediff(p))
            {
                failReason = null;
                return true;
            }
            else
            {
                failReason = "D9CTM_PawnHasNoNaniteHediffs".Translate(p.LabelShort);
                return false;
            }
        }        

        public static bool HasNaniteHediff(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.Where(h => h.def.HasModExtension<NaniteHediff>()).Any();
        }

        public static HediffDef FirstConflictingHediff(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                        .Where(h => h.def.HasModExtension<NaniteHediff>() && (!h.def.everCurableByItem || h.def.GetModExtension<NaniteHediff>().isSevere))?
                        .First()?.def;
        }

        public static bool HasConflictingHediff(Pawn pawn)
        {
            return FirstConflictingHediff(pawn) != null;
        }
    }
    class CompProperties_UseEffectPurgeNanites : CompProperties
    {
#pragma warning disable CS0649
        public HediffDef recoveryHediff;
        public HediffDef conflictHediff;
#pragma warning restore CS0649
        public CompProperties_UseEffectPurgeNanites()
        {
            base.compClass = typeof(CompUseEffect_PurgeNanites);
        }
    }
    class NaniteHediff : DefModExtension
    {
        // for use in the case where a mod doesn't set everCurableByItem but the hediff should conflict with this
        public bool isSevere = true;
    }
}
