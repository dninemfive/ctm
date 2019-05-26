﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    /*   TODO:
     *   - Make pawns remove fuel when target less than current
     *   - Make orbital beam work; remove comp and rewrite, including [StaticConstructorOnStartup]
     */
    class CompOrbitalPowerPlant : CompPowerPlant
    {
        private CompOrbitalBeam beam = null; 
        private readonly int TickRareNum = GenDate.TicksPerHour/10;
        private bool active
        {
            get
            {
                bool broken = breakdownableComp != null && breakdownableComp.BrokenDown;
                bool fueled = refuelableComp != null && refuelableComp.HasFuel;
                bool on = flickableComp != null && flickableComp.SwitchIsOn;
                Log.Message("broken: " + broken + ", fueled: " + fueled + ", on: " + on + ", powerOn: " + base.PowerOn);
                return !broken && !roofed && fueled && on && base.PowerOn;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            beam = base.parent.GetComp<CompOrbitalBeam>();
            Log.Message("beam: " + beam);
        }
        
        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % TickRareNum == 0) DoBeam();
        }

        public void DoBeam()
        {
            if (active && beam != null) beam.StartAnimation(TickRareNum, 0, 0f);
        }

        protected override float DesiredPowerOutput
        {
            get
            {
                return !roofed || refuelableComp == null ? 0 : 0f - (refuelableComp.Fuel * base.Props.basePowerConsumption);
            }
        }

        public bool roofed
        {
            get
            {
                foreach (IntVec3 cell in base.parent.OccupiedRect()) if (base.parent.Map.roofGrid.Roofed(cell)) return false;
                return true;
            }
        }
    }
}
