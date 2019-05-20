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
        public List<Thing> thingsToCreateOnLaunch;
        public List<IncidentDef> Incidents; //TODO: weighted list
    }
}
