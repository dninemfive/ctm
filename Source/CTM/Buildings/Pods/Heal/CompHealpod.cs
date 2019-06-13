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
        private float minorPct => Props.minorFuelUsagePercent;
        private float sealPct => Props.woundSealFuelUsagePercent;
        private float hp => Props.hp;
        private int hediffCount
        {
            get
            {
                //int? ct = pawn?.health.hediffSet.hediffs.Count;
                //return ct == null || ct == 0 ? 1 : (int)ct;
                IEnumerable<Hediff> hediffsToCount = from h in pawn?.health.hediffSet.hediffs where h.def.isBad select h;
                if (hediffsToCount == null) return 1;
                if (hediffsToCount.Count() == 0) return 1;
                return hediffsToCount.Count();
            }
        }
        public Pawn pawn
        {
            get
            {
                if (casket == null) return null;
                return casket.pawn;
            }
        }

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
            if (pawn != null)
            {
                if(PawnIsFullyHealed()) casket.EjectContents();
                SealWounds();
                DoMinorHeal();
                if (ticksToMajor <= 0)DoMajorHeal();      
                else ticksToMajor--;
            }
        }

        public void SealWounds()
        {
            if (pawn != null)
            {
                foreach (Hediff_Injury hi in HealingUtility.GetNonPermanentInjuries(pawn)) if (!hi.IsTended())
                    {
                        hi.Tended(Props.tendQuality);
                        ConsumeFuel(fuel.Props.fuelConsumptionRate * sealPct);
                    }
            }
        }

        public void DoMinorHeal()
        {
            foreach (Hediff_Injury hi in HealingUtility.GetNonPermanentInjuries(pawn))
            {
                hi.Severity -= hp;
                ConsumeFuel(fuel.Props.fuelConsumptionRate * minorPct);
            }
        }

        public void DoMajorHeal()
        {
            HealingUtility.FixWorstHealthCondition(pawn);
            ConsumeFuel(fuel.Props.fuelConsumptionRate);
            ResetMajorTicks();
        }

        public bool PawnIsFullyHealed()
        {
            if (pawn != null) {
                Pawn_HealthTracker tracker = pawn.health;
                return !tracker.HasHediffsNeedingTendByPlayer(false) && !HealthAIUtility.ShouldSeekMedicalRest(pawn) && !tracker.hediffSet.HasTendedAndHealingInjury();
            }
            return false;
        }

        public bool CanBeActive
        {
            get
            {
                CompPowerTrader trader = base.parent.TryGetComp<CompPowerTrader>();
                return ((trader == null) || (trader != null && trader.PowerOn)) && ((fuel == null) || (fuel != null && fuel.HasFuel));
            }
        }

        public void ConsumeFuel(float amount)
        {
            if (fuel == null) return;
            fuel.ConsumeFuel(amount);
        }

        private void ResetMajorTicks()
        {
            ticksToMajor = Props.baseRareTicksPerMajorHeal * (1/hediffCount);
        }
    }
}
