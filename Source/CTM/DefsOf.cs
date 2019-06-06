using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace D9CTM
{
    [DefOf]
    class CTMDefOf
    {
        public static StatDef D9NaniteRepairRate;
        public static StatDef D9NaniteHealRate;
        public static ThingDef SatelliteLeaving;
        public static ThingDef Filth_HealingFoam;
        public static HediffDef D9NanComa;
        public static JobDef d9TakeOneFuel;
        public static JobDef d9UnloadExcessFuel;
        //public static StatDef D9NaniteRepairRateBuilding;
        //public static StatDef D9NaniteHealRateBuilding;

        static CTMDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CTMDefOf));
        }
    }
}
