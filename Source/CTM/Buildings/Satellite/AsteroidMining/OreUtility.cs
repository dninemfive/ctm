using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace D9CTM
{
    //Thanks Mehni!
    [StaticConstructorOnStartup]
    public class OreUtility
    {
        public static List<ThingDef> ores;
        public static Dictionary<ThingDef, float> weightedMineables;
        private static Dictionary<ThingDef, ThingDef> mineable;
        
        static OreUtility()
        {
            ores = new List<ThingDef>();
            weightedMineables = new Dictionary<ThingDef, float>();
            mineable = new Dictionary<ThingDef, ThingDef>();
            GetMineableBuildings();
            GetOres();
        }

        private static void GetOres()
        {
            ores.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading where (x.deepCommonality > 0f || HasMineable(x)) && x != ThingDefOf.Chemfuel && x != ThingDefOf.ComponentIndustrial select x);
            foreach (ThingDef td in ores)
            {
                float weight = td.deepCommonality; //TODO: see if deepCommonality is approximately proportional to mineableScatterCommonality and weight accordingly, incorporate lump sizes for both
                if (mineable.ContainsKey(td)) weight += mineable.TryGetValue(td).building.mineableScatterCommonality;
                weightedMineables.Add(td, Mathf.Sqrt(weight)); //sqrt is to emphasize commonality of rare resources
            }
        }

        private static bool HasMineable(ThingDef td)
        {
            return mineable.ContainsKey(td);
        }

        private static void GetMineableBuildings()
        {
            foreach (ThingDef td in DefDatabase<ThingDef>.AllDefsListForReading) if (td.building != null && td.building.mineableThing != null && td.building.mineableScatterCommonality > 0f) mineable.Add(td.building.mineableThing, td);
        }
    }
}
