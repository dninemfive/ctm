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
     *  - scale fuel consumption on heal with number of injuries healed (make TryHeal return a float)
     *  - prevent healing once fuel is exhausted (make TryConsumeFuel return a bool)
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
                        if (canRepair && allAffectedThings.Count > 0)
                        {
                            foreach (Thing t in allAffectedThings)
                            {
                                Pawn p = t as Pawn;
                                ThingOwner o = t.TryGetInnerInteractableThingOwner();
                                if (p == null && t.HitPoints < t.MaxHitPoints)
                                {
                                    t.HitPoints++;
                                    TryConsumeFuel(); //note: will continue to heal within a pulse even when out of fuel, but will not function thereafter. TODO: make TryConsumeFuel return whether fuel is empty and use if-statements
                                }
                                if (o != null && p == null && o.Owner != null)
                                {
                                    //foreach (Thing t2 in ThingOwnerUtility.GetAllThingsRecursively(o.Owner)) Log.Message("" + t);
                                    //for (int i = 0; i < o.Count; i++) if (o[i].def.useHitPoints && o[i].HitPoints < o[i].MaxHitPoints) o[i].HitPoints++;
                                    foreach (Thing t2 in ThingOwnerUtility.GetAllThingsRecursively(o.Owner)) if (t2.def.useHitPoints && t2.HitPoints < t2.MaxHitPoints)
                                        {
                                            t2.HitPoints++;
                                            TryConsumeFuel();
                                        }
                                }
                                else if (p != null) TryHealPawnThings(p);
                            }
                        }
                        if (canHeal && affectedPawns.Count > 0)
                        {
                            foreach (Pawn p in affectedPawns)
                            {
                                HealingUtility.PartiallyHealAllNonPermInjuries(p, healRate / 100f);
                                TryConsumeFuel();
                            }
                        }
                    }
                }
                ticksTo = ticks;
            }
            else ticksTo--;
        }

        private void TryHealPawnThings(Pawn p)
        {
            Pawn_InventoryTracker inventory = p.inventory;
            if (inventory != null) foreach (Thing t in inventory.innerContainer) if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                    {
                        t.HitPoints++;
                        TryConsumeFuel();
                    }
            Pawn_CarryTracker carry = p.carryTracker;
            if (carry != null) foreach (Thing t in carry.innerContainer) if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                    {
                        t.HitPoints++;
                        TryConsumeFuel();
                    }
            Pawn_EquipmentTracker equipment = p.equipment;
            if (equipment != null) foreach (Thing t in equipment.AllEquipmentListForReading) if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                    {
                        t.HitPoints++;
                        TryConsumeFuel();
                    }
            Pawn_ApparelTracker apparel = p.apparel;
            if (apparel != null) foreach (Thing t in apparel.WornApparel) if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                    {
                        t.HitPoints++;
                        TryConsumeFuel();
                    }
        }

        private void TryConsumeFuel()
        {
            if (fuel != null) fuel.ConsumeFuel(fuel.Props.fuelConsumptionRate);
        }
    }
}