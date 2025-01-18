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
    [HarmonyPatch(typeof(Caravan))]
    [HarmonyPatch(nameof(Caravan.RemoveAllPawns))]
    public class Caravan_RemoveAllPawns_ChangeDietMode
    {
        public static bool Prefix(Caravan __instance) 
        {
            int caravanID = __instance.ID;

            Type modType = Caravan_RemoveAllPawns_ChangeDietMode.GetVehiclePawnType();

            if (CaravanPatchUtility.activeCaravans.ContainsKey(caravanID))
            {
                foreach (Pawn p in CaravanPatchUtility.activeCaravans[caravanID])
                {
                    if ((vehicleFrameworkEnabled ?? false) && modType.IsInstanceOfType(p)) // If mod is enabled
                    {
                        object pawns = AllPawnsAboardMI().Invoke(p, new object[] { });
                        List<Pawn> boardedPawns = pawns as List<Pawn>;


                        if (boardedPawns == null)
                        {
                            Log.Error($"Boarded pawns were null in {nameof(Caravan_RemoveAllPawns_ChangeDietMode)}");
                            return true;
                        }

                        foreach (Pawn b in boardedPawns)
                        {
                            CaravanPatchUtility.RestoreDietMode(b);
                        }
                    }
                    else // Vanilla Behavior
                    {
                        CaravanPatchUtility.RestoreDietMode(p);
                    }
                }
                CaravanPatchUtility.activeCaravans.Remove(caravanID);
            }

            return true;
        }

        // MOD COMPAT FOR VehicleFramework below

        public static MethodInfo _AllPawnsAboardMI = null;
        public static MethodInfo AllPawnsAboardMI()
        {
            if (_vehiclePawnType == null) {
                return null;
            }

            if (_AllPawnsAboardMI == null)
            {
                _AllPawnsAboardMI = GetVehiclePawnType().GetProperty("AllPawnsAboard", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(true);
            }

            return _AllPawnsAboardMI;
        }



        public static Type _vehiclePawnType = null;
        public static bool? vehicleFrameworkEnabled = null;
        public static Type GetVehiclePawnType()
        {
            if (vehicleFrameworkEnabled != null && _vehiclePawnType == null)
            {
                _vehiclePawnType = ModCompatibilityUtility.GetTypeFromMod("Vehicle Framework", "VehiclePawn");
                vehicleFrameworkEnabled = _vehiclePawnType != null;
            }

            return _vehiclePawnType;
        }
    }
}
