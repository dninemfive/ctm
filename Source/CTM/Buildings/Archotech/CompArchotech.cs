using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompArchotech : ThingComp
    {
        struct Personality
        {
            int loyalty,            //higher -> lower chance of hostility and negative actions
            sanity,             //lower -> higher chance of positive and (mostly) negative actions
            drive,              //higher -> faster/better work to assigned job
            competitiveness;    //higher -> more likely to cause issues with other archotechs, directly reduces loyalty
        }
        public CompProperties_Archotech Props => (CompProperties_Archotech)base.props;
        public bool IsActive;
        public int ticksToBoot
        {
            get;
            private set;
        }
        public void StartBootUp()
        {
            ticksToBoot = Props.TicksToBootUp.RandomInRange;
            //play an animation of each server rack turning on, then the screen loading            
        }
        public void StartShutdown()
        {
            //spawn a bunch of raids and shit
        }
        public override void CompTick()
        {
            base.CompTick();
            if (ticksToBoot > 0) ticksToBoot--;
            else if(ticksToBoot == 0)
            {
                IsActive = !IsActive;
                ticksToBoot--;
            }
        }
    }
}
