using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace D9CTM
{
    class CompNanoRepair : ThingComp
    {
        private List<Thing> allAffectedThings = new List<Thing>();
        private List<Thing> affectedNonPawns = new List<Thing>();
        private List<Pawn> affectedPawns = new List<Pawn>();
        private Vector3 center => GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.Size, parent.def.Altitude);
        private CompProperties_NanoRepair Props => (CompProperties_NanoRepair)props;
        private float radius => Props.radius;
        private float repairRate => Props.repairHP10PerHour;
        private bool canRepair => repairRate > 0;
        private float healRate => Props.healHP10PerHour;
        private bool canHeal => healRate > 0;
        private int ticks => Props.rareTicksBetweenPulses;
        private int ticksTo;
        private bool useTicks => ticks > 0;

        public bool CanBeActive
        {
            get
            {
                CompPowerTrader trader = base.parent.TryGetComp<CompPowerTrader>();
                CompRefuelable fuelable = base.parent.TryGetComp<CompRefuelable>();
                return ((trader == null) || (trader != null && trader.PowerOn)) && ((fuelable == null) || (fuelable != null && fuelable.HasFuel));
            }
        }
        //Look at WorkGiver_DoBill.TryFindBestBillIngredients to optimize
        private void getAffectedThings()
        {
            allAffectedThings.Clear();
            affectedPawns.Clear();
            foreach (Thing t in base.parent.Map.listerThings.AllThings)
            {
                if (Vector3.Distance(GenThing.TrueCenter(t.Position, t.Rotation, t.def.Size, t.def.Altitude), center) <= radius)
                {
                    Pawn p = t as Pawn;
                    if (t.def.useHitPoints || p != null)
                    {
                        allAffectedThings.Add(t);
                    }
                    else if (p != null) affectedPawns.Add(p);
                }
            }
        }

        public override void CompTickRare()
        {
            if (CanBeActive)
            {
                if (!useTicks || (useTicks && ticksTo <= 0))
                {
                    getAffectedThings();
                    if (canRepair && allAffectedThings.Count > 0)
                    {
                        foreach (Thing t in allAffectedThings)
                        {
                            Pawn p = t as Pawn;
                            ThingOwner o = t.TryGetInnerInteractableThingOwner();
                            if (p == null && t.HitPoints < t.MaxHitPoints) t.HitPoints++;
                            if(o != null && o.Owner != null)
                            {
                                Log.Message("ThingOwner: " + o + ", thing: " + t + ", pawn: " + p ?? "null" + ", owner: " + o.Owner ?? "null");
                                //for (int i = 0; i < o.Count; i++) if (o[i].def.useHitPoints && o[i].HitPoints < o[i].MaxHitPoints) o[i].HitPoints++;
                                foreach (Thing t2 in ThingOwnerUtility.GetAllThingsRecursively(o.Owner)) if (t2.def.useHitPoints && t2.HitPoints < t2.MaxHitPoints) t2.HitPoints++;
                            }
                        }
                    }
                    if (canHeal && affectedPawns.Count > 0)
                    {
                        foreach (Pawn p in affectedPawns)
                        {
                            HealingUtility.PartiallyHealAllNonPermInjuries(p, healRate / 100f);
                        }
                    }
                }
            }
        }
    }
}