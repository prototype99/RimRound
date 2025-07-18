﻿using AlienRace;
using AlienRace.ExtendedGraphics;
using RimRound.Comps;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimRound.Utilities
{
    public static class BodyTypeUtility
    {
        public static bool PawnIsOverWeightThreshold(Pawn pawn, BodyTypeDef bodyType)
        {
            float maxValueForBodyType = 0;
            if (RacialBodyTypeInfoUtility.defaultFemaleSet.ContainsKey(bodyType))
                maxValueForBodyType = RacialBodyTypeInfoUtility.defaultFemaleSet[bodyType].maxSeverity;
            else
                Log.Error("Only use default female bodytypedefs in PawnIsOverWeightThreshold!");

            if (Utilities.HediffUtility.WeightHediff(pawn).Severity > maxValueForBodyType * RacialBodyTypeInfoUtility.GetBodyTypeWeightRequirementMultiplier(pawn))
            {
                return true;
            }

            return false;
        }


        public static Dictionary<String, Dictionary<BodyTypeDef, bool>> raceToValidBodyTexturePaths = new Dictionary<String, Dictionary<BodyTypeDef, bool>>();

        private static bool CheckIfBodyDefHasCustomGraphicsForRace(string raceName, BodyTypeDef bodyTypeDef, string customPath)
        {
            if (IsRRBody(bodyTypeDef)) {
                return false;
            }

            if (!raceToValidBodyTexturePaths.ContainsKey(raceName)) {
                raceToValidBodyTexturePaths.Add(raceName, new Dictionary<BodyTypeDef, bool>());
            }

            string customBodyPath = customPath + BodyTypeUtility.ConvertBodyTypeDefDefnameAccordingToSettings(RacialBodyTypeInfoUtility.GetEquivalentBodyTypeDef(bodyTypeDef).defName);

            if (!raceToValidBodyTexturePaths[raceName].ContainsKey(bodyTypeDef))
            {
                bool isGoodTex = !ContentFinder<Texture2D>.Get(customBodyPath + "_north", false).NullOrBad();
                raceToValidBodyTexturePaths[raceName].Add(bodyTypeDef, isGoodTex);
            }

            return raceToValidBodyTexturePaths[raceName][bodyTypeDef];
        }


        public static string GetProperBodyGraphicPathFromPawn(Pawn pawn)
        {
            if (ModsConfig.BiotechActive && pawn.story.bodyType == BodyTypeDefOf.Baby)
            {
                return BodyTypeDefOf.Baby.bodyNakedGraphicPath;
            }
            else if (ModsConfig.BiotechActive && pawn.story.bodyType == BodyTypeDefOf.Child)
            {
                return BodyTypeDefOf.Child.bodyNakedGraphicPath;
            }


            string basePath = "Things/Pawn/Humanlike/Bodies/";

            if (pawn.def is AlienRace.ThingDef_AlienRace alienRace &&
                alienRace.alienRace.graphicPaths.body.path is String alienBodyPath &&
                alienBodyPath != basePath)
            {
                if (!IsRRBody(pawn.story.bodyType))
                {
                    /* 
                     * There are two mistakes mod makers make. The first is forgetting the last slash on the body path. The other is Adding the body name in the path (e.g. <path>Race/bodies/Naked_Thin</path>)
                     *  To account for both, we check if the last part of the path contains "Naked_"
                     */

                    if (alienBodyPath.Substring(alienBodyPath.LastIndexOf('/')).Contains("Naked_")) {
                        Log.Warning($"Ran A for {alienRace.defName}");
                        alienBodyPath = alienBodyPath.Substring(0, alienBodyPath.LastIndexOf('/') + 1);
                    }
                    else if (alienBodyPath.Last() != '/')
                    {
                        Log.Warning($"Ran B for {alienRace.defName}");

                        alienBodyPath = alienBodyPath + "/";
                    }

                    if (CheckIfBodyDefHasCustomGraphicsForRace(alienRace.defName, pawn.story.bodyType, alienBodyPath)) {
                        return alienBodyPath + "Naked_" + pawn.story.bodyType.defName;
                    }
                    else { 
                        return basePath + "Naked_" + pawn.story.bodyType.defName;
                    }


                }
            }

            string convertedString = ConvertBodyPathStringsIfNecessary(basePath +"Naked_" + pawn.story.bodyType.defName);

            if (pawn.def is AlienRace.ThingDef_AlienRace alienRace2 && RacialBodyTypeInfoUtility.specialRacialTextureSuffixes.ContainsKey(alienRace2.defName))
            {
                if (RacialBodyTypeInfoUtility.validTextureSuffixes.ContainsKey(RacialBodyTypeInfoUtility.specialRacialTextureSuffixes[alienRace2.defName]))
                {
                    Regex pattern = RacialBodyTypeInfoUtility.validTextureSuffixes[RacialBodyTypeInfoUtility.specialRacialTextureSuffixes[alienRace2.defName]];

                    if (pattern.IsMatch(convertedString))
                    {
                        int lastSlash2 = convertedString.LastIndexOf('/');

                        string basePath2 = convertedString.Substring(0, lastSlash2 + 1);
                        string bodyTypeName2 = convertedString.Substring(lastSlash2 + 1);

                        convertedString = basePath2 + RacialBodyTypeInfoUtility.specialRacialTextureSuffixes[alienRace2.defName] + "/" + bodyTypeName2;

                    }
                }
            }

            return convertedString;
        }

        public static string ConvertBodyPathStringsIfNecessary(string originalBodyPath)
        {
            int lastSlash = originalBodyPath.LastIndexOf('/');

            string basePath = originalBodyPath.Substring(0, lastSlash + 1);
            string bodyTypeName = originalBodyPath.Substring(lastSlash + 1);

            if (!IsRRBody(bodyTypeName))
            {
                return originalBodyPath;
            }

            if (bodyTypeName.Contains("Naked_"))
                bodyTypeName = bodyTypeName.Substring(bodyTypeName.IndexOf("Naked_") + 6);

            if (bodyTypeName == "F_060_LardyAlt")
                bodyTypeName = "F_060_Lardy";


            bodyTypeName = RacialBodyTypeInfoUtility.GetEquivalentBodyTypeDef(DefDatabase<BodyTypeDef>.GetNamed(bodyTypeName)).ToString();
            bodyTypeName = ConvertBodyTypeDefDefnameAccordingToSettings(bodyTypeName);



            return basePath + "Naked_" + bodyTypeName;
        }


        public static bool HasCustomBody(Pawn p)
        {
            if (p?.story?.bodyType is null)
                return false;

            return IsRRBody(p.story.bodyType);
        }

        public static bool IsRRBody(BodyTypeDef bodyTypeDef)
        {
            return IsRRBody(bodyTypeDef.defName);
        }

        public static bool IsRRBody(string bodyTypeDefString)
        {
            string pattern = @"[fmFM]{1}_+[0-9]{3,}?a*_+[A-Za-z]+";

            Regex regex = new Regex(pattern, RegexOptions.Compiled);

            if (regex.IsMatch(bodyTypeDefString))
                return true;

            return false;
        }


        public static string ConvertBodyTypeDefDefnameAccordingToSettings(string bodytypeCleaned)
        {
            if (GlobalSettings.onlyUseStandardBodyType && Regex.IsMatch(bodytypeCleaned, "[0-9]{3}a"))
            {
                bodytypeCleaned = Regex.Replace(bodytypeCleaned, "a_", "_");
            }

            if (GlobalSettings.useLegacyMaleSprites && Regex.IsMatch(bodytypeCleaned, @"^M_")) 
            {
                bodytypeCleaned += "Old";
            }
            else if (GlobalSettings.useAltMaleSprites && Regex.IsMatch(bodytypeCleaned, @"^M_")) // Else is intentional and necessary here
            {
                bodytypeCleaned += "New";
            }

            if (bodytypeCleaned == Defs.BodyTypeDefOf.F_060_Lardy.defName && !GlobalSettings.useOldLardySprite)
            {
                bodytypeCleaned += "Alt";
            }

            if (Regex.IsMatch(bodytypeCleaned, "Gelatinous"))
            {
                bodytypeCleaned = Regex.Replace(bodytypeCleaned, "[0-9]{3}", "100");
            }

            return bodytypeCleaned;
        }

        static Dictionary<Pawn, Corpse> cachedCorpseContainingPawnResults = new Dictionary<Pawn, Corpse>();

        public static void InvalidateCorpseCache()
        {
            cachedCorpseContainingPawnResults.Clear();
        }

        /// <summary>
        /// Validates result from cache.
        /// </summary>
        /// <param name="pawn">Pawn returned from cache</param>
        /// <param name="corpse">Corpse returned from cache</param>
        /// <returns>true of cache needs wiped, false otherwise.</returns>
        static bool CorpseCacheIsStale(Pawn pawn, Corpse corpse)
        {
            if (corpse.InnerPawn.ThingID != pawn.ThingID)
                return true;

            return false;
        }

        public static Corpse GetCorpseContainingPawn(Pawn pawn)
        {
            if (!pawn.Dead)
            {
                return null;
            }

            Corpse corpse = null;
            if (cachedCorpseContainingPawnResults.TryGetValue(pawn, out corpse))
            {
                return corpse;
            }
            else
            {
                List<Corpse> allCorpses = new List<Corpse>();
                ThingOwnerUtility.GetAllThingsRecursively<Corpse>(Find.CurrentMap, ThingRequest.ForGroup(ThingRequestGroup.Corpse), allCorpses, true, null, true);

                corpse = (allCorpses.Where(
                    delegate (Corpse c)
                    {
                        if (!(c.InnerPawn?.RaceProps?.Humanlike is bool b && b))
                            return false;

                        return c.InnerPawn.ThingID == pawn.ThingID;
                    }
                    ))?.FirstOrDefault();

                cachedCorpseContainingPawnResults.Add(pawn, corpse);
            }

            if (corpse is null)
            {
                Log.Warning($"Corpse was null in {nameof(BodyTypeUtility.GetCorpseContainingPawn)}.");
                return null;
            }

            if (CorpseCacheIsStale(pawn, corpse))
            {
                InvalidateCorpseCache();
                return GetCorpseContainingPawn(pawn);
            }
            else
                return corpse;

        }

        public static BodyTypeDef GetBodyTypeBasedOnWeightSeverity(Pawn pawn, bool personallyExempt = false, bool categoricallyExempt = false)
        {
            BodyTypeDef result = null;

            bool dessicated = GetCorpseContainingPawn(pawn) is Corpse c && c.IsDessicated();

            if (pawn.Dead && dessicated)
                return RimWorld.BodyTypeDefOf.Thin;

            if (personallyExempt || categoricallyExempt)
            {
                return pawn.TryGetComp<FullnessAndDietStats_ThingComp>().DefaultBodyType ??
                    RimWorld.BodyTypeDefOf.Thin;
            }


            Dictionary<BodyTypeDef, BodyTypeInfo> bodyTypeDictionary = RacialBodyTypeInfoUtility.GetRacialDictionary(pawn);


            if (bodyTypeDictionary is null)
                return pawn.story.bodyType;

            float weightSeverity = Utilities.HediffUtility.GetHediffOfDefFrom(Defs.HediffDefOf.RimRound_Weight, pawn)?.Severity ?? -1;
            if (weightSeverity == -1)
                return pawn.story.bodyType;

            float weightRequirementMultiplier = RacialBodyTypeInfoUtility.GetBodyTypeWeightRequirementMultiplier(pawn);

            //Edge cases
            if (weightSeverity == 0)
                result = bodyTypeDictionary.First().Key;

            if (weightSeverity >= bodyTypeDictionary.Last().Value.maxSeverity * weightRequirementMultiplier)
                result = bodyTypeDictionary.Last().Key;


            foreach (var x in bodyTypeDictionary)
            {
                if (weightSeverity <= x.Value.maxSeverity * weightRequirementMultiplier)
                {
                    result = x.Key;
                    break;
                }
            }

            if (result is null)
                result = bodyTypeDictionary.First().Key;

            if (!BodyTypeUtility.IsRRBody(result))
                return result;

            int chosenBodyTypeNumber;
            if (!int.TryParse(Regex.Match(result.defName, "[FM]_[0-9]{3}").Value.Substring(2), out chosenBodyTypeNumber))
            {
                Log.Error("Failed to get body number from defName.");
                chosenBodyTypeNumber = 0;
            }

            if (chosenBodyTypeNumber < 100 || GlobalSettings.maxVisualSizeGelLevel.threshold == 20)
                return result;


            int maxSizeNumber = RacialBodyTypeInfoUtility.gelatinousLevelToCode[GlobalSettings.maxVisualSizeGelLevel.threshold];

            if (maxSizeNumber < chosenBodyTypeNumber)
            {
                string resultString = result.defName;

                if (Regex.IsMatch(result.defName, "F_[0-9]{3}"))
                    resultString = Regex.Replace(result.defName, "F_[0-9]{3}", $"F_{maxSizeNumber}");
                else if (Regex.IsMatch(result.defName, "M_[0-9]{3}"))
                    resultString = Regex.Replace(result.defName, "M_[0-9]{3}", $"M_{maxSizeNumber}");

                result = DefDatabase<BodyTypeDef>.GetNamed(resultString);
            }


            return result;
        }


        /// <summary>
        /// Returns true if the pawn did update, false otherwise.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="personallyExempt"></param>
        /// <param name="categoricallyExempt"></param>
        /// <param name="forceUpdate"></param>
        /// <param name="BodyCheck"></param>
        public static bool UpdatePawnSprite(Pawn pawn, bool personallyExempt = false, bool categoricallyExempt = false, bool forceUpdate = false, bool BodyCheck = true)
        {
            if (!(pawn?.RaceProps?.Humanlike is bool isHumanlike && isHumanlike))
                return false;

            var comp = pawn.TryGetComp<PawnBodyType_ThingComp>();

            if (comp is null)
                return false;

            comp.ticksSinceLastBodyChange = 0;

            if (BodyCheck && BodyTypeUtility.GetBodyTypeBasedOnWeightSeverity(pawn, personallyExempt, categoricallyExempt) is BodyTypeDef b && b != pawn.story.bodyType)
            {
                pawn.story.bodyType = b;
                RedrawPawn(pawn);
                NotifyWeightChanged(pawn);
                return true;
            }
            else if (forceUpdate)
            {
                RedrawPawn(pawn);
            }

            return false;
        }

        private static void NotifyWeightChanged(Pawn pawn)
        {
            var pbtComp = pawn.TryGetComp<PawnBodyType_ThingComp>();
            pbtComp.NotifyWeightStageListeners();
            
        }

        private static bool ValidatePawnShouldBeRedrawn(Pawn pawn)
        {
            if (pawn.IsWorldPawn()) { 
                return false;
            }

            List<Pawn> allPawns = null;
            if (Find.CurrentMap is null)
                allPawns = Find.GameInitData.startingAndOptionalPawns;
            else
                allPawns = new List<Pawn>(Find.CurrentMap?.mapPawns?.AllPawns);

            if (allPawns is null)
                return false;

            if (!(allPawns.Where(delegate (Pawn p) { return p.ThingID == pawn.ThingID; }).Any()))
            {
                if (GetCorpseContainingPawn(pawn) is Corpse c)
                    return true;

                return false;
            }

            return true;

        }

        internal static void RedrawPawn(Pawn pawn)
        {
            if (!ValidatePawnShouldBeRedrawn(pawn))
                return;

            PortraitsCache.SetDirty(pawn);
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
            pawn?.Drawer?.renderer?.renderTree.SetDirty();
        }

        public static void UpdatePawnDrawSize(Pawn pawn, bool personallyExempt = false, bool categoricallyExempt = false)
        {

            if (pawn.def is ThingDef_AlienRace alienProps)
            {

                float drawSize;

                BodyTypeInfo? bodyTypeInfo = RacialBodyTypeInfoUtility.GetRacialBodyTypeInfo(pawn);
                if (bodyTypeInfo is null)
                    drawSize = 1;
                else
                    drawSize = bodyTypeInfo.AsNonNullable().meshSize;

                var alienComp = pawn.TryGetComp<AlienPartGenerator.AlienComp>();
                if (alienComp is null)
                {
                    Debug.LogWarning("AlienComp was null in update pawn drawsize!");
                }

                alienComp.customDrawSize =  new Vector2(drawSize, drawSize);

            }
        }


        /// <summary>
        /// Checks a pawn against settings in the RR tab for if they should use RimRound custom bodies.
        /// </summary>
        /// <param name="p">The pawn to check for eligibility.</param>
        /// <returns>true if they should be exempt, false otherwise.</returns>
        public static ExemptionReason CheckExemptions(Pawn p)
        {
            if (GlobalSettings.bodyChangeFemale is false && p.gender is Gender.Female)
                return new ExemptionReason("RR_ExemptionReason_FemaleDisabled".Translate());
            else if (GlobalSettings.bodyChangeMale is false && p.gender is Gender.Male)
                return new ExemptionReason("RR_ExemptionReason_MaleDisabled".Translate());
            else if (GlobalSettings.bodyChangeFriendlyNPC is false && p.Faction != Faction.OfPlayer && p.Faction.AllyOrNeutralTo(Faction.OfPlayer))
                return new ExemptionReason("RR_ExemptionReason_FriendlyDisabled".Translate());
            else if (GlobalSettings.bodyChangeHostileNPC is false && p.Faction.HostileTo(Faction.OfPlayer) && !p.IsPrisoner)
                return new ExemptionReason("RR_ExemptionReason_HostileDisabled".Translate());
            else if (GlobalSettings.bodyChangePrisoners is false && p.IsPrisoner)
                return new ExemptionReason("RR_ExemptionReason_PrisonerDisabled".Translate());
            else if (GlobalSettings.bodyChangeSlaves is false && p.IsSlaveOfColony)
                return new ExemptionReason("RR_ExemptionReason_SlaveDisabled".Translate());
            else if (GlobalSettings.minimumAgeForCustomBody.threshold > (p?.ageTracker?.AgeBiologicalYears ?? 0) || GlobalSettings.maximumAgeForCustomBody.threshold < (p?.ageTracker?.AgeBiologicalYears ?? 0))
                return new ExemptionReason("RR_ExemptionReason_AgeDisabled".Translate());
            else
                return false;

        }


        internal static void AssignPersonalCategoricalExemptions(PawnBodyType_ThingComp comp)
        {
            if (comp is null)
                return;

            if (!comp.parent?.AsPawn().RaceProps.Humanlike ?? false)
                return;

            comp.CategoricallyExempt = CheckExemptions(comp.parent.AsPawn());
        }

        internal static void UpdateAllPawnSprites()
        {
            List<Map> maps = Find.Maps.ToList();
            foreach (Map m in maps)
            {
                List<Pawn> pawnsOnMap = m.mapPawns.AllPawns.ToList();
                foreach (Pawn p in pawnsOnMap)
                {
                    PawnBodyType_ThingComp comp = p.TryGetComp<PawnBodyType_ThingComp>();

                    if (comp is null)
                        continue;

                    UpdatePawnSprite(p, comp.PersonallyExempt, comp.CategoricallyExempt, true, true);
                }
            }
        }


        internal static void AssignBodyTypeCategoricalExemptions(bool updatePawnSprite = false)
        {
            List<Map> maps = Find.Maps.ToList();
            foreach (Map m in maps)
            {
                List<Pawn> pawnsOnMap = m.mapPawns.AllPawns.ToList();
                foreach (Pawn p in pawnsOnMap)
                {
                    if (!m.mapPawns.AllPawns.Contains(p))
                        continue;

                    PawnBodyType_ThingComp comp = p.TryGetComp<PawnBodyType_ThingComp>();

                    if (comp is null)
                        continue;

                    AssignPersonalCategoricalExemptions(comp);

                    if (updatePawnSprite)
                        UpdatePawnSprite(p, comp.PersonallyExempt, comp.CategoricallyExempt, true, true);
                }
            }

            return;
        }

    }
}
