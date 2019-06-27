using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class StatPart_ApparelStatFactor : StatPart
    {
        private StatDef apparelStat;
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing != null)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && pawn.apparel != null)
                {
                    for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
                    {
                        float statValue = pawn.apparel.WornApparel[i].GetStatValue(apparelStat, true);
                        val *= statValue;
                    }
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing != null)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && PawnWearingRelevantGear(pawn))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
                    if (pawn.apparel != null)
                    {
                        for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
                        {
                            Apparel gear = pawn.apparel.WornApparel[i];
                            stringBuilder.AppendLine(InfoTextLineFrom(gear));
                        }
                    }
                    return stringBuilder.ToString();
                }
            }
            return null;
        }

        private string InfoTextLineFrom(Thing gear)
        {
            float num = gear.GetStatValue(apparelStat, true);
            return "    " + gear.LabelCap + ": " + num.ToStringByStyle(base.parentStat.toStringStyle, ToStringNumberSense.Factor);
        }

        private bool PawnWearingRelevantGear(Pawn pawn)
        {
            if (pawn.apparel != null)
            {
                for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
                {
                    Apparel thing = pawn.apparel.WornApparel[i];
                    if (thing.GetStatValue(apparelStat, true) != 0f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
