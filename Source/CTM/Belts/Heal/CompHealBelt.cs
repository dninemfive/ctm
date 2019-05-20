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
                    /*
                    foreach (Hediff h in wearer.health.hediffSet.hediffs)
                    {
                        if (h.Bleeding)
                        {
                            h.BleedRate -= hp;
                        }
                    }*/
                }
            }
        }
        /* DEPRECATED for the performance boost from the unargued version
        public void HealRandomWounds(int n)
        {
            for (int i = 0; i < n; i++) HealRandomWound();
        }
        */
        public void HealRandomWounds()
        {
            //select one item from all items in inventory with less than 100% health and heal it by 1HP. 
            Apparel parent = base.parent as Apparel;
            Pawn wearer;
            if (parent != null && (wearer = parent.Wearer) != null)
            {
                if (!improveImmunity)
                {
                    IEnumerable<Hediff> hediffs = wearer.health.hediffSet.hediffs;
                    IEnumerable<Hediff_Injury> injuries = HealingUtility.GetNonPermanentInjuries(wearer);
                    for (int i = 0; i < heals; i++)
                    {
                        int rand = (int)Rand.Range(0f, injuries.Count());
                        injuries.ElementAt(rand).Severity -= hp;
                    }
                }
                else
                {
                    IEnumerable<Hediff> hediffs = wearer.health.hediffSet.hediffs;
                    IEnumerable<Hediff_Injury> injuries = HealingUtility.GetNonPermanentInjuries(wearer);
                    IEnumerable<ImmunityRecord> immunities = HealingUtility.GetImmunityRecords(wearer);
                    for (int i = 0; i < heals; i++)
                    {
                        int rand = (int)Rand.Range(0f, injuries.Count() + immunities.Count());
                        if (rand < injuries.Count()) injuries.ElementAt(rand).Severity -= hp;
                        else immunities.ElementAt(rand - injuries.Count()).immunity += hp;
                    }
                }
                /*
                foreach (Hediff h in hediffs){
                    Hediff_Injury hi = h as Hediff_Injury;
                    ImmunityRecord ir = wearer.health.immunity.GetImmunityRecord(h.def);
                    if (hi != null && !h.IsPermanent()) //h.Severity -= hp;
                    if (ir != null) ir.immunity += hp;
                    if (stopBleeding) {
                        /*
                        Hediff bloodLoss = wearer.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss, false);
                        if (bloodLoss != null && bloodLoss.Severity > 0) bloodLoss.Severity -= 0.05f;
                        *

                    }
                }*/
            }
        }
    }
}
