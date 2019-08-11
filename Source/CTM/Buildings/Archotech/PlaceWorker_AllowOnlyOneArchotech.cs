using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class PlaceWorker_AllowOnlyOneArchotech : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            if (ArchotechUtility.ArchotechOrAnyBlueprintsExist()) return "D9OnlyOne".Translate(checkingDef.label);
            return true;
        }
    }
}
