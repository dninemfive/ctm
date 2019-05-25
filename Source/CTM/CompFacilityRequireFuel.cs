using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace D9CTM
{
    //identical to CompFacility but checks for fuel as well as power; boolean code (hopefully) optimized
    class CompFacilityRequireFuel : ThingComp
    {
        private List<Thing> linkedBuildings = new List<Thing>();
        private HashSet<Thing> thingsToNotify = new HashSet<Thing>();
        public CompProperties_FacilityRequireFuel Props => (CompProperties_FacilityRequireFuel)props;
        public static void DrawLinesToPotentialThingsToLinkTo(ThingDef myDef, IntVec3 myPos, Rot4 myRot, Map map)
        {
            CompProperties_FacilityRequireFuel compProperties = myDef.GetCompProperties<CompProperties_FacilityRequireFuel>();
            Vector3 a = GenThing.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
            for (int i = 0; i < compProperties.linkableBuildings.Count; i++)
            {
                foreach (Thing item in map.listerThings.ThingsOfDef(compProperties.linkableBuildings[i]))
                {
                    CompAffectedByFacilities compAffectedByFacilities = item.TryGetComp<CompAffectedByFacilities>();
                    if (compAffectedByFacilities != null && compAffectedByFacilities.CanPotentiallyLinkTo(myDef, myPos, myRot))
                    {
                        GenDraw.DrawLineBetween(a, item.TrueCenter());
                        compAffectedByFacilities.DrawRedLineToPotentiallySupplantedFacility(myDef, myPos, myRot);
                    }
                }
            }
        }
        public void Notify_NewLink(Thing thing)
        {
            for (int i = 0; i < linkedBuildings.Count; i++)
            {
                if (linkedBuildings[i] == thing)
                {
                    Log.Error("Notify_NewLink was called but the link is already here.", false);
                    return;
                }
            }
            linkedBuildings.Add(thing);
        }
        public void Notify_LinkRemoved(Thing thing)
        {
            for (int i = 0; i < linkedBuildings.Count; i++)
            {
                if (linkedBuildings[i] == thing)
                {
                    linkedBuildings.RemoveAt(i);
                    return;
                }
            }
            Log.Error("Notify_LinkRemoved was called but there is no such link here.", false);
        }
        public void Notify_LOSBlockerSpawnedOrDespawned()
        {
            RelinkAll();
        }
        public void Notify_ThingChanged()
        {
            RelinkAll();
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            LinkToNearbyBuildings();
        }
        public override void PostDeSpawn(Map map)
        {
            thingsToNotify.Clear();
            for (int i = 0; i < linkedBuildings.Count; i++)
            {
                thingsToNotify.Add(linkedBuildings[i]);
            }
            UnlinkAll();
            foreach (Thing item in thingsToNotify)
            {
                item.TryGetComp<CompAffectedByFacilities>().Notify_FacilityDespawned();
            }
        }
        public override void PostDrawExtraSelectionOverlays()
        {
            for (int i = 0; i < linkedBuildings.Count; i++)
            {
                CompAffectedByFacilities compAffectedByFacilities = linkedBuildings[i].TryGetComp<CompAffectedByFacilities>();
                if (compAffectedByFacilities.IsFacilityActive(base.parent))
                {
                    GenDraw.DrawLineBetween(base.parent.TrueCenter(), linkedBuildings[i].TrueCenter());
                }
                else
                {
                    GenDraw.DrawLineBetween(base.parent.TrueCenter(), linkedBuildings[i].TrueCenter(), CompAffectedByFacilities.InactiveFacilityLineMat);
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            if (Props.statOffsets == null)
            {
                return null;
            }
            bool flag = AmIActiveForAnyone();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < Props.statOffsets.Count; i++)
            {
                StatModifier statModifier = Props.statOffsets[i];
                StatDef stat = statModifier.stat;
                stringBuilder.Append(stat.LabelCap);
                stringBuilder.Append(": ");
                stringBuilder.Append(statModifier.value.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset));
                if (!flag)
                {
                    stringBuilder.Append(" (");
                    stringBuilder.Append("InactiveFacility".Translate());
                    stringBuilder.Append(")");
                }
                if (i < Props.statOffsets.Count - 1)
                {
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString();
        }
        private void RelinkAll()
        {
            LinkToNearbyBuildings();
        }
        private void LinkToNearbyBuildings()
        {
            UnlinkAll();
            if (Props.linkableBuildings != null)
            {
                for (int i = 0; i < Props.linkableBuildings.Count; i++)
                {
                    foreach (Thing item in base.parent.Map.listerThings.ThingsOfDef(Props.linkableBuildings[i]))
                    {
                        CompAffectedByFacilities compAffectedByFacilities = item.TryGetComp<CompAffectedByFacilities>();
                        if (compAffectedByFacilities != null && compAffectedByFacilities.CanLinkTo(base.parent))
                        {
                            linkedBuildings.Add(item);
                            compAffectedByFacilities.Notify_NewLink(base.parent);
                        }
                    }
                }
            }
        }
        private bool AmIActiveForAnyone()
        {
            for (int i = 0; i < linkedBuildings.Count; i++)
            {
                CompAffectedByFacilities compAffectedByFacilities = linkedBuildings[i].TryGetComp<CompAffectedByFacilities>();
                if (compAffectedByFacilities.IsFacilityActive(base.parent))
                {
                    return true;
                }
            }
            return false;
        }
        private void UnlinkAll()
        {
            for (int i = 0; i < linkedBuildings.Count; i++)
            {
                linkedBuildings[i].TryGetComp<CompAffectedByFacilities>().Notify_LinkRemoved(base.parent);
            }
            linkedBuildings.Clear();
        }
        public bool CanBeActive {
            get
            {
                CompPowerTrader trader = base.parent.TryGetComp<CompPowerTrader>();
                CompRefuelable fuelable = base.parent.TryGetComp<CompRefuelable>();
                return ((trader == null) || (trader != null && trader.PowerOn)) && ((fuelable == null) || (fuelable != null && fuelable.HasFuel));
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            CompRefuelable fuel = parent.TryGetComp<CompRefuelable>();
            if (linkedBuildings.Count > 0 && fuel != null) if (linkedBuildings.Count > 0 && fuel != null) fuel.ConsumeFuel(fuel.Props.fuelConsumptionRate);
        }
    }
}
