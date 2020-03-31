using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using Verse;
using RimWorld;
using D9Framework;

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
    /// <summary>
    /// Multiplies the stat if the parent thing is an item of clothing worn on specified body parts.
    /// 
    /// If the apparel covers multiple conflicting body part groups, the chosen multiplier is the one with the greatest abs(1-modifier)
    /// </summary>
    public class StatPart_BodyPartGroupMultiplier : StatPart
    {
        public List<BodyPartGroupMultiplier> multipliers; //not using a Dict<BPGD, float> bc the XML syntax for that is funky

        public class BodyPartGroupMultiplier : IExposable
        {
            public BodyPartGroupDef bpgd;
            public float multiplier;

            public BodyPartGroupMultiplier()
            {
            }

            public BodyPartGroupMultiplier(BodyPartGroupDef b, float f)
            {
                bpgd = b;
                multiplier = f;
            }

            public void ExposeData()
            {
                Scribe_Defs.Look(ref bpgd, "bodyPartGroupDef");
                Scribe_Values.Look(ref multiplier, "multiplier", 1, false);
            }

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                if (xmlRoot.ChildNodes.Count != 1)
                {
                    Log.Error("Misconfigured BodyPartGroupMultiplier: " + xmlRoot.OuterXml, false); //TODO: rewrite ULog and call it here
                }
                else
                {
                    DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "bpgd", xmlRoot.Name, null, null);
                    multiplier = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
                }
            }
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing) return;
            val *= GetBestMultiplier(req.Thing.def);
        }

        // poor performance, needs to be improved. Might just switch to a dict for that reason.
        public float GetBestMultiplier(ThingDef td)
        {
            if (!td.IsApparel) return 1f;
            float greatestDif = 0f, ret = 1f;
            List<BodyPartGroupDef> bpgds = td.apparel.bodyPartGroups;
            foreach (BodyPartGroupMultiplier bpgm in multipliers)
            {
                foreach(BodyPartGroupDef bpgd in bpgds)
                {
                    if (bpgm.bpgd == bpgd)
                    {
                        float dif = Mathf.Abs(bpgm.multiplier - 1f);
                        if (dif > greatestDif)
                        {
                            greatestDif = dif;
                            ret = bpgm.multiplier;
                        }
                    }
                }
            }
            return ret;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !req.Thing.def.IsApparel) return null;
            return "D9MultiplierForBodyPartGroup".Translate();
        }

        // not actually going to return config errors, just taking the opportunity to sort the list for performance
        public override IEnumerable<string> ConfigErrors()
        {
            multipliers.Sort(delegate (BodyPartGroupMultiplier x, BodyPartGroupMultiplier y)
            {
                if (x.multiplier < y.multiplier) return -1;
                if (x.multiplier > y.multiplier) return 1;
                return 0;
            });
            yield break;
        }
    }
}
