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
#pragma warning disable CS0649        
        public static ThingDef SatelliteLeaving;
        public static ThingDef Filth_HealingFoam;
        public static ThingDef D9Archotech;
        public static HediffDef D9NanComa;
        public static HediffDef D9DisabledPart;
        public static JobDef d9TakeOneFuel;
        public static JobDef d9UnloadExcessFuel;
        public static JobDef d9EnterPod;
        public static ThingSetMakerDef AsteroidMinerReturns;
        //public static StatDef D9NaniteRepairRateBuilding;
        //public static StatDef D9NaniteHealRateBuilding;
        //public static StatDef D9NaniteRepairRate;
        //public static StatDef D9NaniteHealRate;

        static CTMDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CTMDefOf));
        }
    }
}
