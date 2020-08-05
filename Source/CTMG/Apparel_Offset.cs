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
            foreach (ThingComp comp in base.AllComps) if (comp is CompWithApparelOffset cwao) ret += cwao.ApparelScoreOffset;
            return ret;
        }
    }
    abstract class CompWithApparelOffset : ThingComp
    {
        public abstract float ApparelScoreOffset { get; set; }
    }
}
