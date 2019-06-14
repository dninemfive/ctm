using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class IncidentWorker_AsteroidMinerReturns
    {
        private int maxValue = 1000;
        public List<ThingDef> GenReturnSet()
        {
            List<ThingDef> temp = new List<ThingDef>();
            while (valueOf(temp) < maxValue) temp.Add(DefDatabase<ThingDef>.AllDefs.RandomElementByWeightWithFallback(delegate (ThingDef d)
             {
                 if (d.building == null || (d.building.mineableThing != null && d.building.mineableThing.BaseMarketValue > maxValue))
                 {
                     return 0;
                 }
                 return d.building.mineableScatterCommonality;
             }, null));
            //get things from mineableThing, stack them into ThingCounts
        }
    }
}
