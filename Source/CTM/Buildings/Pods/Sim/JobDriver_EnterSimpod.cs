using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace D9CTM
{
    class JobDriver_EnterSimpod : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //notation: A = JobDriver_EnterCryptosleepCasket, B = JobDriver_SitFacingBuilding
            Pawn pawn = base.pawn; // A & B
            LocalTargetInfo targetA = base.job.targetA; // A & B
            Job job = base.job; // A & B
            bool errorOnFailed2 = errorOnFailed; // A & B
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed2); // A
        }
        // from JobDriver_EnterCryptosleepCasket
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil prepare = Toils_General.Wait(500, TargetIndex.None);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            prepare.AddFailCondition(delegate
            {
                return ((Building_Pod)TargetA).HasAnyContents;
            });
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
                action();
            };
            enter.tickAction = delegate
            {
                this.pawn.GainComfortFromCellIfPossible(); //TODO: add comfort to simpod def
                JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, 1f, (Building)this.TargetThingA);
            };
            enter.defaultCompleteMode = ToilCompleteMode.Delay;
            enter.defaultDuration = this.job.def.joyDuration;
            enter.AddFinishAction(delegate
            {
                Building_Pod a = enter.actor.CurJob.targetA.Thing as Building_Pod;
                a.EjectContents();
            });
            yield return enter;
        }
    }
}
