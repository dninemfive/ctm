/* DEPRECATED: use comp instead
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace D9CTM
{
    [StaticConstructorOnStartup]    
    public class RepairBelt : Apparel
    {
        private float repairRate => this.GetStatValue(CTMDefOf.D9NaniteRepairRate, true);
        private int ticksUntilNextRepair = 0;
        private bool canRepair => repairRate > 0;
        private float healRate => this.GetStatValue(CTMDefOf.D9NaniteHealRate, true);
        private int ticksUntilNextHeal = 0;
        private bool canHeal => healRate > 0;

        public override void Tick()
        {
            base.Tick();
            if(base.Wearer != null)// && Find.TickManager.TicksGame % (2500 / repairRate) == 0) //so a repair rate of 100% equals 1 HP repaired per hour, 50% = 2hr, &c. 
            {
                Pawn pawn = base.Wearer;
                if(canRepair && ticksUntilNextRepair <= 0)
                {
                    foreach(Thing t in pawn.inventory.innerContainer) //assuming inventory will never be empty since they're wearing at least this apparel item
                    {
                        if (t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                        {
                            t.HitPoints++;
                        }
                    }
                    ticksUntilNextRepair = (int)(2500 / repairRate); //so a repair rate of 100% equals 1 HP repaired per hour, 50% = 2hr, &c. 
                }
                else
                {
                    ticksUntilNextRepair--;
                }
                if (canHeal) {
                    List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                    if (ticksUntilNextHeal <= 0 && hediffs != null && hediffs.Count > 0)
                    {
                        foreach (Hediff h in hediffs)
                        {
                            if (h is Hediff_Injury && !h.IsTended())
                            {
                                h.Severity--;
                            }
                        }
                        ticksUntilNextHeal = (int)(2500 / healRate); //so a heal rate of 100% equals one heal pulse per hour, 50% = 2hr, &c
                    }
                    else
                    {
                        ticksUntilNextHeal--;
                    }
                }
            }
        }
    }
}*/