using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompRepairBelt : CompEMPable
    {
        private Pawn Pawn => (base.parent as Apparel)?.Wearer;
        public bool IsCheapHashIntervalTick(int tick) => true;
        public int HPToRepairPerPulse => (int)base.parent.GetStatValue(Props.apparelScoreOffsetStat);

        public override void CompTick()
        {
            base.CompTick();
            if (!EMPed && IsCheapHashIntervalTick(Find.TickManager.TicksAbs)) RepairItems();
        }

        public void RepairItems()
        {
            // I could short-circuit the IEnumerable, and I strongly considered it, but it's not worth the performance loss in cases where you'd want to iterate over the list multiple times.
            // Besides, with this, we can shuffle the order to make it seem more naturalistic.
            List<Thing> itemsToRepair = ItemsToRepair().ToList();
            int hpLeft = HPToRepairPerPulse;
            while(hpLeft > 0 && itemsToRepair.Count > 0)
            {
                Thing thing = itemsToRepair.RandomElement();
                if (thing.HitPoints < thing.MaxHitPoints)
                {
                    thing.HitPoints++;
                    hpLeft--;
                }
                else itemsToRepair = itemsToRepair.Where(x => x.HitPoints < x.MaxHitPoints).ToList();
            }
        }

        public IEnumerable<Thing> ItemsToRepair()
        {
            if (Pawn == null) yield break;
            foreach (Thing thing in Pawn.apparel.WornApparel) if (thing.def.useHitPoints && thing.HitPoints < thing.MaxHitPoints) yield return thing;
            foreach (Thing thing in Pawn.equipment.AllEquipmentListForReading) if (thing.def.useHitPoints && thing.HitPoints < thing.MaxHitPoints) yield return thing;
            foreach (Thing thing in Pawn.inventory.innerContainer) if (thing.def.useHitPoints && thing.HitPoints < thing.MaxHitPoints) yield return thing;
            Thing carriedThing = Pawn.carryTracker.CarriedThing;
            if (carriedThing.def.useHitPoints && carriedThing.HitPoints < carriedThing.MaxHitPoints) yield return carriedThing;
        }
    }
    class CompProperties_RepairBelt : CompProperties_EMPable
    {

    }
}
