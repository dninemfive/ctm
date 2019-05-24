using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompProperties_LaunchableSatellite : CompProperties
    {
        public float fuelToLaunch = 1000f;
        public List<ThingDefCountClass> thingsToCreateOnLaunch = null;
        public List<IncidentInfo> Incidents = null; //TODO: weighted list

        public CompProperties_LaunchableSatellite()
        {
            compClass = typeof(CompLaunchableSatellite);
        }
    }
}
