using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompArchotech : ThingComp
    {
        public int ticksToRes, ticksToMech, ageTicks, hostileAge, endHostileAge;
        public CompProperties_Archotech Props => (CompProperties_Archotech)base.props;
        public IntRange researchTicks => Props.TicksPerResearchPulse;
        public IntRange hostileTicks => Props.TicksPerHostilePulse;

        public bool IncidentQueueContainsMechRaid
        {
            get
            {
                foreach(QueuedIncident qi in Find.Storyteller.incidentQueue)
                {
                    if (qi.FiringIncident.parms.faction.Equals(FactionDefOf.Mechanoid)) return true;
                }
                return false;
            }
        }

        public bool Hostile
        {
            get
            {
                //check if deconstruction ordered, pawn is attacking, &c; in latter case or if pawn is on way to deconstruct, instantly send raid instead of queueing
                return ageTicks > hostileAge && ageTicks < endHostileAge;
            }
        }

        public ResearchProjectDef CheapestArchotechResearch
        {
            get
            {
                IEnumerable<ResearchProjectDef> defs = DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.techLevel == TechLevel.Archotech);
                ResearchProjectDef ret = null;
                float cost = 99999999f;
                foreach (ResearchProjectDef rpd in defs)
                {
                    float temp = Find.ResearchManager.GetProgress(rpd);
                    if (temp < cost)
                    {
                        ret = rpd;
                        cost = temp;
                    }
                }
                return ret;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                ticksToRes = 0;
                ticksToMech = 0;
                ageTicks = 0;
                hostileAge = Props.HostilityAgeRange.RandomInRange;
                endHostileAge = Props.HostilityEndRange.RandomInRange;
                if(hostileAge >= endHostileAge)
                {
                    int x = hostileAge;
                    hostileAge = endHostileAge;
                    endHostileAge = x;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToRes, "ticksToRes", researchTicks.RandomInRange);
            Scribe_Values.Look(ref ticksToMech, "ticksToMech", hostileTicks.RandomInRange);
            Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
            Scribe_Values.Look(ref hostileAge, "hostileAge", Props.HostilityAgeRange.RandomInRange);
            Scribe_Values.Look(ref endHostileAge, "endHostileAge", Props.HostilityEndRange.RandomInRange);
        }

        public override void CompTick()
        {
            base.CompTick();
            ticksToRes--;
            ticksToMech--;
            CheckDoEvent();
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            ticksToRes -= 250;
            ticksToMech -= 250;
            CheckDoEvent();
        }
        public void CheckDoEvent()
        {
            if(ticksToRes <= 0)
            {
                InstantlyResearchCheapestArchotechResearch();
                ticksToRes = researchTicks.RandomInRange;
            }
            if(ticksToMech <= 0)
            {
                TryDoThreat();
                ticksToMech = hostileTicks.RandomInRange;
            }
        }        
        public void TryDoThreat()
        {
            if (!IncidentQueueContainsMechRaid) QueueMechanoidRaid();
            else if(Hostile)
            {
                //if any pawn has brain chip, make them go berserk or catatonic
                //shut down random pawn's bionics
                //draw excessive power, generating heat, for an hour or so
                //break random building down
                //(once automated production and combat bots) produce a hostile combat bot
            }
        }
        public void QueueMechanoidRaid()
        {

        }
        public void InstantlyResearchCheapestArchotechResearch()
        {
            Find.ResearchManager.FinishProject(CheapestArchotechResearch);
        }
    }
}
