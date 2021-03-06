﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;

namespace D9CTM
{
    /// <summary>
    /// Comp class for pods which, when launched, create items.
    /// 
    /// Previously overscoped to include event creation, but asteroid miners will get their own comp in the future.
    /// </summary>
    [StaticConstructorOnStartup]
    class CompLaunchableSatellite : ThingComp
    {
        private static Texture2D LaunchCommandTex;
        public Building FuelingPortSource => FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(base.parent.Position, Map);
        public bool ConnectedToFuelingPort => FuelingPortSource != null;
        public bool FuelingPortSourceHasAnyFuel => ConnectedToFuelingPort && FuelingPortSource.GetComp<CompRefuelable>().HasFuel;
        public bool IsUnderRoof => parent.Position.Roofed(Map);
        public float FuelingPortSourceFuel => !ConnectedToFuelingPort ? 0f : FuelingPortSource.GetComp<CompRefuelable>().Fuel;
        public bool EnoughFuel => FuelingPortSourceFuel >= FuelToLaunch;
        public bool CanLaunch => EnoughFuel && !IsUnderRoof;
        private Map Map => base.parent.Map;

        public CompProperties_LaunchableSatellite Props => (CompProperties_LaunchableSatellite)props;
        public float FuelToLaunch => Props.fuelToLaunch;
        public List<ThingDefCountClass> ToCreate => Props.thingsToCreateOnLaunch;

        public CompLaunchableSatellite()
        {
            LaunchCommandTex = ContentFinder<Texture2D>.Get(Props.launchCommandPath, true);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            Command_Action launch = new Command_Action
            {
                defaultLabel = "D9CTM_CommandLaunchPod".Translate(base.parent.Label),
                defaultDesc = "D9CTM_CommandLaunchPodDesc".Translate(base.parent.Label),
                icon = LaunchCommandTex,
                alsoClickIfOtherInGroupClicked = false,
                action = delegate
                {
                    TryLaunch();
                }
            };
            if (!EnoughFuel)
            {
                launch.Disable("D9CTM_PodUnderfueled".Translate(base.parent.Label));
            }
            else if (IsUnderRoof)
            {
                launch.Disable("D9CTM_PodUnderRoof".Translate(base.parent.Label));
            }
            yield return launch;
        }

        public override string CompInspectStringExtra()
        {
            if (CanLaunch)
            {
                return "D9CTM_PodReadyToLaunch".Translate();
            }
            else
            {
                if (IsUnderRoof) return "D9CTM_PodUnderRoof".Translate(base.parent.Label);
                if (!EnoughFuel) return "D9CTM_PodUnderfueled".Translate(base.parent.Label);
            }
            return null;
        }

        public void TryLaunch()
        {
            if (!CanLaunch)
            {
                Messages.Message("D9CTM_PodNotReady".Translate(base.parent.Label), new LookTargets(base.parent), MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (FuelingPortSource != null) FuelingPortSource.TryGetComp<CompRefuelable>()?.ConsumeFuel(FuelToLaunch);
            Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(Props.skyfallerDef);
            GenSpawn.Spawn(skyfaller, parent.Position, Map, WipeMode.Vanish);
            try
            {
                if (ToCreate != null)
                {
                    ThingOwner<Thing> owner = new ThingOwner<Thing>();
                    foreach (ThingDefCountClass td in ToCreate)
                    {
                        Thing t = ThingMaker.MakeThing(td.thingDef, null);
                        owner.TryAdd(t);
                    }
                    do
                    {
                        if (owner.Count <= 0) return;
                    }
                    while (owner.TryDrop(owner[0], base.parent.Position, Map, ThingPlaceMode.Near, out Thing last, null, null));
                }
            }
            finally
            {
                parent.Destroy(DestroyMode.Vanish);
            }
        }
    }
    class CompProperties_LaunchableSatellite : CompProperties
    {
        public float fuelToLaunch = 1000f;
        public List<ThingDefCountClass> thingsToCreateOnLaunch = null;
        public string launchCommandPath = "UI/Commands/LaunchShip";
#pragma warning disable CS0649
        public ThingDef skyfallerDef;
#pragma warning restore CS0649

        public CompProperties_LaunchableSatellite()
        {
            compClass = typeof(CompLaunchableSatellite);
        }
    }
}