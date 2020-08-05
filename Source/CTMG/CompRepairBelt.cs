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
}
