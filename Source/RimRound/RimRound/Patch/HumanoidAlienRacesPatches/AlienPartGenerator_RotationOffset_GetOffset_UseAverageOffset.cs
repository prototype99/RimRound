using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using static AlienRace.AlienPartGenerator;

namespace RimRound.Patch
{
    /// <summary>
    /// If a race modder incorrectly aligns to head but uses an exhaustive list of individual body alignments, this will apply the average of the body alignments to RR bodies as well. This does not account for if they specify headtype-specific offsets, which appear to be extremely rare.
    /// </summary>
    [HarmonyPatch(typeof(AlienPartGenerator.RotationOffset))]
    [HarmonyPatch(nameof(AlienPartGenerator.RotationOffset.GetOffset))]
    public class AlienPartGenerator_RotationOffset_GetOffset_UseAverageOffset
    {
        static MethodInfo getZeroVectorMI = typeof(Vector2).GetProperty(nameof(Vector2.zero), BindingFlags.Static | BindingFlags.Public).GetGetMethod(true);
        static MethodInfo replacementMethodInfo = typeof(AlienPartGenerator_RotationOffset_GetOffset_UseAverageOffset).GetMethod(nameof(GetAverageOffset), BindingFlags.Static | BindingFlags.Public);
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);
            List<CodeInstruction> newInstructions = new List<CodeInstruction>();

            bool foundInsertionPoint = false;

            if (getZeroVectorMI is null) 
            {
                Log.Error($"Get zero vector method info was null in {nameof(AlienPartGenerator_RotationOffset_GetOffset_UseAverageOffset)}");
                return codeInstructions;
            }

            if (replacementMethodInfo is null)
            {
                Log.Error($"Replacement method info was null in {nameof(AlienPartGenerator_RotationOffset_GetOffset_UseAverageOffset)}");
                return codeInstructions;
            }



            for (int i = 0; i < codeInstructions.Count; i++)
            {
                if (codeInstructions[i].operand is MethodInfo mi && mi == getZeroVectorMI)
                {
                    foundInsertionPoint = true;

                    codeInstructions[i].operand = replacementMethodInfo;
                    codeInstructions.Insert(i, new CodeInstruction(OpCodes.Ldarg_1)); // portrait boolean
                    codeInstructions.Insert(i, new CodeInstruction(OpCodes.Ldarg_0)); // RotationOffset instance

                    break;
                }
            }

            if (!foundInsertionPoint)
                Log.Error($"Failed to find insertion point in {nameof(AlienPartGenerator_RotationOffset_GetOffset_UseAverageOffset)}.");

            return codeInstructions;
        }

        public static Vector2 GetAverageOffset(RotationOffset instance, bool portrait) {


            List<AlienPartGenerator.BodyTypeOffset> partOffsets = portrait ? (instance.portraitBodyTypes ?? instance.bodyTypes) : instance.bodyTypes;

            if (partOffsets == null) {
                return Vector2.zero;
            }

            Vector2 averageOffset = Vector2.zero;
            foreach (BodyTypeOffset bto in partOffsets) {
                averageOffset += bto.offset;
            }

            averageOffset /= partOffsets.Count;

            return averageOffset;
        
        }
    }
}
