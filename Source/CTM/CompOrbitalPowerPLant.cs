using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace D9CTM
{
    /*   TODO:
     *   - Make pawns remove fuel when target less than current
     *      - Gizmo to eject all excess fuel
     *      - context menu to have a pawn take one targeter
     *   - Take over heat output from CompHeatPusher?
     */
    class CompOrbitalPowerPlant : CompPowerPlant
    {
        private CompOPRBeam beam = null; 
        private readonly int TickRareNum = GenDate.TicksPerHour/10;
        private bool active
        {
            get
            {
                bool broken = breakdownableComp != null && breakdownableComp.BrokenDown;
                bool fueled = refuelableComp != null && refuelableComp.HasFuel;
                bool on = flickableComp != null && flickableComp.SwitchIsOn;
                return !broken && !roofed && fueled && on && base.PowerOn;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            beam = base.parent.GetComp<CompOPRBeam>();
        }
        
        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % TickRareNum == 0) DoBeam();
        }

        public void DoBeam()
        {
            if (beam != null) beam.ShouldBeActive = active;
        }

        protected override float DesiredPowerOutput
        {
            get
            {
                if(active) return 0f - (refuelableComp.Fuel * base.Props.basePowerConsumption);
                return 0f;
            }
        }

        public bool roofed
        {
            get
            {
                foreach (IntVec3 cell in base.parent.OccupiedRect()) if (base.parent.Map.roofGrid.Roofed(cell)) return true;
                return false;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn p)
        {
            if (refuelableComp != null)
            {
                float fuel = refuelableComp.Fuel;
                if (fuel >= 1f)
                {
                    yield return FMO_TakeOne();
                    if (fuel > refuelableComp.TargetFuelLevel) yield return FMO_EmptyToTarget();
                }
            }
        }

        private FloatMenuOption FMO_TakeOne()
        {
            ThingDef defReceived = refuelableComp.Props.fuelFilter.AllowedThingDefs.ElementAt(0);// ?? ThingDefOf.Beer; //just to prevent thrown errors, I guess
            Action act = delegate
            {
                refuelableComp.ConsumeFuel(1f);
                ThingOwner<Thing> owner = new ThingOwner<Thing>();
                Thing t = ThingMaker.MakeThing(defReceived, null);
                owner.TryAdd(t);
                if(owner.Count > 0)owner.TryDrop(owner[0], base.parent.Position, base.parent.Map, ThingPlaceMode.Near, out t, null, null);
            };
            return new FloatMenuOption("D9TakeOneOPR".Translate(defReceived.label), act);
        }

        private FloatMenuOption FMO_EmptyToTarget()
        {
            ThingDef defReceived = refuelableComp.Props.fuelFilter.AllowedThingDefs.ElementAt(0);
            Action act = delegate
            {
                float toTake = Mathf.RoundToInt(refuelableComp.Fuel - refuelableComp.TargetFuelLevel);
                ThingOwner<Thing> owner = new ThingOwner<Thing>();
                for (int i = 0; i < toTake; i++)
                {
                    Thing t = ThingMaker.MakeThing(defReceived, null);
                    owner.TryAdd(t);
                }
                refuelableComp.ConsumeFuel(toTake);
                Thing last;
                do
                {
                    if (owner.Count <= 0) return;
                }
                while (owner.TryDrop(owner[0], base.parent.Position, base.parent.Map, ThingPlaceMode.Near, out last, null, null));
            };
            return new FloatMenuOption("D9EmptyToTargetOPR".Translate(), act);
        }
    }
}
