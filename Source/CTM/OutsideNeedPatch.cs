﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;
using System.Reflection;

namespace D9CTM
{
    [StaticConstructorOnStartup]
    static class OutsideNeedPatch
    {
        static OutsideNeedPatch()
        {
            
            var harmony = HarmonyInstance.Create("com.dninemfive.combinedtechmod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("Loaded CTM outdoors need patch");
            
            // manual patch code
            /*
            var harmony = HarmonyInstance.Create("com.dninemfive.advancedshields");
            var original = typeof(WorkGiver_HunterHunt).GetMethod("HasShieldAndRangedWeapon");
            var prefix = typeof(Hunterfix).GetMethod("HunterPrefix");
            harmony.Patch(original, new HarmonyMethod(prefix), null);
            original = typeof(Alert_ShieldUserHasRangedWeapon).GetMethod("GetReport");
            prefix = typeof(GenericFix).GetMethod("GenericPrefix");
            harmony.Patch(original, new HarmonyMethod(prefix), null);
            Log.Message("THIS IS COMMENTED OUT WHY ARE YOU READING IT");
            */
            
        }

        [HarmonyPatch(typeof(Need_Outdoors), "NeedInterval", new Type[] { })]
        class OutsidePatch
        {
            public static void OutsidePostfix(Need_Outdoors __instance, ref Pawn ___pawn, ref bool ___Disabled, ref bool ___IsFrozen, ref float ___lastEffectiveDelta)
            {
                IThingHolder holder = ___pawn.ParentHolder;
                if(holder != null && !___Disabled && !___IsFrozen)
                {
                    Thing holderThing = ThingOwnerUtility.SpawnedParentOrMe(holder);
                    if(holderThing != null && holderThing is Simpod)
                    {
                        SimpodModExtension sme = holderThing.def.GetModExtension<SimpodModExtension>();
                        if(sme != null && sme.SatisfiesOutdoors)
                        {
                            float cl = __instance.CurLevel;
                            __instance.CurLevel = 3f;
                            ___lastEffectiveDelta = __instance.CurLevel - cl;
                        }
                    }
                }
            }
        }
    }
}