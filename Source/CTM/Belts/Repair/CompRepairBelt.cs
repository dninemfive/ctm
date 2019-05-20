using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    //TODO: expose repair (and heal) rates in item description as effective heal rate (= HealChance * HealsPerProc * HealAmt). See CompProperties_Power's override SpecialDisplayStats
    class CompRepairBelt : ThingComp
    {
        private CompProperties_RepairBelt Props => (CompProperties_RepairBelt)base.props;
        private float healChance => Props.RepairChance10PerHour;
        private int heals => Props.RepairsPerProc;
        private int hp => Props.HPPerRepair;
        
        public override void CompTickRare()
        {
            if (Rand.Range(0f, 1f) < healChance) HealRandomItems(heals);
        }
        public void HealRandomItems(int n)
        {
            for (int i = 0; i < n; i++) HealRandomItem();
        }
        public void HealRandomItem()
        {
            //select one item from all items in inventory with less than 100% health and heal it by 1HP. 
            Apparel parent = base.parent as Apparel;
            Pawn wearer;
            if (parent != null && (wearer = parent.Wearer) != null)
            {
                IEnumerable<Thing> inv = (from t in wearer.inventory.innerContainer where t.def.useHitPoints && t.HitPoints < t.MaxHitPoints select t);
                int rand = (int)Rand.Range(0f, inv.Count());
                inv.ElementAt(rand).HitPoints += hp;
            }
        }
    }
}
