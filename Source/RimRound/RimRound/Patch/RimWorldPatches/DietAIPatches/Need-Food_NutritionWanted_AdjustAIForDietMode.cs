using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using HarmonyLib;
using RimRound.UI;
using RimRound.Comps;
using RimRound.Utilities;

namespace RimRound.Patch
{
    [HarmonyPatch(typeof(Need_Food))]
    [HarmonyPatch("NutritionWanted", MethodType.Getter)]
    public class RimRound_NeedFood_NutritionWantedPatch
    {
        public static void Postfix(Need_Food __instance , ref float __result, Pawn ___pawn) 
        {
            // Guard: pawn and comp must exist and be humanlike; otherwise, respect vanilla result
            if (___pawn == null || !___pawn.RaceProps.Humanlike)
                return;

            FullnessAndDietStats_ThingComp fullnessComp = ___pawn.TryGetComp<FullnessAndDietStats_ThingComp>();
            if (fullnessComp == null || fullnessComp.Disabled)
                return;

            // Safe check for the "SetAboveHardLimit" state without dereferencing a null fullnessbar
            bool aboveHardLimit = fullnessComp.fullnessbar != null && fullnessComp.fullnessbar.peaceForeverHeld;

            float burstingNutritionMult = 2f;

            switch (fullnessComp.DietMode)
            {
                case DietMode.Nutrition:
                    {
                        var ranges = fullnessComp.GetRanges(); // safe: returns (-1,-1) if bars are null
                        float delta = ranges.Second - ranges.First;
                        if (delta < 0) return; // bars not initialized; keep vanilla
                        __result = aboveHardLimit ? delta * burstingNutritionMult : delta;
                        return;
                    }
                case DietMode.Hybrid:
                    {
                        if (aboveHardLimit)
                        {
                            __result = fullnessComp.RemainingFullnessUntil(fullnessComp.HardLimit) * burstingNutritionMult;
                        }
                        else
                        {
                            var ranges = fullnessComp.GetRanges();
                            float target = ranges.Second;
                            if (target < 0) return; // bars not initialized; keep vanilla
                            __result = target - fullnessComp.CurrentFullness; // was: / ratio (intentionally not used)
                        }
                        return;
                    }
                case DietMode.Fullness:
                    {
                        if (aboveHardLimit)
                        {
                            __result = fullnessComp.RemainingFullnessUntil(fullnessComp.HardLimit) * burstingNutritionMult;
                        }
                        else
                        {
                            var ranges = fullnessComp.GetRanges();
                            float target = ranges.Second;
                            if (target < 0) return; // bars not initialized; keep vanilla
                            __result = target - fullnessComp.CurrentFullness;
                        }
                        return;
                    }
                case DietMode.Disabled:
                    return; // respect vanilla
                default:
                    Log.Error($"{fullnessComp.parent.AsPawn().Name.ToStringShort}'s DietMode was invalid in RimRound_NeedFood_NutritionWantedPatch");
                    return;
            }
        }
    }
}
