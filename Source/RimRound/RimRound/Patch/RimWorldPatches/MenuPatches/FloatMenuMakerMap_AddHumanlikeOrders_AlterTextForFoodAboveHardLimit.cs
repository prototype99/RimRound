using HarmonyLib;
using RimRound.Comps;
using RimRound.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimRound.Patch
{
    [HarmonyPatch(typeof(FloatMenuOptionProvider_Ingest))]
    [HarmonyPatch("GetSingleOptionFor")]
    public class FloatMenuMakerMap_AddHumanlikeOrders_AlterTextForFoodAboveHardLimit
    {
        public static void Postfix(Thing clickedThing, FloatMenuContext context, ref FloatMenuOption __result)
        {
            if (context is null) 
            {
                return;
            }
            
            Pawn selectedPawn = context?.FirstSelectedPawn;

            if (selectedPawn == null || !selectedPawn.RaceProps.Humanlike) 
            {
                return;
            }

            string targetLabel = "ConsumeThing".Translate(clickedThing.LabelShort, clickedThing);
            if (__result?.Label.Contains(targetLabel) ?? false)
            {
                float fullnessToNutritionRatio =
                    clickedThing.TryGetComp<ThingComp_FoodItems_NutritionDensity>()?.Props?.fullnessToNutritionRatio ??
                    FullnessAndDietStats_ThingComp.defaultFullnessToNutritionRatio;

                if (selectedPawn.RaceProps.Humanlike && selectedPawn.TryGetComp<FullnessAndDietStats_ThingComp>() is FullnessAndDietStats_ThingComp FnDStatsComp &&
                    clickedThing.def.ingestible.CachedNutrition * fullnessToNutritionRatio >= FnDStatsComp.RemainingFullnessUntil(FnDStatsComp.HardLimit))
                {
                    if (!FnDStatsComp.fullnessbar.peaceForeverHeld)
                    {
                        __result.Label = "FloatMenuCantConsumeTooBig".Translate(clickedThing.LabelShort, clickedThing);
                        __result.action = null;
                    }
                }
            }
        }
    }
}
