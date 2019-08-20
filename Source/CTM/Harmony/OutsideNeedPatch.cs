using System;
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
    static class HarmonyPatches
    {
        private const float Delta_InSimpod = 6.5f; //average of Delta_IndoorsNoRoof and Delta_OutdoorsNoRoof
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("com.dninemfive.combinedtechmod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[Accessible Archotech] Harmony loaded.");
        }
        
        [HarmonyPatch(typeof(Need_Outdoors))]
        [HarmonyPatch("NeedInterval")]
        class OutsidePatch
        {
            public static void OutsidePostfix(Need_Outdoors __instance, ref Pawn ___pawn, ref bool ___Disabled, ref bool ___IsFrozen, ref float ___lastEffectiveDelta)
            {
                Log.Message("Need_Outdoors.NeedInterval()");
                if (!___Disabled && !___IsFrozen)
                {
                    IThingHolder holder = ___pawn.ParentHolder;
                    if (holder != null)
                    {
                        Thing holderThing = ThingOwnerUtility.SpawnedParentOrMe(holder);
                        if (holderThing != null && holderThing is Building_Pod)
                        {
                            CompSimpod cs = holderThing.TryGetComp<CompSimpod>();
                            if (cs != null && cs.Props.SatisfiesOutdoors)
                            {
                                float cl = __instance.CurLevel;
                                __instance.CurLevel = Delta_InSimpod;
                                ___lastEffectiveDelta = __instance.CurLevel - cl;
                            }
                        }
                    }
                }
            }
        }
    }
}