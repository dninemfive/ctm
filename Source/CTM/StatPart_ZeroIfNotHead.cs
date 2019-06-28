using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class StatPart_ZeroIfNotHead : StatPart
    {
        private const float eyesFactor = 0.5f, headFactor = 1.0f, otherFactor = 0f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing != null && req.Thing.def.IsApparel && req.Thing.def.apparel != null)val *= HeadFactor(req.Thing.def.apparel);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing != null && req.Thing.def.IsApparel && req.Thing.def.apparel != null)
            {
                float result = HeadFactor(req.Thing.def.apparel);
                return "StatPart_HeadFactor".Translate(BodyPartName(result)) + ": " + result.ToStringPercent();
            }
            return null;
        }

        private float HeadFactor(ApparelProperties props)
        {
            float ret = otherFactor;
            foreach(BodyPartGroupDef bpgd in props.bodyPartGroups)
            {
                if (bpgd.Equals(BodyPartGroupDefOf.Eyes) && ret < eyesFactor) ret = eyesFactor;
                if (bpgd.Equals(BodyPartGroupDefOf.FullHead) || bpgd.Equals(BodyPartGroupDefOf.UpperHead)) return headFactor;
            }
            return ret;
        }

        private string BodyPartName(float f)
        {
            if (f < eyesFactor) return "D9NotHead".Translate();
            if (f < headFactor) return "D9Eyes".Translate();
            return "D9Head".Translate();
        }
    }
}
