using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompHealBelt : ThingComp
    {
        private CompProperties_HealBelt Props => (CompProperties_HealBelt)base.props;
        private float healChance => Props.HealChance10PerHour;
        private float blood => Props.BloodLossReduction10PerHour;
        private int heals => Props.HealsPerProc;
        private float hp => Props.HPPerHeal/100f;
        private bool stopBleeding => blood > 0f;
        private bool improveImmunity => Props.IncreaseImmunity; //not planning on using since I can just increase BloodFiltration but left in anyway

        private bool tend => Props.TendWounds;
        private float tendQuality => Props.TendQuality;
        private int maxTends => Props.MaxWoundsToTend;

        public override void CompTick()
        {
            base.CompTick();
            if(Find.TickManager.TicksGame % 250 == 0)
            {
                if (Rand.Range(0f, 1f) < healChance) HealRandomWounds();
                if (stopBleeding)
                {
                    Apparel parent = base.parent as Apparel;
                    Pawn wearer;
                    if (parent != null && (wearer = parent.Wearer) != null)
                    {
                        Hediff bloodLoss = wearer.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss, false);
                        if (bloodLoss != null && bloodLoss.Severity > 0) bloodLoss.Severity -= blood;
                    }
                }
            }
        }

        public override void CompTickRare()
        {
            if (Rand.Range(0f, 1f) < healChance) HealRandomWounds();
            if (stopBleeding)
            {
                Apparel parent = base.parent as Apparel;
                Pawn wearer;
                if (parent != null && (wearer = parent.Wearer) != null)
                {
                    Hediff bloodLoss = wearer.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss, false);
                    if (bloodLoss != null && bloodLoss.Severity > 0) bloodLoss.Severity -= blood;
                }
            }
        }
        public void HealRandomWounds()
        {
            //select one item from all items in inventory with less than 100% health and heal it by 1HP. 
            Apparel parent = base.parent as Apparel;
            Pawn wearer;
            if (parent != null && (wearer = parent.Wearer) != null)
            {
                if (!improveImmunity)
                {
                    IEnumerable<Hediff_Injury> injuries = HealingUtility.GetNonPermanentInjuries(wearer);
                    if (injuries.Count() > 0)
                    {
                        for (int i = 0; i < heals; i++)
                        {
                            int rand = (int)Rand.Range(0f, injuries.Count());
                            injuries.ElementAt(rand).Severity -= hp;
                        }
                        int tends = new IntRange(0, maxTends).RandomInRange;
                        for (int i = 0; i < tends; i++)
                        {
                            int rand = (int)Rand.Range(0f, injuries.Count());
                            Hediff_Injury injury = injuries.ElementAt(rand);
                            if (!injury.IsTended())injury.Tended(tendQuality);
                        }
                    }
                }
                else
                {
                    IEnumerable<Hediff> hediffs = wearer.health.hediffSet.hediffs;
                    IEnumerable<Hediff_Injury> injuries = HealingUtility.GetNonPermanentInjuries(wearer);
                    IEnumerable<ImmunityRecord> immunities = HealingUtility.GetImmunityRecords(wearer);
                    if (injuries.Count() > 0)
                    {
                        for (int i = 0; i < heals; i++)
                        {
                            int rand = (int)Rand.Range(0f, injuries.Count() + immunities.Count());
                            if (rand < injuries.Count()) injuries.ElementAt(rand).Severity -= hp;
                            //note: due to poor execution this probably throws errors. It's deprecated so a nonissue imo
                            else immunities.ElementAt(rand - injuries.Count()).immunity += hp;
                        }
                        int tends = new IntRange(0, maxTends).RandomInRange;
                        for (int i = 0; i < tends; i++)
                        {
                            int rand = (int)Rand.Range(0f, injuries.Count());
                            Hediff_Injury injury = injuries.ElementAt(rand);
                            if (!injury.IsTended()) injury.Tended(tendQuality);
                        }
                    }
                }
            }
        }
    }
}
