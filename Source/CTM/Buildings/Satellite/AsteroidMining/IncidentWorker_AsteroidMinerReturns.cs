using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class IncidentWorker_AsteroidMinerReturns : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> things = CTMDefOf.AsteroidMinerReturns.root.Generate();
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true, true);
            Find.LetterStack.ReceiveLetter("LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.PositiveEvent, new TargetInfo(intVec, map, false), null, null);
            return true;
        }
    }
}
