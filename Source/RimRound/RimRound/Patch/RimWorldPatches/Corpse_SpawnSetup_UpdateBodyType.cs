using HarmonyLib;
using RimRound.Comps;
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
    [HarmonyPatch(typeof(Corpse))]
    [HarmonyPatch(nameof(Corpse.SpawnSetup))]
    public class Corpse_SpawnSetup_UpdateBodyType
    {
        public static void Postfix(Corpse __instance) 
        {
            if (__instance?.InnerPawn is null || (!__instance?.InnerPawn?.RaceProps?.Humanlike ?? false))
                return;

            if (__instance.GetRotStage() == RotStage.Dessicated) // Don't update dessicated corpses so we can change their render size elsewhere
                return;

            PawnBodyType_ThingComp comp = __instance.InnerPawn.TryGetComp<PawnBodyType_ThingComp>();

            if (comp is null)
            {
                Log.Error($"Comp was null in {nameof(Corpse_SpawnSetup_UpdateBodyType.Postfix)} ");
                return;
            }

            BodyTypeUtility.UpdatePawnSprite(__instance.InnerPawn, comp.PersonallyExempt, comp.CategoricallyExempt, true, true);
        }
    }
}
