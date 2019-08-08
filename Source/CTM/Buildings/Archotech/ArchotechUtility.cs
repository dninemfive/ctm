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
        public static CompArchotech Archotech
        {
            get
            {
                foreach (Map map in maps)
                {
                    IEnumerable<Building> archotechThings = map.listerBuildings.allBuildingsColonist.Where(x => x.def.HasComp(typeof(CompArchotech)));
                    if (archotechThings.Count() > 0) return archotechThings.ElementAt(0).TryGetComp<CompArchotech>();
                }
                return null;
            }
        }
        public static int ThoughtStage
        {
            get
            {
                if (Archotech == null) return nullThoughtStageIndex;
                return Archotech.ageTicks / ticksPerThoughtStageUpgrade;
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
        public static bool BuildingOrAnyBlueprintsExist(ThingDef td)
        {
            //foreach existing map, if an archotech or a blueprint thereof exists and is colonist-owned, return true; else return false
            foreach (Map map in maps)
            {
                if (map.listerBuildings.ColonistsHaveBuilding(td)) return true;
                foreach (List<Blueprint> blueprints in map.blueprintGrid.InnerArray)
                {
                    foreach(Blueprint bp in blueprints)
                    {
                        if (bp.Faction.IsPlayer && bp.def == td) return true;
                    }
                }
            }
            return false;
        }
        public static bool BuildingWithCompOrAnyBlueprintsExist(Type comp)
        {
            //foreach existing map, if an archotech or a blueprint thereof exists and is colonist-owned, return true; else return false
            foreach (Map map in maps)
            {
                if (map.listerBuildings.ColonistsHaveBuilding(delegate (Thing t)
                {
                    if (t.def.HasComp(comp)) return true;
                    return false;
                })) return true;
                foreach (List<Blueprint> blueprints in map.blueprintGrid.InnerArray)
                {
                    foreach (Blueprint bp in blueprints)
                    {
                        if (bp.Faction.IsPlayer && bp.def.HasComp(comp)) return true;
                    }
                }
            }
            return false;
        }
    }
}
