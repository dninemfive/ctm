using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class ThingSetMaker_AsteroidMining : ThingSetMaker
    {
        private static List<ThingDef> mineables = new List<ThingDef>();
        private static Dictionary<ThingDef, float> weightedMineables = new Dictionary<ThingDef, float>();

        public List<ThingDefCountClass> guaranteedItems;

        public static void Reset()
        {
            mineables.Clear();
            weightedMineables.Clear();
            mineables.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading where (x.deepCommonality > 0f || HasMineable(x)) && x != ThingDefOf.Chemfuel select x);

        }

        private static bool HasMineable(ThingDef y)
        {
            /*
            IEnumerable<ThingDef> things = from x in DefDatabase<ThingDef>.AllDefsListForReading where (x.building != null) && (x.building.mineableThing == y) && (x.building.mineableScatterCommonality > 0f) select x;
            return things.Any();
            */
            IEnumerable<ThingDef> buildings = from x in DefDatabase<ThingDef>.AllDefsListForReading where x.building != null select x;
            foreach (ThingDef td in buildings) if (td.building.mineableThing == y && td.building.mineableScatterCommonality > 0f) return true;
            return false;
        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            return mineables;
        }
    }
}
