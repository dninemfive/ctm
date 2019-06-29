using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace D9CTM
{
    class ThoughtWorker_BrainChipHappiness : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if(HasBrainChip(p) && ArchotechUtility.ThoughtStage != ArchotechUtility.nullThoughtStageIndex) return ThoughtState.ActiveAtStage(ArchotechUtility.ThoughtStage);
            return ThoughtState.Inactive;
        }
        public bool HasBrainChip(Pawn p)
        {
            foreach (Hediff h in p.health.hediffSet.hediffs)
            {
                if (h as HediffWithComps != null && h.def.HasComp(typeof(HediffComp_BrainChip))) return true;
            }
            return false;
        }
    }
}
