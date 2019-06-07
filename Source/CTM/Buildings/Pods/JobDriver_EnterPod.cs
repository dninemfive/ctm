﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace D9CTM
{
    class JobDriver_EnterPod : JobDriver
    {
            public override bool TryMakePreToilReservations(bool errorOnFailed)
            {
                Pawn pawn = this.pawn;
                LocalTargetInfo targetA = this.job.targetA;
                Job job = this.job;
                return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
            }
            
            protected override IEnumerable<Toil> MakeNewToils()
            {
                this.FailOnDespawnedOrNull(TargetIndex.A);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                Toil prepare = Toils_General.Wait(500, TargetIndex.None);
                prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
                prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
                yield return prepare;
                Toil enter = new Toil();
                enter.initAction = delegate
                {
                    Pawn actor = enter.actor;
                    Building_Pod pod = (Building_Pod)actor.CurJob.targetA.Thing;
                    Action action = delegate
                    {
                        actor.DeSpawn(DestroyMode.Vanish);
                        pod.TryAcceptThing(actor, true);
                    };
                    if (!pod.def.building.isPlayerEjectable)
                    {
                        int freeColonistsSpawnedOrInPlayerEjectablePodsCount = this.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount;
                        if (freeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate(actor.Named("PAWN")).AdjustedFor(actor, "PAWN"), action, false, null));
                        }
                        else
                        {
                            action();
                        }
                    }
                    else
                    {
                        action();
                    }
                };
                enter.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return enter;
            }
        }
    }
