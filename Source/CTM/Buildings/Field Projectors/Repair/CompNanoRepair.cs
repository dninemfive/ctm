using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace D9CTM
{
    /*
     * TODO:
     *  - make repairHP actually matter
     */
    class CompNanoRepair : ThingComp
    {
        private List<Thing> allAffectedThings = new List<Thing>();
        private List<Thing> affectedNonPawns = new List<Thing>();
        private List<Pawn> affectedPawns = new List<Pawn>();
        private Vector3 center => GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.Size, parent.def.Altitude);
        private CompProperties_NanoRepair Props => (CompProperties_NanoRepair)props;
        private float radius => Props.radius;
        private float repairRate => Props.repairHP10PerHour;
        private bool canRepair => repairRate > 0;
        private float healRate => Props.healHP10PerHour;
        private bool canHeal => healRate > 0;
        private int ticks => Props.rareTicksBetweenPulses;
        private int ticksTo;
        private bool useTicks => ticks > 0;
        public CompRefuelable fuel;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            fuel = base.parent.GetComp<CompRefuelable>();
            if(!respawningAfterLoad)ticksTo = 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksTo, "ticksTo", 0);
        }

        public bool CanBeActive
        {
            get
            {
                CompPowerTrader trader = base.parent.TryGetComp<CompPowerTrader>();
                return ((trader == null) || (trader != null && trader.PowerOn)) && ((fuel == null) || (fuel != null && fuel.HasFuel));
            }
        }
        //Look at WorkGiver_DoBill.TryFindBestBillIngredients to optimize
        private void getAffectedThings()
        {
            allAffectedThings.Clear();
            affectedPawns.Clear();
            foreach (Thing t in base.parent.Map.listerThings.AllThings)
            {
                if (Vector3.Distance(GenThing.TrueCenter(t.Position, t.Rotation, t.def.Size, t.def.Altitude), center) <= radius)
                {
                    Pawn p = t as Pawn;
                    if (t.def.useHitPoints || p != null)
                    {
                        allAffectedThings.Add(t);
                    }
                    else if (p != null) affectedPawns.Add(p);
                }
            }
        }

        public override void CompTickRare()
        {
            if (ticksTo <= 0)
            {
                if (CanBeActive)
                {
                    if (!useTicks || (useTicks && ticksTo <= 0))
                    {
                        getAffectedThings();
                        allAffectedThings.Shuffle();
                        if (canRepair && allAffectedThings.Count > 0)
                        {
                            foreach (Thing t in allAffectedThings)
                            {
                                Pawn p = t as Pawn;
                                ThingOwner o = t.TryGetInnerInteractableThingOwner();
                                if (p == null && t.HitPoints < t.MaxHitPoints)
                                {
                                    if(ConsumeFuel()) t.HitPoints++;
                                }
                                if (o != null && p == null && o.Owner != null)
                                {
                                    foreach (Thing t2 in ThingOwnerUtility.GetAllThingsRecursively(o.Owner)) if (t2.def.useHitPoints && t2.HitPoints < t2.MaxHitPoints)
                                        {
                                            if(ConsumeFuel())t2.HitPoints++;
                                        }
                                }
                                else if (p != null) TryHealPawnThings(p);
                            }
                        }
                        if (canHeal && affectedPawns.Count > 0)
                        {
                            foreach (Pawn p in affectedPawns) DoMinorHeal(p);
                        }
                    }
                }
                ticksTo = ticks;
            }
            else ticksTo--;
        }

        private void TryHealPawnThings(Pawn p)
        {
            Pawn_ApparelTracker apparel = p.apparel;
            if (apparel != null) foreach (Thing t in apparel.WornApparel) if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                    {
                        if (ConsumeFuel()) t.HitPoints++;
                    }
            Pawn_EquipmentTracker equipment = p.equipment;
            if (equipment != null) foreach (Thing t in equipment.AllEquipmentListForReading) if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                    {
                        if (ConsumeFuel()) t.HitPoints++;
                    }
            Pawn_InventoryTracker inventory = p.inventory;
            if (inventory != null) foreach (Thing t in inventory.innerContainer) if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                    {
                        if (ConsumeFuel()) t.HitPoints++;
                    }
            Pawn_CarryTracker carry = p.carryTracker;
            if (carry != null) foreach (Thing t in carry.innerContainer) if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                    {
                        if (ConsumeFuel()) t.HitPoints++;
                    }      
        }

        private void TryConsumeFuel()
        {
            if (fuel != null) fuel.ConsumeFuel(fuel.Props.fuelConsumptionRate);
        }

        private bool ConsumeFuel()
        {
            if (fuel == null) return true;
            if (fuel.Fuel < fuel.Props.fuelConsumptionRate) return false;
            fuel.ConsumeFuel(fuel.Props.fuelConsumptionRate);
            return true;
        }

        public void DoMinorHeal(Pawn p)
        {
            foreach (Hediff_Injury hi in HealingUtility.GetNonPermanentInjuries(p))
            {
                if (hi != null && ConsumeFuel())
                {
                    hi.Severity -= healRate;
                    Log.Message("healed " + p.LabelShort);
                }
            }
        }
    }
}