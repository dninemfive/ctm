using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
