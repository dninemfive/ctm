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
        private float StandbyPower => Props.basePowerConsumption;
        public CompTempControl temp;
        private SimpleCurve powerFromTempCurve;
        private const float powerPerHeat = 10f;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            temp = base.parent.TryGetComp<CompTempControl>();
            initCurve();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            initCurve();
        }

        private void initCurve()
        {
            //just an approximation about how I think it should work
            //very low power gen under most conditions (much lower than standby power), increasing a lot across livable range and then having greatly reduced increases above that
            powerFromTempCurve = new SimpleCurve();
            powerFromTempCurve.Add(-273.15f, 0f);
            powerFromTempCurve.Add(-100f, 100f);
            powerFromTempCurve.Add(0f, 1000f);
            powerFromTempCurve.Add(50f, 10000f);
            powerFromTempCurve.Add(100f, 24000f);
            powerFromTempCurve.Add(150f, 40000f);
            powerFromTempCurve.Add(200f, 50000f);
            powerFromTempCurve.Add(2000f, 60000f);
        }

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

        enum Mode { Standby, StandbyForced, Power, Temperature };
    }
}