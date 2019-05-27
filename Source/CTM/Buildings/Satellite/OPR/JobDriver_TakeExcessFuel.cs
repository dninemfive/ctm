using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace D9CTM
{
    class JobDriver_TakeExcessFuel : JobDriver
    {
        //mostly taken from JobDriver_TakeBeerOutOfFermentingBarrel, from decompile here: https://github.com/josh-m/RW-Decompile/blob/master/RimWorld/JobDriver_TakeBeerOutOfFermentingBarrel.cs

        private const TargetIndex Building = TargetIndex.A;
        //private const TargetIndex Fuel = TargetIndex.B;

        private const int Duration = 200;

        protected Building building
        {
            get
            {
                return (Building)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected CompRefuelable comp
        {
            get
            {
                return building.GetComp<CompRefuelable>();
            }
        }

        protected ThingDef fuel
        {
            get
            {
                return comp.Props.fuelFilter.AllowedThingDefs.ElementAt(0);
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.building;
            Job job = this.job;
            return pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Log.Message("A: " + TargetIndex.A + ", B: " + TargetIndex.B + ", building: " + building + ", comp: " + comp + ", fuel: " + fuel);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).FailOn(() => this.comp.Fuel <= this.comp.TargetFuelLevel).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    int toTake = Mathf.FloorToInt(this.comp.Fuel - this.comp.TargetFuelLevel);
                    comp.ConsumeFuel(toTake);
                    while (toTake > 0)
                    {
                        int num2 = Mathf.Clamp(toTake, 1, this.fuel.stackLimit);
                        toTake -= num2;
                        Thing thing = ThingMaker.MakeThing(this.fuel, null);
                        thing.stackCount = num2;
                        GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, null, null);
                    }                    
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
