using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

/*
 *  REWORK: Structure: Building_HeatExchange
- CompTempControl
- CompPowerPlant ?? CompHeatExchangePowerGen
 */

namespace D9CTM
{
    class CompPowerHeatExchange : CompPowerPlant
    {
        private const int TicksPerPush = 60;
        private float StandbyPower => Props.basePowerConsumption;
        public CompTempControl temp;
        public CompPowerTrader power;
        public CompRefuelable fuelable;
        private SimpleCurve powerFromTempCurve;
        private const float powerPerHeat = 10f;
        #region mode
        private Mode preferredMode
        {
            get
            {
                return preferredMode;
            }
            set
            {
                if (value.Equals(Mode.StandbyForced)) preferredMode = Mode.Standby;
                else preferredMode = value;
            }
        }
        private Mode mode
        {
            get
            {
                Building b = (Building)base.parent;
                if (b != null && GridsUtility.UsesOutdoorTemperature(IntVec3Utility.ToIntVec3(b.TrueCenter()), b.Map)) return Mode.StandbyForced;
                return preferredMode;
            }
        }
        enum Mode { Standby, StandbyForced, Power, Temperature };
        #endregion mode
        #region bools
        private bool Powered
        {
            get
            {
                return power == null || (power != null && power.PowerOn);
            }
        }
        private bool Fueled
        {
            get
            {
                return fuelable == null || (fuelable != null && fuelable.HasFuel);
            }
        }
        private bool Push
        {
            get
            {
                if (mode == Mode.Standby || mode == Mode.StandbyForced) return false;
                if (mode == Mode.Temperature) return Powered && Fueled && heatToPush() != 0;
                return heatToPush() != 0;
            }
        }
        #endregion bools
        #region io
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            temp = base.parent.TryGetComp<CompTempControl>();
            InitCurve();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            InitCurve();
        }
        #endregion io
        #region tick
        public override void CompTick()
        {
            base.CompTick();
            if(base.parent.IsHashIntervalTick(TicksPerPush) && FlickUtility.WantsToBeOn(base.parent) && Push)
            {
                GenTemperature.PushHeat(base.parent.PositionHeld, base.parent.MapHeld, heatToPush());
            }
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            if (FlickUtility.WantsToBeOn(base.parent) && Push)
            {
                GenTemperature.PushHeat(base.parent.PositionHeld, base.parent.MapHeld, heatToPush() * (250/(float)TicksPerPush));
            }
        }
        #endregion tick

        private void InitCurve()
        {
            //very low power gen under most conditions (much lower than standby power), increasing a lot across livable range and then having greatly reduced increases above that
            //approximated by 50000/(1+e^-.9(x-50))
            powerFromTempCurve = new SimpleCurve();
            powerFromTempCurve.Add(-273.15f, 0f);
            //powerFromTempCurve.Add(-250f, 220f);
            powerFromTempCurve.Add(-200f, 550f);
            powerFromTempCurve.Add(-100f, 3150f);
            powerFromTempCurve.Add(-50f, 7090f);
            powerFromTempCurve.Add(0f, 14450f);
            powerFromTempCurve.Add(25f, 19470f);
            powerFromTempCurve.Add(50f, 25000f);
            powerFromTempCurve.Add(75f, 30530f);
            powerFromTempCurve.Add(100f, 35550f);
            powerFromTempCurve.Add(150f, 42910f);
            powerFromTempCurve.Add(200f, 46850f);
            powerFromTempCurve.Add(300f, 49450f);
            powerFromTempCurve.Add(2000f, 50000f);
        }       

        protected override float DesiredPowerOutput
        {
            get
            {
                switch (mode)
                {                    
                    case Mode.Power: return getPowerDif();
                    case Mode.Temperature: return heatToPush() * powerPerHeat;
                    case Mode.Standby:
                    case Mode.StandbyForced:
                    default: return 0 - StandbyPower;
                }
            }
        }

        private float getPowerDif()
        {
            Building b = (Building)base.parent;
            float local = GridsUtility.GetTemperature(IntVec3Utility.ToIntVec3(b.TrueCenter()), b.Map);
            return powerFromTempCurve.Evaluate(local);
        }

        private float heatToPush()
        {
            float target = temp.targetTemperature;
            Building b = (Building)base.parent;
            float local = GridsUtility.GetTemperature(IntVec3Utility.ToIntVec3(b.TrueCenter()), b.Map);
            return Mathf.Clamp((target - local) * temp.Props.energyPerSecond, -200, 200);
        }        
    }
}