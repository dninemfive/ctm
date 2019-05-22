using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;

namespace D9CTM
{
    [StaticConstructorOnStartup]
    class CompLaunchableSatellite : ThingComp
    {
        public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true); 
        private static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true); //should probably expose in CompProperties
        public Building FuelingPortSource => FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(base.parent.Position, base.parent.Map);
        public bool ConnectedToFuelingPort => FuelingPortSource != null;
        public bool FuelingPortSourceHasAnyFuel => ConnectedToFuelingPort && FuelingPortSource.GetComp<CompRefuelable>().HasFuel;
        public bool IsUnderRoof => parent.Position.Roofed(base.parent.Map);
        public float FuelingPortSourceFuel => !ConnectedToFuelingPort ? 0f : FuelingPortSource.GetComp<CompRefuelable>().Fuel;
        public bool EnoughFuel => FuelingPortSourceFuel >= FuelToLaunch;
        public bool CanLaunch => EnoughFuel && !IsUnderRoof;

        public CompProperties_LaunchableSatellite Props => (CompProperties_LaunchableSatellite)props;
        public float FuelToLaunch => Props.fuelToLaunch;
        public List<ThingDefCountClass> toCreate => Props.thingsToCreateOnLaunch;
        public List<IncidentInfo> incidents => Props.Incidents;
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            Command_Action launch = new Command_Action();
            launch.defaultLabel = "CommandLaunchSatellite".Translate();
            launch.defaultDesc = "CommandLaunchSatelliteDesc".Translate();
            launch.icon = LaunchCommandTex;
            launch.alsoClickIfOtherInGroupClicked = false;
            launch.action = delegate
            {
                TryLaunch();
            };
            if (!EnoughFuel)
            {
                launch.Disable("NotEnoughFuelToLaunchSatellite".Translate());
            }
            else if (IsUnderRoof)
            {
                launch.Disable("SatelliteIsUnderRoof".Translate());
            }
            yield return launch;
        }

        public override string CompInspectStringExtra()
        {
            if (CanLaunch)
            {
                return "ReadyForLaunchSatellite".Translate();
            }
            else
            {
                if (IsUnderRoof) return "SatelliteIsUnderRoof".Translate();
                if (!EnoughFuel) return "NotEnoughFuelToLaunchSatellite".Translate();                                
            }
            return null;
        }

        public void TryLaunch()
        {
            if (!CanLaunch) return; //TODO: throw message
            if (FuelingPortSource != null) FuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel(FuelToLaunch);
            Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(CTMDefOf.SatelliteLeaving);
            parent.Destroy(DestroyMode.Vanish);
            GenSpawn.Spawn(skyfaller, parent.Position, parent.Map, WipeMode.Vanish);
            ThingOwner<Thing> owner = new ThingOwner<Thing>();
            foreach (ThingDefCountClass td in toCreate)
            {
                Thing t = ThingMaker.MakeThing(td.thingDef, null);
                owner.TryAdd(t);
            }
            Thing last;
            do
            {
                if (owner.Count <= 0) return;
            }
            while (owner.TryDrop(owner[0], base.parent.Position, base.parent.Map, ThingPlaceMode.Near, out last, null, null));
            IncidentInfo info = incidents.RandomElementByWeight(x => x.weight);
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(info.def.category, base.parent.Map);
            int day = GenDate.TicksPerDay;
            IntRange delay = new IntRange((int)(day * info.minDelayDays), (int)(day * info.maxDelayDays));
            Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(info.def, null, parms), Find.TickManager.TicksGame + delay.RandomInRange, 0));
        }
    }
}
