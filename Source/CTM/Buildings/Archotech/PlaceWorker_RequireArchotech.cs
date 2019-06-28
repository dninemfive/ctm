using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9CTM
{
    class PlaceWorker_RequireArchotech : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            if(map.listerBuildings.ColonistsHaveBuilding(CTMDefOf.D9Archotech)) return true;
            return "D9RequiresArchotech".Translate();
        }
    }
}
