using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class IncidentWorker_AsteroidMinerReturnsFrag : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> things = CTMDefOf.D9AsteroidMiner_Frag.root.Generate();
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true, true);
            Find.LetterStack.ReceiveLetter("D9AsteroidMinerLabel".Translate(), "D9AsteroidMinerDescFrag".Translate(), LetterDefOf.PositiveEvent, new TargetInfo(intVec, map, false), null, null);
            return true;
        }
    }
    class IncidentWorker_AsteroidMinerReturnsShac : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> things = CTMDefOf.D9AsteroidMiner_Shaq.root.Generate();
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true, true);
            Find.LetterStack.ReceiveLetter("D9AsteroidMinerLabel".Translate(), "D9AsteroidMinerDescShaq".Translate(), LetterDefOf.PositiveEvent, new TargetInfo(intVec, map, false), null, null);
            return true;
        }
    }
    class IncidentWorker_AsteroidMinerReturnsCore : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> things = CTMDefOf.D9AsteroidMiner_Core.root.Generate();
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true, true);
            Find.LetterStack.ReceiveLetter("D9AsteroidMinerLabel".Translate(), "D9AsteroidMinerDescCore".Translate(), LetterDefOf.PositiveEvent, new TargetInfo(intVec, map, false), null, null);
            return true;
        }
    }
}
