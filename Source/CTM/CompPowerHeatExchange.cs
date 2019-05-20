using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace D9CTM
{
    class CompPowerHeatExchange : CompPowerPlant
    {

        public CompProperties_HeatExchange Props => (CompProperties_HeatExchange)props;
        private float StandbyPower => Props.standbyPower;
        public float targetTemperature = -99999f;
        private float defaultTargetTemperature => Props.defaultTargetTemperature;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (targetTemperature < -200f) targetTemperature = defaultTargetTemperature;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref targetTemperature, "targetTemperature", 0f, false);
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
                    case Mode.Temperature: return 0 - getPowerDif();
                    case Mode.Standby:
                    case Mode.StandbyForced:
                    default: return 0 - StandbyPower;
                }
            }
        }

        private float getPowerDif()
        {
            Building b = (Building)base.parent;
            float dif = Mathf.Abs(targetTemperature - GridsUtility.GetTemperature(IntVec3Utility.ToIntVec3(b.TrueCenter()), b.Map));
            return dif;// * dif;
        }

        enum Mode { Standby, StandbyForced, Power, Temperature };
    }
}