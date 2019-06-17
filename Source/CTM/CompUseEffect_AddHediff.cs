using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompUseEffect_AddHediff : CompUseEffect
    {
        public CompProperties_UseEffect_AddHediff Props => (CompProperties_UseEffect_AddHediff)base.props;
        public List<BodyPartDef> parts => Props.partsToAddTo;
        public HediffDef hediff => Props.hediffToAdd;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy != null)
            {
                Pawn_HealthTracker health = usedBy.health;
                if (parts != null)
                {
                    foreach (BodyPartDef bpd in parts)
                    {
                        IEnumerable<BodyPartRecord> parts2 = usedBy.def.race.body.GetPartsWithDef(bpd);
                        foreach (BodyPartRecord bpr in parts2)
                        {
                            if (!health.hediffSet.PartIsMissing(bpr)) health.AddHediff(hediff, bpr);
                        }
                    }
                }
                else health.AddHediff(hediff);                
            }
        }
    }
}
