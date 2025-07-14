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
using RimRound.Utilities;

namespace RimRound.Patch
{
    [HarmonyPatch(typeof(AlienPawnRenderNodeWorker_BodyAddon))]
    [HarmonyPatch(nameof(AlienPawnRenderNodeWorker_BodyAddon.ScaleFor))]
    public class AlienPawnRenderNodeWorker_BodyAddon_ScaleFor_DontScaleForBodySizeForRRBodies
    {
        static FieldInfo scaleWithPawnDrawsizeFI = typeof(AlienPartGenerator.BodyAddon).GetField(nameof(AlienPartGenerator.BodyAddon.scaleWithPawnDrawsize), BindingFlags.Instance | BindingFlags.Public);
        static MethodInfo replacementMI = typeof(AlienPawnRenderNodeWorker_BodyAddon_ScaleFor_DontScaleForBodySizeForRRBodies).GetMethod(nameof(ShouldScaleWithPawnDrawsize), BindingFlags.Static | BindingFlags.Public);
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);

            bool foundInsertionPoint = false;

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                if (codeInstructions[i].operand is FieldInfo fi && fi == scaleWithPawnDrawsizeFI)
                {
                    foundInsertionPoint = true;

                    if (replacementMI is null || scaleWithPawnDrawsizeFI is null) {
                        Log.Error($"Reflection targets were null in {nameof(AlienPawnRenderNodeWorker_BodyAddon_ScaleFor_DontScaleForBodySizeForRRBodies)}, {replacementMI}, {scaleWithPawnDrawsizeFI}");
                    }

                    codeInstructions[i] = new CodeInstruction(OpCodes.Call, replacementMI);
                    codeInstructions.Insert(i, new CodeInstruction(OpCodes.Ldloc_0)); // Add AlienPawnRenderNodeProperties_BodyAddon instance
                    codeInstructions.Insert(i, new CodeInstruction(OpCodes.Pop)); // Remove BodyAddon instance

                    break;
                }
            }

            if (!foundInsertionPoint)
                Log.Error($"Failed to find insertion point in {nameof(AlienPawnRenderNodeWorker_BodyAddon_ScaleFor_DontScaleForBodySizeForRRBodies)}.");

            return codeInstructions;
        }

        public static bool ShouldScaleWithPawnDrawsize(AlienPawnRenderNodeProperties_BodyAddon props) {
            BodyTypeDef bodytype = props?.alienComp?.parent?.AsPawn()?.story?.bodyType;

            if (bodytype == null) {
                return true;
            }

            return !BodyTypeUtility.IsRRBody(bodytype);
        }
    }
}
