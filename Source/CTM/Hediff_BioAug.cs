using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class Hediff_BioAug : Hediff_AddedPart
    {
        public override bool ShouldRemove
        {
            get
            {
                //apparently body parts replaced with Hediff_AddedPart count as removed so no other checks needed
                return base.pawn.health.hediffSet.PartIsMissing(base.Part) || base.ShouldRemove;
            }
        }
        public override void PostAdd(DamageInfo? dinfo)
        {
            if (comps != null)
            {
                for (int i = 0; i < comps.Count; i++)
                {
                    comps[i].CompPostPostAdd(dinfo);
                }
            }
        }
    }
}
