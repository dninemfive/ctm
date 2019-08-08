using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class PlaceWorker_AllowOnlyOne : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            /*
            if (map.listerBuildings.ColonistsHaveBuilding(checkingDef as ThingDef)) return "D9OnlyOne".Translate(checkingDef.label);
            return true;
            */
            if(ArchotechUtility.BuildingOrAnyBlueprintsExist(checkingDef as ThingDef)) return "D9OnlyOne".Translate(checkingDef.label);
            return true;
        }
    }
}
