using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompOrbitalPowerPlant : CompPowerPlant
    {
        private Building_Storage par;
        //public float powerPerDef => Props.powerPerItem;
        public float powerPerDef => base.Props.basePowerConsumption;
        private float cachedCount;
        //public List<ThingDef> defsToCount => Props.defsToCount;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            par = base.parent as Building_Storage;
            if (par == null) Log.Error("[Accessible Archotech] A Thing has the comp CompOrbitalPowerPlant but is not of class Building_Storage. This is a big problem.");
            cachedCount = count();
        }

        protected override float DesiredPowerOutput
        {
            get
            {
                return powerPerDef * cachedCount;
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            cachedCount = count();
        }

        private float count()
        {
            int ct = 0;
            if (par != null)
            {
                foreach (Thing t in par.slotGroup.HeldThings)
                {
                    ct++;
                    /*
                    foreach (ThingDef d in defsToCount) if (t.def == d)
                        {
                            ct++;
                            break; //note: check if break only breaks one layer or both together. I miss labels.
                        }
                    */
                }
            }
            return ct;
        }
    }
}
