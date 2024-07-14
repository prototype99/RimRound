using HarmonyLib;
using RimRound.Comps;
using RimRound.Things;
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
    [HarmonyPatch(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover))]
    [HarmonyPatch("CurrentStateInternal")]
    public static class ThoughtWorker_WantToSleepWithSpouseOrLover_IncludeBlobBedPartner
    {
        public static void Postfix(ref ThoughtState __result, Pawn p) {
            if (!__result.Active) {
                return;
            }

            // Pawn is sleeping on blob bed of partner
            if (p?.CurrentBed()?.IsBlobBed() ?? false)
            {
                Pawn originatingPawn = (p.ownership.OwnedBed as Building_BlobBed).originatingPawn;

                if (LovePartnerRelationUtility.LovePartnerRelationExists(p, originatingPawn))
                {
                    __result = false;
                    return;
                }
            }
            // Pawn is blob bed and partner is assigned
            else {
                if (p.TryGetComp<MakeBlobIntoBed_ThingComp>() is var comp && 
                    comp.IsBed && 
                    comp.blobBed != null &&
                    ((Building_BlobBed)comp.blobBed).GetAssignedPawn() is Pawn p1 &&
                    LovePartnerRelationUtility.LovePartnerRelationExists(p, p1)) {

                    __result = false;
                    return;
                }
            }
        }        
    }
}
