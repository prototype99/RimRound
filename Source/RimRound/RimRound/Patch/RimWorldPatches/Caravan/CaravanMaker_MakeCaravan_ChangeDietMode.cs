using HarmonyLib;
using RimRound.Utilities;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRound.Patch
{
    [HarmonyPatch(typeof(CaravanMaker))]
    [HarmonyPatch(nameof(CaravanMaker.MakeCaravan))]
    public class CaravanMaker_MakeCaravan_ChangeDietMode
    {
        public static void Postfix(IEnumerable<Pawn> __0, Caravan __result)
        {
            int id = __result.ID; //CaravanPatchUtility.GetUniqueID(__result);

            if (CaravanPatchUtility.activeCaravans.ContainsKey(id))
            {
                CaravanPatchUtility.activeCaravans[id].AddRange(__0);
            }
            else
            {
                CaravanPatchUtility.activeCaravans.Add(id, new List<Pawn>(__0));
            }

            foreach (Pawn p in __0)
                CaravanPatchUtility.SetDietModeToDisabled(p);
        }
    }
}
