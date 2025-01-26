using AlienRace;
using HarmonyLib;
using RimRound.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRound.Patch
{
    [HarmonyPatch(typeof(AlienPawnRenderNodeWorker_BodyAddon))]
    [HarmonyPatch(nameof(AlienPawnRenderNodeWorker_BodyAddon.CanDrawNow))]
    public class AlienPawnRenderNodeWorker_BodyAddon_CanDrawNow_OnlyDrawHeadItemsForRRBodies
    {
        public static void Postfix(PawnRenderNode node, PawnDrawParms parms, ref bool __result) {
            BodyTypeDef bodytypedef = parms.pawn?.AsPawn()?.story?.bodyType;

            if (bodytypedef is null || !BodyTypeUtility.IsRRBody(bodytypedef)) {
                return;
            }

            __result = AlienPawnRenderNodeWorker_BodyAddon.AddonFromNode(node).alignWithHead;
        }
    }
}
