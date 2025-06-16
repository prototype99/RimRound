using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimRound.Utilities;
using Verse;

namespace RimRound.Patch
{
    [HarmonyPatch(typeof(HediffComp_Lactating))]
    [HarmonyPatch(nameof(HediffComp_Lactating.TryCharge))]
    public class HediffComp_Lactating_AddedNutritionPerDay_ChangeByBodyWeight
    {
        public static bool Prefix(ref float desiredChargeAmount, HediffComp_Lactating __instance) 
        {
            float mult = BodyResourceUtility.GetMilkMultiplierByWeight(__instance.parent.pawn);
            desiredChargeAmount *= mult;
            return true;
        }


    }
}
