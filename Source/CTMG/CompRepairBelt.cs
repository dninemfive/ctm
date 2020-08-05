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
        public new CompProperties_RepairBelt Props => (CompProperties_RepairBelt)base.props;
        private Pawn Pawn => (base.parent as Apparel)?.Wearer;
        public bool IsCheapIntervalTick(int interval) => (int)(Find.TickManager.TicksGame + hashOffset) % interval == 0;
        public int HPToRepairPerPulse => (int)base.parent.GetStatValue(Props.apparelScoreOffsetStat);
        #region cheap hash interval shit
        private int hashOffset = 0;      

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            hashOffset = parent.thingIDNumber.HashOffset();
        }
        #endregion cheap hash interval shit

        public override void CompTick()
        {
            base.CompTick();
            if (!EMPed && IsCheapIntervalTick(Props.tickInterval)) RepairItems();
        }

        public void RepairItems()
        {
            // I could short-circuit the IEnumerable, and I strongly considered it, but it's not worth the performance loss in cases where you'd want to iterate over the list multiple times.
            // Besides, with this, we can shuffle the order to make it seem more naturalistic.
            // also considered healing with more than one HP at a time for cases with a lot of HP to repair and few items, but it's not worth the added complexity imo.
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
#pragma warning disable CS0649
        public int tickInterval = 250;
#pragma warning restore CS0649
        public CompProperties_RepairBelt()
        {
            base.compClass = typeof(CompRepairBelt);
        }
    }
}
