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
    public class VehicleFramework_CaravanHelper_MakeVehicleCaravan_ChangeDietMode
    {
        public static void Postfix(IEnumerable<Pawn> __0, Caravan __result)
        {

            if (__result == null) 
            { 
                Log.Error($"Caravan was null in {nameof(VehicleFramework_CaravanHelper_MakeVehicleCaravan_ChangeDietMode)}"); 
            }
            
            Type t = VehicleFramework_CaravanHelper_MakeVehicleCaravan_ChangeDietMode.GetVehiclePawnType();
            if (t == null)
            {
                Log.Error("GetVehiclePawnType was null!");
            }


            int id = __result.ID;

            if (!CaravanPatchUtility.activeCaravans.ContainsKey(id))
            {
                CaravanPatchUtility.activeCaravans.Add(id, new List<Pawn>(__0));
            }
           

            foreach (Pawn p in __0) 
            {

                if (t.IsInstanceOfType(p))
                {
                    object pawns = AllPawnsAboardMI().Invoke(p, new object[] { });
                    List<Pawn> boardedPawns = pawns as List<Pawn>;


                    if (boardedPawns == null) 
                    {
                        Log.Error($"Boarded pawns were null in {nameof(VehicleFramework_CaravanHelper_MakeVehicleCaravan_ChangeDietMode)}");
                        return;
                    }

                    foreach (Pawn b in boardedPawns) {
                        CaravanPatchUtility.activeCaravans[id].Add(b);
                        CaravanPatchUtility.SetDietModeToDisabled(b);
                    }
                }
                else
                {
                    CaravanPatchUtility.activeCaravans[id].Add(p);
                    CaravanPatchUtility.SetDietModeToDisabled(p);
                }
            }
        }

        public static PatchCollection GetPatchCollection()
        {
            return new PatchCollection
            {
                postfix = typeof(VehicleFramework_CaravanHelper_MakeVehicleCaravan_ChangeDietMode).GetMethod(
                    nameof(VehicleFramework_CaravanHelper_MakeVehicleCaravan_ChangeDietMode.Postfix), ModCompatibilityUtility.majorFlags)
            };
        }


        public static MethodInfo _AllPawnsAboardMI;

        public static MethodInfo AllPawnsAboardMI() 
        {
            if (_AllPawnsAboardMI == null)
            {
                _AllPawnsAboardMI = GetVehiclePawnType().GetProperty("AllPawnsAboard", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(true);
            }

            return _AllPawnsAboardMI;
        }



        public static Type _vehiclePawnType;
        public static Type GetVehiclePawnType() 
        {
            if (_vehiclePawnType == null) {
                _vehiclePawnType = ModCompatibilityUtility.GetTypeFromMod("Vehicle Framework", "VehiclePawn");
            }

            return _vehiclePawnType;
        }
    }
}
