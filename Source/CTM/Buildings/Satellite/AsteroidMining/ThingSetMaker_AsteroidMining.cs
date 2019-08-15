using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace D9CTM
{
    class ThingSetMaker_AsteroidMining : ThingSetMaker
    {
        private static List<ThingDef> ores = new List<ThingDef>();
        private static Dictionary<ThingDef, float> weightedMineables = new Dictionary<ThingDef, float>();
        private static Dictionary<ThingDef, ThingDef> mineable = new Dictionary<ThingDef, ThingDef>();

        public List<ThingDefCountClass> guaranteedItems;
        public float amount;

        public static void Reset()
        {
            ores.Clear();
            weightedMineables.Clear();
            ores.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading where (x.deepCommonality > 0f || HasMineable(x)) && x != ThingDefOf.Chemfuel select x);
            foreach(ThingDef td in ores)
            {
                float weight = td.deepCommonality; //TODO: see if deepCommonality is approximately proportional to mineableScatterCommonality and weight accordingly, incorporate lump sizes for both
                if (mineable.ContainsKey(td)) weight += td.building.mineableScatterCommonality;
                weightedMineables.Add(td, Mathf.Sqrt(weight)); //sqrt is to emphasize commonality of rare resources
            }
        }

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            foreach(ThingDef td in ores)
            {
                weightedMineables.TryGetValue(td, out float value);
                int count = (int)(amount * value);
                while(count > 0)
                {
                    Thing nextThing = ThingMaker.MakeThing(td);
                    if (count > td.stackLimit) nextThing.stackCount = td.stackLimit;
                    else nextThing.stackCount = count;
                    outThings.Add(nextThing);
                }
            }
        }

        private static bool HasMineable(ThingDef y)
        {
            foreach (ThingDef td in DefDatabase<ThingDef>.AllDefsListForReading) if (td.building != null && td.building.mineableThing == y && td.building.mineableScatterCommonality > 0f)
                {
                    mineable.Add(y, td);
                    return true;
                }
            return false;
        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            return ores;
        }
    }
}
