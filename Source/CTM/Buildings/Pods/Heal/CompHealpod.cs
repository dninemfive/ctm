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
            ticksToMajor = Props.baseRareTicksPerMajorHeal;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 250 == 0) CompTickRare();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();            
            if (pawn != null)
            {                
                SealWounds();
                DoMinorHeal();
                if (ticksToMajor <= 0)DoMajorHeal();      
                else ticksToMajor--;
                if (PawnIsFullyHealed()) casket.EjectContents();
            }
        }

        public void SealWounds()
        {
            if (pawn != null)
            {
                foreach (Hediff_Injury hi in HealingUtility.GetNonPermanentInjuries(pawn)) if (!hi.IsTended() && ConsumeFuel(fuel.Props.fuelConsumptionRate * sealPct)) hi.Tended(Props.tendQuality);
            }
        }

        public void DoMinorHeal()
        {
            foreach (Hediff_Injury hi in HealingUtility.GetNonPermanentInjuries(pawn))
            {
                if (hi != null && ConsumeFuel(fuel.Props.fuelConsumptionRate * minorPct)) hi.Severity -= hp;
            }
        }

        public void DoMajorHeal()
        {
            if (HealingUtility.HasWorstHealthCondition(pawn) && ConsumeFuel(fuel.Props.fuelConsumptionRate))
            {
                HealingUtility.FixWorstHealthCondition(pawn);
            }
            ResetMajorTicks();
        }

        public bool PawnIsFullyHealed()
        {
            if (pawn != null) {
                Pawn_HealthTracker tracker = pawn.health;
                return !tracker.HasHediffsNeedingTendByPlayer(false) && !HealthAIUtility.ShouldSeekMedicalRest(pawn) && !tracker.hediffSet.HasTendedAndHealingInjury() && !HealingUtility.HasWorstHealthCondition(pawn);
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

        public bool ConsumeFuel(float amount)
        {
            if (fuel == null) return true;
            if (amount <= fuel.Fuel)
            {
                fuel.ConsumeFuel(amount);
                return true;
            }
            else return false;
        }

        private void ResetMajorTicks()
        {
            ticksToMajor = Props.baseRareTicksPerMajorHeal * (1/hediffCount);
        }

        public override string CompInspectStringExtra()
        {
            return "TicksToMajorHeal".Translate((ticksToMajor * 250).ToStringTicksToPeriod());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToMajor, "ticksToMajor", Props.baseRareTicksPerMajorHeal);
        }
    }
}
