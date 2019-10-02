using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class ArchotechUtility
    {
        public static List<Map> maps => Find.Maps;
        public const int ticksPerThoughtStageUpgrade = GenDate.TicksPerDay * 5, nullThoughtStageIndex = -9999;

        public List<ResearchProjectDef> AllGrantableResearch //TODO: check that prerequisites have been researched
        {
            get
            {
                IEnumerable<ResearchProjectDef> defs = DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.techLevel == TechLevel.Archotech && progress.TryGetValue(x) >= x.baseCost && x.PrerequisitesCompleted);
                return defs.ToList();
            }
        }
        public static bool ArchotechIsActive()
        {
            //foreach existing map, if an archotech exists, is colonist-owned, and is powered, return true; else return false
            foreach (Map map in maps) if (map.listerBuildings.ColonistsHaveBuilding(delegate (Thing t)
             {
                 if (t.def.HasComp(typeof(CompArchotech)))
                 {
                     CompPowerTrader pow = t.TryGetComp<CompPowerTrader>();
                     if (pow == null || pow.PowerOn) return true;
                 }
                 return false;
             })) return true;
            return false;
        }
        public static bool ArchotechOrAnyBlueprintsExist()
        {
            return BuildingWithCompOrAnyBlueprintsExist(typeof(CompArchotech));
        }
        //foreach existing map, if an archotech or a blueprint thereof exists and is colonist-owned, return true; else return false
        public static bool BuildingWithCompOrAnyBlueprintsExist(Type comp)
        {            
            foreach (Map map in maps)
            {
                if (map.listerBuildings.ColonistsHaveBuilding(delegate (Thing t)
                {
                    if (t.def.HasComp(comp)) return true;
                    return false;
                })) return true;
                foreach (List<Blueprint> blueprints in map.blueprintGrid.InnerArray)
                {
                    if (blueprints != null)
                    {
                        foreach (Blueprint bp in blueprints)
                        {
                            if ((bp != null && (bp.Faction != null && bp.Faction.IsPlayer) && (bp.def != null && bp.def.HasComp(comp)))) return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
