﻿using System;
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
            //foreach existing map, if an archotech or a blueprint thereof exists and is colonist-owned, return true; else return false
        }
    }
}