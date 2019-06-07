using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace D9CTM
{
    class Simpod : Building_Pod
    {
        private List<SkillDef> skillsToTrain => base.def.GetModExtension<SimpodModExtension>().SkillsToTrain;
        private JoyKindDef joyKind => base.def.GetModExtension<SimpodModExtension>().JoyKind;
        private Boolean satisfiesOutdoors => base.def.GetModExtension<SimpodModExtension>().SatisfiesOutdoors;

        public override void Tick()
        {
            base.Tick();
            if (Powered) {
                foreach (Thing item in (IEnumerable<Thing>)base.innerContainer)
                {
                    Pawn pawn = item as Pawn;
                    if (pawn != null)
                    {
                        Pawn_SkillTracker tracker = pawn.skills;
                        if (tracker != null)
                        {
                            Passion highestPassion = Passion.None;
                            foreach (SkillDef def in skillsToTrain)
                            {
                                float xp = SkillTuning.XpPerTickDefault;
                                /*switch(def)
                                {
                                   case SkillDefOf.Mining: xp = SkillTuning.XpPerTickMining;
                                }*/
                                if (def.Equals(SkillDefOf.Mining))
                                {
                                    xp = SkillTuning.XpPerTickMining;
                                }
                                else if (def.Equals(SkillDefOf.Construction))
                                {
                                    xp = SkillTuning.XpPerTickConstruction;
                                }
                                else if (def.Equals(SkillDefOf.Plants))
                                {
                                    xp = SkillTuning.XpPerTickGrowing;
                                }
                                /*else if (def.Equals(SkillDefOf.Shooting))
                                {
                                    xp = (SkillTuning.XpPerSecondFiringHostile + SkillTuning.XpPerSecondFiringNonHostile);
                                }*/
                                Passion passion = tracker.GetSkill(def).passion;
                                if (passion == Passion.Minor && !(highestPassion == Passion.Major)) highestPassion = passion;
                                else if (passion == Passion.Major) highestPassion = passion;
                                tracker.Learn(def, getPassionFactor(passion) * .75f * xp);
                            }
                            /*Pawn_NeedsTracker tracker2 = pawn.needs;
                            if (tracker2 != null && highestPassion != Passion.None)
                            {
                                tracker2.joy.GainJoy(getPassionFactor(highestPassion) - 0.5f, joyKind);
                                //if (satisfiesOutdoors)
                                //{
                                //gonna need a Harmony patch
                                //}
                            }*/
                        }
                    }
                }
            }
        }

        private float getPassionFactor(Passion p)//this means sending passionate pawns to simpods is much more effective than making them do their jobs normally. Might want to rebalance at some point.
        {
            switch (p)
            {
                case Passion.None: return 0.5f;
                case Passion.Minor: return 1f;
                case Passion.Major: return 2f;
            }
            return 1f;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            using (IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(myPawn).GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    FloatMenuOption o = enumerator.Current;
                    yield return o;
                    ;
                }
            }
            if (base.innerContainer.Count != 0)
            {
                yield break;
            }
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return failer;
                ;
            }
            JobDef jobDef = JobDefOf.EnterCryptosleepCasket;
            string jobStr = "EnterCryptosleepCasket".Translate(); //TODO: add translation string for entering simpod
            Action jobAction = delegate
            {
                Job job = new Job(jobDef, new LocalTargetInfo(this));
                myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            };
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
        }

        public bool Powered
        {
            get
            {
                CompPowerTrader trader = this.TryGetComp<CompPowerTrader>();
                CompRefuelable fuelable = this.TryGetComp<CompRefuelable>();
                return ((trader == null) || (trader != null && trader.PowerOn)) && ((fuelable == null) || (fuelable != null && fuelable.HasFuel));
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    Gizmo c = enumerator.Current;
                    yield return c;
                    ;
                }
            }
            if (base.Faction != Faction.OfPlayer)
            {
                yield break;
            }
            if (base.innerContainer.Count <= 0)
            {
                yield break;
            }
            if (!base.def.building.isPlayerEjectable)
            {
                yield break;
            }
            Command_Action eject = new Command_Action
            {
                action = new Action(((Building_Casket)this).EjectContents),
                defaultLabel = "CommandPodEject".Translate(), //TODO: make unique versions of these for simpods
                defaultDesc = "CommandPodEjectDesc".Translate()
            };
            if (base.innerContainer.Count == 0)
            {
                eject.Disable("CommandPodEjectFailEmpty".Translate());
            }
            eject.hotKey = KeyBindingDefOf.Misc1;
            eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
            yield return (Gizmo)eject;
        }
    }
}
