using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace D9CTM
{
    class Building_Pod : Building_Casket
    {
        CompProperties_PodSettings settings;
        ThingDef filth => settings.slimeDef;
        HediffDef hediff => settings.hediffOnExit;

        //TODO: un-hardcode shit if I ever have an incentive to
        /*
        private readonly int ticksPerMinorHeal = GenDate.TicksPerHour / 10;
        private int ticksUntilNextMajorHeal, ticksUntilNextMinorHeal;
        */

        /*
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
            //ticksUntilNextMajorHeal = GenDate.TicksPerHour * 6;
            //ticksUntilNextMinorHeal = GenDate.TicksPerHour;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref ticksUntilNextMinorHeal, "ticksUntilNextMinorHeal", 0, false);
            //Scribe_Values.Look(ref ticksUntilNextMinorHeal, "ticksUntilNextMinorHeal", 0, false);
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
        }*/

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                if (allowSpecialEffects)
                {
                    SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
                }
                return true;
            }
            return false;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(myPawn))
            {
                yield return o;
            }
            if (this.innerContainer.Count == 0)
            {
                if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return failer;
                }
                else
                {
                    JobDef jobDef = JobDefOf.EnterCryptosleepCasket;
                    string jobStr = "EnterPod".Translate(LabelShort);
                    Action jobAction = delegate
                    {
                        Job job = new Job(jobDef, this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            if (base.Faction == Faction.OfPlayer && this.innerContainer.Count > 0 && this.def.building.isPlayerEjectable)
            {
                Command_Action eject = new Command_Action();
                eject.action = new Action(this.EjectContents);
                eject.defaultLabel = "CommandPodEject".Translate();
                eject.defaultDesc = "CommandPodEjectDesc".Translate();
                if (this.innerContainer.Count == 0)
                {
                    eject.Disable("CommandPodEjectFailEmpty".Translate());
                }
                eject.hotKey = KeyBindingDefOf.Misc1;
                eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
                yield return eject;
            }
        }

        public override void EjectContents()
        {
            if(filth != null || hediff != null)foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
            {
                Pawn pawn = current as Pawn;
                if (pawn != null)
                {
                    if (filth != null)
                    {
                        PawnComponentsUtility.AddComponentsForSpawn(pawn);
                        pawn.filth.GainFilth(filth);
                    }
                    if (hediff != null && pawn.RaceProps.IsFlesh)
                    {
                        pawn.health.AddHediff(hediff, null, null, null);
                    }
                }
            }
            if (!base.Destroyed)
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));
            }
            base.EjectContents();
        }

        public static Building_CryptosleepCasket FindHealpodFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {
            IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
                                               where typeof(Building_CryptosleepCasket).IsAssignableFrom(def.thingClass)
                                               select def;
            foreach (ThingDef current in enumerable)
            {
                Building_CryptosleepCasket building_CryptosleepCasket = (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(current), PathEndMode.InteractionCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing x)
                {
                    bool arg_33_0;
                    if (!((Building_CryptosleepCasket)x).HasAnyContents)
                    {
                        Pawn traveler2 = traveler;
                        LocalTargetInfo target = x;
                        bool ignoreOtherReservations2 = ignoreOtherReservations;
                        arg_33_0 = traveler2.CanReserve(target, 1, -1, null, ignoreOtherReservations2);
                    }
                    else
                    {
                        arg_33_0 = false;
                    }
                    return arg_33_0;
                }, null, 0, -1, false, RegionType.Set_Passable, false);
                if (building_CryptosleepCasket != null)
                {
                    return building_CryptosleepCasket;
                }
            }
            return null;
        }
    }
}