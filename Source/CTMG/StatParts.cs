using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace D9CTM
{
    /// <summary>
    /// Either sets a stat's base value to the base value of a specified statDef on the request's Thing, or if not found sets it to a specified default value.
    /// 
    /// Intended for use by Apparel_PsychicSensitivity, so that Psychic Foil works as intended (i.e. increases psychic sensitivity if set to, and otherwise reduces it)
    /// </summary>
    public class StatPart_OffsetPlusBaseValue : StatPart
    {
# pragma warning disable CS0649
        // The StatDef base to look for
        StatDef stat;        
        // The value to return if not found and overwriteValue = false
        float baseVal;
# pragma warning restore CS0649

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing)
            {
                val = baseVal;
                return;
            }
            val = getVal(req.Thing);
        }

        public float getVal(Thing t)
        {
            if (t == null) return baseVal;
            foreach (StatModifier sm in t.def.statBases) if (sm.stat == stat) return Mathf.Sign(sm.value) * (Mathf.Abs(sm.value) + Mathf.Abs(baseVal));
            return baseVal;
        }

        public override string ExplanationPart(StatRequest req)
        {
            return "D9StatPart_BaseValue".Translate(getVal(req.Thing));
        }
    }
}
