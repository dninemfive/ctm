using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class Healpod : Building_Casket
    {
        //TODO: un-hardcode shit if I ever have an incentive to
        private readonly int ticksPerMinorHeal = GenDate.TicksPerHour / 10;
        private int ticksUntilNextMajorHeal, ticksUntilNextMinorHeal;
        private bool Powered
        {
            get
            {
                CompPowerTrader trader = this.TryGetComp<CompPowerTrader>();
                CompRefuelable fuelable = this.TryGetComp<CompRefuelable>();
                return ((trader == null) || (trader != null && trader.PowerOn)) && ((fuelable == null) || (fuelable != null && fuelable.HasFuel));
            }
        }

        public Healpod()
        {
            ticksUntilNextMajorHeal = GenDate.TicksPerHour * 6;
            ticksUntilNextMinorHeal = GenDate.TicksPerHour;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksUntilNextMinorHeal, "ticksUntilNextMinorHeal", 0, false);
            Scribe_Values.Look(ref ticksUntilNextMinorHeal, "ticksUntilNextMinorHeal", 0, false);
        }

        public override void Tick()
        {
            base.Tick();
            if (Powered)
            {
                foreach (Thing item in innerContainer)
                {
                    Pawn pawn = item as Pawn;
                    if (pawn != null)
                    {
                        CompRefuelable fuel = this.TryGetComp<CompRefuelable>();
                        
                       if(ticksUntilNextMinorHeal <= 0)
                        {
                            HealingUtility.doMinorHeal(pawn);
                            ticksUntilNextMinorHeal = ticksPerMinorHeal; //heals minor wounds 10 times an hour
                            if (fuel != null) fuel.ConsumeFuel(fuel.Props.fuelConsumptionRate / 240);
                        }
                       else
                        {
                            ticksUntilNextMinorHeal--;
                        }
                       if(ticksUntilNextMajorHeal <= 0)
                        {
                            CompUseEffect_FixWorstHealthCondition ce = new CompUseEffect_FixWorstHealthCondition();
                            ce.DoEffect(pawn);
                            //HealingUtility.doMajorHeal(pawn, "Healing Pod");
                            resetMajor(pawn);
                            if (fuel != null) fuel.ConsumeFuel(fuel.Props.fuelConsumptionRate / 24);
                        }
                        else
                        {
                            ticksUntilNextMajorHeal--;
                        }
                    }
                }
            }
        }

        public void resetMajor(Pawn p)
        {
            float permaScore = 0f;
            List<Hediff> hediffs = p.health.hediffSet.hediffs.Where(HediffUtility.IsPermanent).ToList();
            if (hediffs.Count == 0)if(PawnUtility.ShouldSendNotificationAbout(p)) Messages.Message("MessageAllPermanentWoundsHealed".Translate(p.Named("PAWN")), p, MessageTypeDefOf.PositiveEvent, true);
                else foreach (Hediff h in p.health.hediffSet.hediffs.Where(HediffUtility.IsPermanent)) permaScore += h.Severity / h.def.maxSeverity;
            permaScore = (int)permaScore <= 0 ? 1 : permaScore;
            ticksUntilNextMajorHeal = Rand.Range(20, 120/(int)permaScore) * 2500; //between 20 hours and 5 days, max less the more old wounds the pawn has
        }
    }
}
