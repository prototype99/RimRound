using HarmonyLib;
using RimRound.Comps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace RimRound.Patch
{
    [HarmonyPatch(typeof(FoodUtility))]
    [HarmonyPatch(nameof(FoodUtility.BestFoodSourceOnMap))]
    public class FoodUtility_BestFoodSourceOnMap_AlterValidatorToCheckForFullness
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) 
        {
            List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);
            List<CodeInstruction> newInstructions = new List<CodeInstruction>();
            bool triedToApply = false;
            int startJndex = -1;

            MethodInfo zigmaMI = typeof(FoodUtility_BestFoodSourceOnMap_AlterValidatorToCheckForFullness).GetMethod(nameof(Zigma), BindingFlags.Public | BindingFlags.Static);


            if (zigmaMI is null)
                Log.Error($"Error getting method info for {nameof(Zigma)}!");

            for (int jndex = 0; jndex < codeInstructions.Count; ++jndex)
            {
                if (codeInstructions[jndex].opcode != OpCodes.Ldftn ||
                    codeInstructions[jndex + 1].opcode != OpCodes.Newobj) continue;
                startJndex = jndex + 2;
                break;
            }

            newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_1));
            newInstructions.Add(new CodeInstruction(OpCodes.Call, zigmaMI));



            if (startJndex != -1)
            {
                triedToApply = true;
                codeInstructions.InsertRange(startJndex, newInstructions);
            }

            if (!triedToApply)
                Log.Error($"Failed to apply transpiler for {nameof(FoodUtility_BestFoodSourceOnMap_AlterValidatorToCheckForFullness.Transpiler)}");

            return codeInstructions.AsEnumerable();
        }
        
        static bool FeedingTubeModInstalled() 
        {
            try
            {
                Activator.CreateInstance("RimRound Feeding Tube", "Building_FoodFaucet");
                return true;
            }
            catch (Exception) 
            {
                return false;
            }
        }

        public static Predicate<Thing> Zigma(Predicate<Thing> ogPredicate, Pawn eater)//, Pawn eater, ThingDef foodDef)
        {
            Predicate<Thing> dogma = delegate (Thing t)
            {
                if (eater is null || !eater.RaceProps.Humanlike || t is null || !(eater.TryGetComp<FullnessAndDietStats_ThingComp>() is FullnessAndDietStats_ThingComp fullnessComp) || fullnessComp is null)
                {
                    if (eater != null && (eater.RaceProps.Humanlike || t == null)) return ogPredicate(t);
                    bool value = false;
                    try
                    {
                        value = ogPredicate(t) && !t.def.defName.Contains("FeedingTube") && !t.def.defName.Contains("FoodFaucet");
                    }
                    catch
                    {
                        //💪꒰ ˘ω˘ 💪 ꒱
                    }
                    return value;
                }

                // If the thing isn't ingestible in the vanilla sense, don't touch the original predicate
                if (t.def?.ingestible == null)
                    return ogPredicate(t);

                float ftnRatio = FullnessAndDietStats_ThingComp.defaultFullnessToNutritionRatio;
                ThingComp_FoodItems_NutritionDensity nutDensityComp = t.TryGetComp<ThingComp_FoodItems_NutritionDensity>();
                if (nutDensityComp != null)
                {
                    ftnRatio = nutDensityComp.Props.fullnessToNutritionRatio;
                }

                // If bars aren't initialized, fall back to original behavior
                Pair<float, float> ranges = fullnessComp.GetRanges();
                if (ranges.Second < 0)
                    return ogPredicate(t);

                float nutritionValueOfMeal = t.GetStatValue(StatDefOf.Nutrition) * ftnRatio;
                float wantedNutrition = fullnessComp.DietMode == DietMode.Fullness || fullnessComp.DietMode == DietMode.Hybrid
                    ? fullnessComp.RemainingFullnessUntil(ranges.Second) * ftnRatio
                    : fullnessComp.RemainingFullnessUntil(ranges.Second);

                int stackCount = FoodUtility.StackCountForNutrition(wantedNutrition, nutritionValueOfMeal);

                bool aboveHardLimit = fullnessComp.fullnessbar != null && fullnessComp.fullnessbar.peaceForeverHeld;
                if (!aboveHardLimit && fullnessComp.RemainingFullnessUntil(fullnessComp.HardLimit) <= nutritionValueOfMeal * stackCount)
                    return false;

                return ogPredicate(t);
            };

            return dogma;
        }

    }
}
