using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace D9CTM
{
    class Apparel_Offset : Apparel
    {
        public override float GetSpecialApparelScoreOffset()
        {
            float ret = 0f;
            foreach (ThingComp comp in base.AllComps) if (comp is CompApparelScoreOffset cwao) ret += cwao.ApparelScoreOffset;
            return ret;
        }
    }
    class CompApparelScoreOffset : ThingComp
    {
        public CompProperties_ApparelScoreOffset Props => (CompProperties_ApparelScoreOffset)base.props;

        public virtual float ApparelScoreOffset => base.parent.GetStatValue(Props.apparelScoreOffsetStat) * Props.apparelScoreFactor;
    }
    class CompProperties_ApparelScoreOffset : CompProperties
    {
#pragma warning disable CS0649
        public float apparelScoreFactor = 0.25f;
        public StatDef apparelScoreOffsetStat;
#pragma warning restore CS0649
        public CompProperties_ApparelScoreOffset()
        {
            base.compClass = typeof(CompApparelScoreOffset);
        }
    }
}
