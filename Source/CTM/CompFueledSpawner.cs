using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompFueledSpawner : ThingComp
    {
        private int ticksToSpawn;
        public CompProperties_Spawner Props => (CompProperties_Spawner)base.props;
        public CompRefuelable fuelable;

        private bool On
        {
            get
            {
                CompPowerTrader pow = base.parent.GetComp<CompPowerTrader>();
                return (pow == null || (pow != null && pow.PowerOn)) && (fuelable != null && fuelable.HasFuel);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            fuelable = base.parent.GetComp<CompRefuelable>();
            if (fuelable == null) Log.Error("[Accessible Archotech] A thing with CompFueledSpawner has no CompRefuelable.");
            if (!respawningAfterLoad)
            {
                Reset();
            }
        }

        public override void CompTick()
        {
            tick(1);
        }

        public override void CompTickRare()
        {
            tick(250);
        }

        private void tick(int i)
        {
            if (base.parent.Spawned)
            {
                if (base.parent.Position.Fogged(base.parent.Map) || !On) return;
                ticksToSpawn -= i;
                CheckShouldSpawn();
            }
        }
        private void CheckShouldSpawn()
        {
            if (ticksToSpawn <= 0)
            {
                if(ConsumeFuel()) Spawn();
                Reset();
            }
        }
        public bool Spawn()
        {
            if (!base.parent.Spawned)
            {
                return false;
            }
            if (Props.spawnMaxAdjacent >= 0)
            {
                int num = 0;
                for (int i = 0; i < 9; i++)
                {
                    IntVec3 c = base.parent.Position + GenAdj.AdjacentCellsAndInside[i];
                    if (c.InBounds(base.parent.Map))
                    {
                        List<Thing> thingList = c.GetThingList(base.parent.Map);
                        for (int j = 0; j < thingList.Count; j++)
                        {
                            if (thingList[j].def == Props.thingToSpawn)
                            {
                                num += thingList[j].stackCount;
                                if (num >= Props.spawnMaxAdjacent)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            if (TryFindSpawnCell(out IntVec3 center))
            {
                Thing thing = ThingMaker.MakeThing(Props.thingToSpawn, null);
                thing.stackCount = Props.spawnCount;
                if (Props.inheritFaction && thing.Faction != base.parent.Faction)
                {
                    thing.SetFaction(base.parent.Faction, null);
                }
                GenPlace.TryPlaceThing(thing, center, base.parent.Map, ThingPlaceMode.Direct, out Thing t, null, null);
                if (Props.spawnForbidden)
                {
                    t.SetForbidden(true, true);
                }
                if (Props.showMessageIfOwned && base.parent.Faction == Faction.OfPlayer)
                {
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(Props.thingToSpawn.LabelCap).CapitalizeFirst(), thing, MessageTypeDefOf.PositiveEvent, true);
                }
                return true;
            }
            return false;
        }
        private bool TryFindSpawnCell(out IntVec3 result)
        {
            foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(base.parent).InRandomOrder(null))
            {
                if (item.Walkable(base.parent.Map))
                {
                    Building edifice = item.GetEdifice(base.parent.Map);
                    if (edifice == null || !Props.thingToSpawn.IsEdifice())
                    {
                        Building_Door building_Door = edifice as Building_Door;
                        if ((building_Door == null || building_Door.FreePassage) && (base.parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(base.parent.Position, item, base.parent.Map, false, null, 0, 0)))
                        {
                            bool flag = false;
                            List<Thing> thingList = item.GetThingList(base.parent.Map);
                            for (int i = 0; i < thingList.Count; i++)
                            {
                                Thing thing = thingList[i];
                                if (thing.def.category == ThingCategory.Item && (thing.def != Props.thingToSpawn || thing.stackCount > Props.thingToSpawn.stackLimit - Props.spawnCount))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                result = item;
                                return true;
                            }
                        }
                    }
                }
            }
            result = IntVec3.Invalid;
            return false;
        }
        private void Reset()
        {
            ticksToSpawn = Props.spawnIntervalRange.RandomInRange;
        }
        private bool ConsumeFuel()
        {
            if (fuelable != null){
                if (fuelable.Fuel >= fuelable.Props.fuelConsumptionRate)
                {
                    fuelable.ConsumeFuel(fuelable.Props.fuelConsumptionRate);
                    return true;
                }
            }
            return false;
        }
        public override void PostExposeData()
        {
            string str = (!Props.saveKeysPrefix.NullOrEmpty()) ? (Props.saveKeysPrefix + "_") : null;
            Scribe_Values.Look(ref ticksToSpawn, str + "ticksToSpawn", 0, false);
        }
        public override string CompInspectStringExtra()
        {
            if (fuelable.Fuel < fuelable.Props.fuelConsumptionRate) return "NotEnoughFuelForSpawner".Translate(Props.thingToSpawn.label);
            if (Props.writeTimeLeftToSpawn && (!Props.requiresPower || On))
            {
                return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(Props.thingToSpawn, null, Props.spawnCount)) + ": " + ticksToSpawn.ToStringTicksToPeriod();
            }
            return null;
        }
    }
}
