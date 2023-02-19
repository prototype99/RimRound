﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

using UnityEngine;
using System.Reflection;
using RimRound.Utilities;
using System.IO;
using System.Text.RegularExpressions;

namespace RimRound.Patch
{
	
	[StaticConstructorOnStartup]
	static class HarmonyPatchMain
	{
		static HarmonyPatchMain()
		{
			var harmony = new Harmony("RRHarmony");
			harmony.PatchAll();

            #region External Mod Patching
            ModCompatibilityUtility.TryPatch(
				harmony, 
				new ModPatchInfo("RimJobWorld - Milkable Colonists", "CompMilkableHuman", "ResourceAmount", MethodType.Getter), 
				CompMilkableHuman_ResourceAmount_AdjustForPawnBodyWeight.GetPatchCollection());


			ModCompatibilityUtility.TryPatch(
				harmony,
				new ModPatchInfo("Statue of Colonist", "StatueOfColonistRenderer", "Render", MethodType.Normal),
				StatueOfColonistRenderer_Render_SwitchGetBodyMeshForAlienRaceVersion.GetPatchCollection());

			int patchedMethodsCount = 0;
			int postfixesCount = 0;
			int prefixesCount = 0;
			int transpilersCount = 0;

			


			foreach (var x in harmony.GetPatchedMethods())
			{
				var y = Harmony.GetPatchInfo(x);
				IEnumerable<HarmonyLib.Patch> postfixes = from z in y.Postfixes where z.owner == harmony.Id select z;
				IEnumerable<HarmonyLib.Patch> prefixes = from z in y.Prefixes where z.owner == harmony.Id select z;
				IEnumerable<HarmonyLib.Patch> transpilers = from z in y.Transpilers where z.owner == harmony.Id select z;

				patchedMethodsCount++;
				postfixesCount += postfixes.Count();
				prefixesCount += prefixes.Count();
				transpilersCount += transpilers.Count();
			}

			Assembly RimRoundAssembly = Assembly.GetAssembly(typeof(HarmonyPatchMain));

            Log.Message($"[RimRound] {RimRoundAssembly.GetName()?.Name ?? "EGG"} version {RimRoundAssembly?.GetName()?.Version.ToString() ?? "EGGER"}");
			Log.Message($"[RimRound] Successfully added {prefixesCount} prefixes, {postfixesCount} postfixes, and {transpilersCount} transpilers to {patchedMethodsCount} methods with Harmony!");

			#endregion
        }

    }
}
