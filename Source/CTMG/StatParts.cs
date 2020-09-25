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
    /// Multiplies the input value by a stat base, if it exists; otherwise does nothing
    /// 
    /// Intended for use by Apparel_PsychicSensitivity, so that Psychic Foil works as intended (i.e. increases psychic sensitivity if the apparel in question should do so, and otherwise reduces it)
    /// </summary>
    /// <remarks>Might make it just multiply by the sign of the offset in the future.</remarks>
    public class StatPart_MultiplyByOffset : StatPart
    {
# pragma warning disable CS0649
        // The StatDef base to look for
        StatDef stat;
# pragma warning restore CS0649

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing) return;
            float? f;
            if((f = GetVal(req.Thing)) != null) val *= (float)f;
        }
        
        public float? GetVal(Thing t)
        {
            if (t == null) return null;
            foreach (StatModifier sm in t?.def.statBases) if (sm.stat == stat) return sm.value;
            return null;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing) return null;
            float? f = GetVal(req.Thing);
            if (f == null) return null;
            return "D9CTM_StatPart_BaseValue".Translate((float)f);
        }
    }
    /// <summary>
    /// Multiplies the stat if the parent thing is an item of clothing worn on specified body parts.
    /// 
    /// If the apparel covers multiple conflicting body part groups, the chosen multiplier is the one with the greatest abs(1-modifier)    /// 
    /// </summary>
    /// <remarks>Ignores any apparel with the parent stat explicitly set, e.g. Royalty clothing with PsychicSensitivityOffset, because that's already being factored in</remarks>
    public class StatPart_BodyPartGroupMultiplier : StatPart
    {
#pragma warning disable CS0649
        public List<BodyPartGroupMultiplier> multipliers; //not using a Dict<BPGD, float> bc the XML syntax for that is funky
        float @default = 1f; //@ is just for XML clarity
#pragma warning restore CS0649

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

            public override bool Equals(object obj)
            {
                var multiplier = obj as BodyPartGroupMultiplier;
                return multiplier != null &&
                       EqualityComparer<BodyPartGroupDef>.Default.Equals(bpgd, multiplier.bpgd);
            }

            public void ExposeData()
            {
                Scribe_Defs.Look(ref bpgd, "bodyPartGroupDef");
                Scribe_Values.Look(ref multiplier, "multiplier", 1, false);
            }

            public override int GetHashCode()
            {
                return 1680496116 + EqualityComparer<BodyPartGroupDef>.Default.GetHashCode(bpgd);
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

        private Dictionary<BodyPartGroupDef, float> multDict; //can technically be written to directly but don't do that please

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || req.Thing.def.equippedStatOffsets.Any(x => x.stat == parentStat)) return;
            val *= GetBestMultiplier(req.Thing.def);
        }
        
        public float GetBestMultiplier(ThingDef td)
        {
            if (!td.IsApparel) return @default;
            float greatestDif = 0f, ret = @default;
            List<BodyPartGroupDef> bpgds = td.apparel.bodyPartGroups;
            foreach(BodyPartGroupDef bpgd in bpgds)
            {
                // if (!multDict.ContainsKey(bpgd)) continue; // No longer necessary since all body part groups should:tm: be added to the dictionary during initialization
                // Hello future me. You're here because someone reported an NRE here. Just null-check your dictionary inputs and you should be good. 
                // Might also want to handle the case where mods improperly generate BodyPartGroupDefs late.
                float dif = Mathf.Abs(multDict[bpgd]);
                if (dif > greatestDif)
                {
                    greatestDif = dif;
                    ret = multDict[bpgd];
                }
            }
            return ret;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !req.Thing.def.IsApparel) return null;
            return "D9CTM_MultForBPGD".Translate(GetBestMultiplier(req.Thing.def));
        }

        public override IEnumerable<string> ConfigErrors()
        {
            multDict = new Dictionary<BodyPartGroupDef, float>();
            foreach (BodyPartGroupMultiplier bpgm in multipliers) multDict.Add(bpgm.bpgd, bpgm.multiplier);
            if (multipliers.Count > multDict.Count) yield return "D9CTM_BPGM_HashCollision".Translate();
            foreach (BodyPartGroupDef bpgd in DefDatabase<BodyPartGroupDef>.AllDefsListForReading) if (!multDict.ContainsKey(bpgd)) multDict.Add(bpgd, @default);            
        }
    }
}
