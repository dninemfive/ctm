using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class Hediff_BioAug : HediffWithComps
    {
        public override bool ShouldRemove
        {
            get
            {
                //if apparently body parts replaced with Hediff_AddedPart count as removed so no other checks needed
                return !base.pawn.health.hediffSet.PartIsMissing(base.Part);
            }
        }
    }
}
