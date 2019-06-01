using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompHealpod : ThingComp
    {
        public CompProperties_Healpod Props => (CompProperties_Healpod)base.props;
        private int ticksPerMajor => Props.baseRareTicksPerMajorHeal;
        private int ticksToMajor;
        public Building_Pod casket;
        public CompRefuelable fuel;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            casket = base.parent as Building_Pod;
            if (casket == null) Log.Error("[Accessible Archotech] A thing with comp CompHealpod is not a Building_Pod.");
            fuel = base.parent.GetComp<CompRefuelable>();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            
        }

        public bool CanBeActive
        {
            get
            {
                CompPowerTrader trader = base.parent.TryGetComp<CompPowerTrader>();
                return ((trader == null) || (trader != null && trader.PowerOn)) && ((fuel == null) || (fuel != null && fuel.HasFuel));
            }
        }

        private void ResetMajorTicks()
        {
        }
    }
}
