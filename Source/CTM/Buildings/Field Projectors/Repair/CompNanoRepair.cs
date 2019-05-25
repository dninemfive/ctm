using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace D9CTM
{
    class CompNanoRepair : ThingComp
    {
        private List<Thing> affectedBuildings = new List<Thing>(); //Actually affected things, but whatever
        private List<Pawn> affectedPawns = new List<Pawn>();
        private Vector3 center => GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.Size, parent.def.Altitude);
        private CompProperties_NanoRepair Props => (CompProperties_NanoRepair)props;
        private float radius => Props.radius;
        private float repairRate => Props.repairHP10PerHour;
        private bool canRepair => repairRate > 0;
        private float healRate => Props.healHP10PerHour;
        private bool canHeal => healRate > 0;
        //private int ticksUntilNextRepair = 0, ticksUntilNextHeal = 0;

        public bool CanBeActive
        {
            get
            {
                CompPowerTrader trader = base.parent.TryGetComp<CompPowerTrader>();
                CompRefuelable fuelable = base.parent.TryGetComp<CompRefuelable>();
                return ((trader == null) || (trader != null && trader.PowerOn)) && ((fuelable == null) || (fuelable != null && fuelable.HasFuel));
            }
        }
        private void getAffectedThings()
        {
            affectedBuildings.Clear();
            affectedPawns.Clear();
            foreach (Thing t in base.parent.Map.listerThings.AllThings)
            {
                if (Vector3.Distance(GenThing.TrueCenter(t.Position, t.Rotation, t.def.Size, t.def.Altitude), center) <= radius)
                {
                    Pawn p = t as Pawn;
                    if (p == null && t.def.useHitPoints)
                    {
                        affectedBuildings.Add(t);
                    }
                    else if (p != null)
                    {
                        affectedPawns.Add(p);
                    }
                }
            }
        }

        public override void CompTickRare()
        {
            if (CanBeActive)
            {
                getAffectedThings();
                if (canRepair && affectedBuildings.Count > 0)
                {
                    foreach (Thing t in affectedBuildings) if (t.HitPoints < t.MaxHitPoints) t.HitPoints++;
                    foreach (Pawn p in affectedPawns) foreach (ThingWithComps t in p.equipment.AllEquipmentListForReading) if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints) t.HitPoints++;
                    //ticksUntilNextRepair = ticksUntilNextRepair = (int)(10 / repairRate); //so a repair rate of 100% equals 1 HP repaired per hour, 50% = 2hr, &c. 
                }
                if (canHeal && affectedPawns.Count > 0)
                {
                    foreach (Pawn p in affectedPawns)
                    {
                        HealingUtility.doMinorHeal(p);
                    }
                    //ticksUntilNextHeal = (int)(2500 / healRate); //so a heal rate of 100% equals one heal pulse per hour, 50% = 2hr, &c
                }
            }
        }
    }
}