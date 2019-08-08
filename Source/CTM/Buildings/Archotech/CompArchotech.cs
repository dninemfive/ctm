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
        public int ticksToRes, ticksToMech, ageTicks, hostileAge, endHostileAge, nextInstaMechTicks;
        public CompProperties_Archotech Props => (CompProperties_Archotech)base.props;
        public IntRange researchTicks => Props.TicksPerResearchPulse;
        public IntRange hostileTicks => Props.TicksPerHostilePulse;
        public IntRange raidDelay => Props.RaidDelay;
        public Dictionary<ResearchProjectDef, int> progress;
        #region hostility        

        public bool Hostile
        {
            get
            {
                if (DesignatedToDeconstruct || ToBeFlicked) return true;
                //check if targeted by any pawns or turrets
                return ageTicks > hostileAge && ageTicks < endHostileAge;
            }
        }

        public bool DesignatedToDeconstruct
        {
            get
            {
                foreach (Designation d in base.parent.Map.designationManager.AllDesignationsOn(base.parent))
                {
                    if (d.def.Equals(DesignationDefOf.Deconstruct)) return true;
                }
                return false;
            }
        }
        public bool ToBeFlicked
        {
            get
            {
                CompFlickable flickable = base.parent.TryGetComp<CompFlickable>();
                return (flickable == null || (flickable != null &&  flickable.SwitchIsOn && flickable.WantsFlick()));
            }
        }


        #endregion hostility       
        #region ticking, save/load
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                ticksToRes = 0;
                ticksToMech = 0;
                ageTicks = 0;
                hostileAge = Props.HostilityAgeRange.RandomInRange;
                endHostileAge = hostileAge + Props.HostilityEndRange.RandomInRange;
                InitResearch();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToRes, "ticksToRes", researchTicks.RandomInRange);
            Scribe_Values.Look(ref ticksToMech, "ticksToMech", hostileTicks.RandomInRange);
            Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
            Scribe_Values.Look(ref hostileAge, "hostileAge", Props.HostilityAgeRange.RandomInRange);
            Scribe_Values.Look(ref endHostileAge, "endHostileAge", hostileAge + Props.HostilityEndRange.RandomInRange);
            Scribe_Values.Look(ref nextInstaMechTicks, "nextInstaMechTicks", 0);
            Scribe_Collections.Look(ref progress, "progress", LookMode.Def, LookMode.Value);
        }

        public override void CompTick()
        {
            base.CompTick();
            ticksToRes--;
            ticksToMech--;
            if (base.parent.IsHashIntervalTick(250)) CheckDoEvent();
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            ticksToRes -= 250;
            ticksToMech -= 250;
            CheckDoEvent();
        }
        #endregion ticking, save/load
        public void CheckDoEvent()
        {
            ResearchPulse();
            if (DesignatedToDeconstruct || ToBeFlicked)
            {
                if (nextInstaMechTicks <= 0)
                {
                    InstantMechanoidRaid();
                    nextInstaMechTicks = GenDate.TicksPerHour * 6;
                }
                else nextInstaMechTicks--;
                TryDoThreat(0);
                return;
            }
            if(ticksToMech <= 0)
            {
                if(Hostile && !TryQueueMechRaid()) TryDoThreat(0);
                ticksToMech = hostileTicks.RandomInRange;
            }
        }                
        #region threats
        public void TryDoThreat(int tries)
        {
            if (tries > 3) return;
            bool didThreat = false;
            IntRange sel = new IntRange(0, 1);
                //if any pawn has brain chip, make them go berserk or catatonic
            //shut down random pawn's bionics
            //draw excessive power, generating heat, for an hour or so
                //break random building down
            //(once automated production and combat bots) produce a hostile combat bot
            switch (sel.RandomInRange)
            {
                default:
                    //psychic drone
                case 0: didThreat = TryDoBrainChipThreat();
                    break;
                case 1: didThreat = TryBreakdownRandomBuilding();
                    break;
            }
            if (!didThreat) TryDoThreat(tries + 1);
              
        }
        //Brain Chip
        public static bool PawnHasBrainChip(Pawn p)
        {
            foreach (Hediff h in p.health.hediffSet.hediffs)
            {
                if (h.def.HasModExtension<ModExtension_BrainChip>()) return true;
            }
            return false;
        }
        public bool TryDoBrainChipThreat()
        {
            IEnumerable<Pawn> pawnsWithBrainChips = from Pawn p in base.parent.Map.mapPawns.AllPawnsSpawned where PawnHasBrainChip(p) select p;
            if (!pawnsWithBrainChips.Any()) return false;
            MakePawnCatatonicOrBerserk(pawnsWithBrainChips.RandomElement());
            return true;
            
        }
        public void MakePawnCatatonicOrBerserk(Pawn p)
        {
            IntRange range = new IntRange(0, 1);
            if (range.RandomInRange == 1)
            {
                p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
            }
            else
            {
                p.health.AddHediff(HediffDefOf.CatatonicBreakdown, null, null, null);
                TrySendLetter(p, "LetterCatatonicMentalBreak");
            }
        }
        //adapted from MentalBreakWorker.TrySendLetter
        private void TrySendLetter(Pawn p, String key)
        {
            if (!PawnUtility.ShouldSendNotificationAbout(p)) return;
            string label = "Catatonic: " + p.LabelShortCap;
            string text = key.Translate(p.Label, p.Named("PAWN")).CapitalizeFirst();
            text = text.AdjustedFor(p, "PAWN");
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, p, null, null);
        }
        //bionic shutdown
        //power draw
        //building breakdown
        private bool TryBreakdownRandomBuilding()
        {
            IEnumerable<Building> breakdownables = from b in base.parent.Map.spawnedThings where b.def.building != null && !b.def.building.isNaturalRock && b.TryGetComp<CompBreakdownable>() != null select b as Building;
            if (!breakdownables.Any()) return false;
            Building toBreakDown = breakdownables.RandomElement();
            if (toBreakDown != null)
            {
                toBreakDown.TryGetComp<CompBreakdownable>().DoBreakdown();
                return true;
            }
            else return false;
        }
        #endregion threats
        #region raids
        public bool IncidentQueueContainsMechRaid
        {
            get
            {
                foreach (QueuedIncident qi in Find.Storyteller.incidentQueue)
                {
                    if (qi.FiringIncident.parms.faction.Equals(FactionDefOf.Mechanoid)) return true;
                }
                return false;
            }
        }
        public bool TryQueueMechRaid()
        {
            if (!IncidentQueueContainsMechRaid)
            {
                QueueMechanoidRaid();
                return true;
            }
            return false;
        }
        public void QueueMechanoidRaid()
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, base.parent.Map);
            parms.faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Mechanoid);
            FiringIncident fi = new FiringIncident();
            fi.def = IncidentDefOf.RaidEnemy;
            fi.parms = parms;
            Find.Storyteller.incidentQueue.Add(new QueuedIncident(fi, raidDelay.RandomInRange));
        }
        public void InstantMechanoidRaid()
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, base.parent.Map);
            parms.faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Mechanoid);
            parms.raidArrivalMode = edgeOrCenterDrop();
            IncidentWorker_RaidEnemy iwre = new IncidentWorker_RaidEnemy();
            iwre.TryExecute(parms);
        }
        private PawnsArrivalModeDef edgeOrCenterDrop()
        {
            IntRange range = new IntRange(0, 9);
            if (range.RandomInRange < 3) return PawnsArrivalModeDefOf.CenterDrop;
            else return PawnsArrivalModeDefOf.EdgeDrop;
        }
        #endregion raidshit
        #region research
        public void InitResearch()
        {
            foreach (ResearchProjectDef rpd in DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.techLevel == TechLevel.Archotech)) progress.Add(rpd, 0);
        }
        public void ResearchPulse()
        {
            foreach (KeyValuePair<ResearchProjectDef, int> kvp in progress)
            {
                progress[kvp.Key]++;
            }
            if (ticksToRes <= 0)
            {
                InstantlyFinishRandomGrantableResearch();
                ticksToRes = researchTicks.RandomInRange;
            }
            else ticksToRes--;
        }
        public List<ResearchProjectDef> AllGrantableResearch //TODO: check that prerequisites have been researched
        {
            get
            {
                IEnumerable<ResearchProjectDef> defs = DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.techLevel == TechLevel.Archotech && progress.TryGetValue(x) >= x.baseCost && x.PrerequisitesCompleted);
                return defs.ToList();
            }
        }
        public void InstantlyFinishRandomGrantableResearch() //... if possible
        {
            List<ResearchProjectDef> grantable = AllGrantableResearch;
            if(grantable.Count > 0) Find.ResearchManager.FinishProject(grantable.RandomElement());
        }
        #endregion research
    }
}
