﻿using HarmonyLib;
using RimRound.Comps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRound.Patch
{
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateTraits")]
    public class PawnGenerator_GenerateTraits_AddWeightOpinions
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.TryGetComp<FullnessAndDietStats_ThingComp>() is null || !pawn.RaceProps.Humanlike)
                return;

            Utilities.PawnGeneratorUtility.InitializeWeightOpinion(pawn);
        }

    }
}
