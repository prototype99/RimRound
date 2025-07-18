﻿using HarmonyLib;
using RimRound.Comps;
using RimRound.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimRound.UI
{
    public class MainTabWindow_RRGlobalSettings : MainTabWindow
    {
        const float windowWidth = 1200f;
        const float windowHeight = 900f;

        const float spaceBetweenCheckBoxes = 26;
        const float spaceBetweenNumberFields = 26;

        const float marginBetweenNumberFields = 8;

        const float bufferForCheckmarks = 40;
        const float bufferForNumberFields = 80;
        const float numberFieldRightOffset = 120;

        const int numberOfRowsPerColumnAlienRaceSettings = 24;


        readonly int numberOfGizmoSettingCheckboxes = 6;
        readonly int numberOfExemptionSettingsCheckboxes = 6;

        static float _metaFloat;
        static string _metaStrBuffer;
        static float _metaFloat2;
        static string _metaStrBuffer2;
        static float _metaFloat3;
        static string _metaStrBuffer3;

        List<TabRecord> tabs = new List<TabRecord>();

        enum TabKind
        {
            mainSettings,
            alienBodySettings,
            hediffSettings,
            soundSettings,
        };

        TabKind curTab = TabKind.mainSettings;

        public override void PreOpen()
        {
            base.PreOpen();
            tabs.Clear();
            this.tabs.Add(new TabRecord(
                "Main Settings",
                delegate () { this.curTab = TabKind.mainSettings; },
                () => this.curTab == TabKind.mainSettings));

            this.tabs.Add(new TabRecord(
                "Sound Settings",
                delegate () { this.curTab = TabKind.soundSettings; },
                () => this.curTab == TabKind.soundSettings));

            this.tabs.Add(new TabRecord(
                "Alien Body Settings",
                delegate () { this.curTab = TabKind.alienBodySettings; },
                () => this.curTab == TabKind.alienBodySettings));

            this.tabs.Add(new TabRecord(
                "Hediff Settings",
                delegate () { this.curTab = TabKind.hediffSettings; },
                () => this.curTab == TabKind.hediffSettings));

        }


        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(windowWidth, windowHeight);
            }
        }



        public override void DoWindowContents(Rect inRect)
        {
            Rect tabRect = inRect;
            tabRect.yMin += 45f;
            TabDrawer.DrawTabs<TabRecord>(tabRect, this.tabs);

            switch (curTab)
            {
                case TabKind.mainSettings:
                    DoGeneralSettingsWindow(inRect);
                    return;
                case TabKind.alienBodySettings:
                    DoAlienBodySettingsWindow(inRect);
                    return;
                case TabKind.hediffSettings:
                    DoHediffSettingsWindow(inRect);
                    return;
                case TabKind.soundSettings:
                    DoSoundSettingsWindow(inRect);
                    return;
                default:
                    return;
            }
        }

        private struct GenderRaceCombo
        {
            public Gender gender;
            public String race;
        };

        private void DoSoundSettingsWindow(Rect inRect) 
        {
            GUI.BeginGroup(inRect);
            Text.Font = GameFont.Medium;
            Rect soundSettingsTitleRect = new Rect(0, 50, 450, Text.LineHeight);
            Widgets.Label(soundSettingsTitleRect, "RR_Nhs_SoundSettingsTitle".Translate());

            Rect soundSettingsFieldRect = new Rect(0, soundSettingsTitleRect.yMax, soundSettingsTitleRect.width, 800);

            Text.Font = GameFont.Small;
            int numericFieldCount = 0;

            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundMalesGlobalMult, numericFieldCount++, "RR_soundSettings_soundMales", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundMalesGlobalMultiplier");
            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundFemalesGlobalMult, numericFieldCount++, "RR_soundSettings_soundFemales", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundFemalesGlobalMultiplier");

            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundFeedingMachine, numericFieldCount++, "RR_soundSettings_soundFeedingMachine", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundFeedingMachine");
            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundFeedingMachineSwallow, numericFieldCount++, "RR_soundSettings_soundFeedingMachineSwallow", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundFeedingMachineSwallow");

            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundRegularBreath, numericFieldCount++, "RR_soundSettings_soundRegularBreath", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundRegularBreath");
            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundPleasureBreath, numericFieldCount++, "RR_soundSettings_soundPleasureBreath", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundPleasureBreath");

            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundFootsteps, numericFieldCount++, "RR_soundSettings_soundFootsteps", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundFootsteps");

            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundStomachGurgles, numericFieldCount++, "RR_soundSettings_soundStomachGurgles", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundStomachGurgles");
            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundStomachStretch, numericFieldCount++, "RR_soundSettings_soundStomachStretch", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundStomachStretch");
            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundStomachEmpty, numericFieldCount++, "RR_soundSettings_soundStomachEmpty", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundStomachEmpty");
            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundStomachSlosh, numericFieldCount++, "RR_soundSettings_soundStomachSlosh", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundStomachSlosh");

            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundZenithOrb, numericFieldCount++, "RR_soundSettings_soundZenithOrb", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundZenithOrb");
            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundRapidWeightGain, numericFieldCount++, "RR_soundSettings_soundRapidWeightGain", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundRapidWeightGain");
            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundBurp, numericFieldCount++, "RR_soundSettings_soundBurp", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundBurp");
            NumberFieldLabeledWithRect(soundSettingsFieldRect, ref GlobalSettings.soundBurpDelaySeconds, numericFieldCount++, "RR_soundSettings_soundBurpDelaySeconds", GameFont.Small, null, "RR_ToolTip_SoundSettings_SoundBurpDelaySeconds");

            soundSettingsFieldRect.height = numericFieldCount * spaceBetweenNumberFields;

            GUI.EndGroup();
        }



        private void DoHediffSettingsWindow(Rect inRect) 
        {
            DoNumericHediffSettings(inRect);
        }

        private void DoNumericHediffSettings(Rect inRect) 
        {
            GUI.BeginGroup(inRect);

            //Category Title
            Text.Font = GameFont.Medium;
            Rect globalMultipliersSettingsTitleRect = new Rect(0, 50, 600, Text.LineHeight);
            Widgets.Label(globalMultipliersSettingsTitleRect, "RR_Hsw_HediffNumericSettings_SettingsTitle".Translate());

            Rect globalMultipliersSettingsFieldRect = new Rect(0, globalMultipliersSettingsTitleRect.yMax, globalMultipliersSettingsTitleRect.width, 200);

            Text.Font = GameFont.Small;

            int numericFieldCount = 0;


            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightHediffHungerRateMult, numericFieldCount++, "RR_Hsw_HediffNumericSettings_weightHediffHungerRateMult", GameFont.Small, () => { DirtyAllHediffSetCaches(); }, "RR_ToolTip_HediffSettings_WeightHungerFallRateMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightHediffManipulationPenaltyMult, numericFieldCount++, "RR_Hsw_HediffNumericSettings_weightHediffManipulationPenaltyMult", GameFont.Small, () => { DirtyAllHediffSetCaches(); }, "RR_ToolTip_HediffSettings_WeightManipulationPenaltyMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightHediffMovementPenaltyMult, numericFieldCount++, "RR_Hsw_HediffNumericSettings_weightHediffMovementPenaltyMult", GameFont.Small, () => { DirtyAllHediffSetCaches(); }, "RR_ToolTip_HediffSettings_WeightMovementPenaltyMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightHediffRestRateMult, numericFieldCount++, "RR_Hsw_HediffNumericSettings_weightHediffRestRateMult", GameFont.Small, () => { DirtyAllHediffSetCaches(); }, "RR_ToolTip_HediffSettings_WeightRestFallRateMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.fullnessHediffEatingPenaltyMult, numericFieldCount++, "RR_Hsw_HediffNumericSettings_fullnessHediffEatingPenaltyMult", GameFont.Small, () => { DirtyAllHediffSetCaches(); }, "RR_ToolTip_HediffSettings_FullnessEatingPenaltyMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.fullnessHediffMovementPenaltyMult, numericFieldCount++, "RR_Hsw_HediffNumericSettings_fullnessHediffMovementPenaltyMult", GameFont.Small, () => { DirtyAllHediffSetCaches(); }, "RR_ToolTip_HediffSettings_FullnessMovementPenaltyMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.fullnessHediffPainMult, numericFieldCount++, "RR_Hsw_HediffNumericSettings_fullnessHediffPainMult", GameFont.Small, () => { DirtyAllHediffSetCaches(); }, "RR_ToolTip_HediffSettings_FullnessPainMultiplier");

            globalMultipliersSettingsFieldRect.height = numericFieldCount * spaceBetweenNumberFields;

            GUI.EndGroup();
        }

        private void DoAlienBodySettingsWindow(Rect inRect)
        {
            MakeAllDropdownsForAllRaces();

        }

        private static void MakeAllDropdownsForAllRaces()
        {
            int positionIndex = 0;

            foreach (var raceEntry in RacialBodyTypeInfoUtility.raceToProperDictDictionary)
            {
                if (DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefsListForReading.Any(x => x.defName == raceEntry.Key)) 
                {
                    string raceName = raceEntry.Key;

                    MakeAllDropdownsForEachGenderForRace(raceName, ref positionIndex);
                }

            }
        }
        
        private static void MakeAllDropdownsForEachGenderForRace(string raceName, ref int positionIndex) 
        {
            Widgets.Label(new Rect { x = (float)(20 + 200 * Math.Floor(positionIndex/(float)numberOfRowsPerColumnAlienRaceSettings)), y = 50 + 27 * (positionIndex % numberOfRowsPerColumnAlienRaceSettings), width = 180, height = 25 }, raceName + "'s body settings");
            ++positionIndex;
            MakeDropdownsForGender(new GenderRaceCombo { race = raceName, gender = Gender.Male }, positionIndex++);
            MakeDropdownsForGender(new GenderRaceCombo { race = raceName, gender = Gender.Female }, positionIndex++);
            MakeDropdownsForGender(new GenderRaceCombo { race = raceName, gender = Gender.None }, positionIndex++);
        }

        private static void MakeDropdownsForGender(GenderRaceCombo genderRaceCombo, int positionIndex) 
        {
            string buttonLabel = genderRaceCombo.gender.ToString() + " bodytype";
            Rect dropdownMenuRect = new Rect()
            {
                x = (float)(20 + 200 * Math.Floor(positionIndex / (float)numberOfRowsPerColumnAlienRaceSettings)),
                y = 50 + 27 * (positionIndex % numberOfRowsPerColumnAlienRaceSettings),
                width = 180,
                height = 25,
            };

            Widgets.Dropdown<
                GenderRaceCombo,
                Dictionary<BodyArchetype, Dictionary<BodyTypeDef, BodyTypeInfo>>>
                (
                    dropdownMenuRect, genderRaceCombo, null, new Func<GenderRaceCombo, IEnumerable<Widgets.DropdownMenuElement<Dictionary<BodyArchetype, Dictionary<BodyTypeDef, BodyTypeInfo>>>>>(BodyTypeSetDropdownMenuGenerator), buttonLabel
                );
        }

        private static IEnumerable<Widgets.DropdownMenuElement<Dictionary<BodyArchetype, Dictionary<BodyTypeDef, BodyTypeInfo>>>> BodyTypeSetDropdownMenuGenerator(GenderRaceCombo genderRaceCombo)
        {
            using (var enumerator = RacialBodyTypeInfoUtility.genderedSets.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var currentEntry = enumerator.Current;

                    string label = currentEntry.Key;
                    var dicitonaryPayload = currentEntry.Value;

                    yield return new Widgets.DropdownMenuElement<Dictionary<BodyArchetype, Dictionary<BodyTypeDef, BodyTypeInfo>>>
                    {
                        option = new FloatMenuOption(label, delegate ()
                        {
                            RacialBodyTypeInfoUtility.raceToProperDictDictionary[genderRaceCombo.race][genderRaceCombo.gender] = dicitonaryPayload;

                            BodyTypeUtility.UpdateAllPawnSprites();
                        }),
                        payload = dicitonaryPayload
                    };
                }
            }
        }

        private void DoGeneralSettingsWindow(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, 2 * Text.LineHeight);
            DoMainSettingsTitleGroup(titleRect);

            Rect nutritionSettingsGroup = new Rect(inRect.x, titleRect.yMax, windowWidth / 3, 0);
            //DoNutritionSettingsGroup(nutritionSettingsGroup);

            Rect globalMultipliersSettingsGroup = new Rect(nutritionSettingsGroup.x, nutritionSettingsGroup.yMax, nutritionSettingsGroup.width, windowHeight);
            DoGlobalMultpliersSettingsGroup(globalMultipliersSettingsGroup);

            Rect exemptionSettingsGroup = new Rect(0.33333f * inRect.width, titleRect.yMax, 0.33333f * inRect.width, 200);
            DoExemptionSettingsGroup(exemptionSettingsGroup);

            Rect generalSettingsRect = new Rect(exemptionSettingsGroup.x, exemptionSettingsGroup.yMax, exemptionSettingsGroup.width, exemptionSettingsGroup.height + 500);
            DoGeneralSettingsGroup(generalSettingsRect);

            Rect gizmoSettingsGroupRect = new Rect(0.66666f * inRect.width, titleRect.yMax, 0.33333f * inRect.width, inRect.height - titleRect.height);
            DoUISettingsGroup(gizmoSettingsGroupRect);
        }

        private void DoUISettingsGroup(Rect gizmoSettingsGroup)
        {
            GUI.BeginGroup(gizmoSettingsGroup);

            Text.Font = GameFont.Medium;
            Rect gizmoSettingsTitleRect = new Rect(0, 0, gizmoSettingsGroup.width, Text.LineHeight);
            Widgets.Label(gizmoSettingsTitleRect, "RR_Mtw_GizmoSettingsTitle".Translate());


            Rect gizmoSettingsCheckBoxesRect = new Rect(0, gizmoSettingsTitleRect.yMax, gizmoSettingsGroup.width, numberOfGizmoSettingCheckboxes * spaceBetweenCheckBoxes);

            Text.Font = GameFont.Small;

            int positionIndex = 0;

            CheckboxLabeled(new Rect(
                gizmoSettingsCheckBoxesRect.x,
                positionIndex++ * spaceBetweenCheckBoxes + gizmoSettingsCheckBoxesRect.y,
                gizmoSettingsCheckBoxesRect.width - bufferForCheckmarks,
                spaceBetweenCheckBoxes),
                "RR_Mtw_GizmoSettings_PawnDietManagementGizmo",
                ref GlobalSettings.showPawnDietManagementGizmo,
                false, null, null, false,
                null,
                "RR_ToolTip_Preferences_ShowPawnDietGizmo");

            CheckboxLabeled(new Rect
            {
                x = gizmoSettingsCheckBoxesRect.x,
                y = positionIndex++ * spaceBetweenCheckBoxes + gizmoSettingsCheckBoxesRect.y,
                width = gizmoSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_GizmoSettings_LargeDietGizmo",
                ref GlobalSettings.largeDietGizmo,
                false, null, null, false,
                null,
                "RR_ToolTip_Preferences_ShowLegacyDietGizmo");

            CheckboxLabeled(new Rect
            {
                x = gizmoSettingsCheckBoxesRect.x,
                y = positionIndex++ * spaceBetweenCheckBoxes + gizmoSettingsCheckBoxesRect.y,
                width = gizmoSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_GizmoSettings_SleepPostureManagementGizmo",
                ref GlobalSettings.showSleepPostureManagementGizmo,
                false, null, null, false,
                null,
                "RR_ToolTip_Preferences_ShowSleepPostureGizmo");

            CheckboxLabeled(new Rect
            {
                x = gizmoSettingsCheckBoxesRect.x,
                y = positionIndex++ * spaceBetweenCheckBoxes + gizmoSettingsCheckBoxesRect.y,
                width = gizmoSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_GizmoSettings_BlanketMangementGizmo",
                ref GlobalSettings.showBlanketManagementGizmo,
                false, null, null, false,
                null,
                "RR_ToolTip_Preferences_ShowBlanketManagementGizmo");

            CheckboxLabeled(new Rect
            {
                x = gizmoSettingsCheckBoxesRect.x,
                y = positionIndex++ * spaceBetweenCheckBoxes + gizmoSettingsCheckBoxesRect.y,
                width = gizmoSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_GizmoSettings_ExemptionGizmo",
                ref GlobalSettings.showExemptionGizmo,
                false, null, null, false,
                null,
                "RR_ToolTip_Preferences_ShowPersonalExemptionGizmo");

            CheckboxLabeled(new Rect
            {
                x = gizmoSettingsCheckBoxesRect.x,
                y = positionIndex++ * spaceBetweenCheckBoxes + gizmoSettingsCheckBoxesRect.y,
                width = gizmoSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_GizmoSettings_BlobIntoBedGizmo",
                ref GlobalSettings.showBlobIntobedGizmo,
                false, null, null, false,
                null,
                "RR_ToolTip_Preferences_ShowBlobIntoBedGizmo");

            CheckboxLabeled(new Rect
            {
                x = gizmoSettingsCheckBoxesRect.x,
                y = positionIndex++ * spaceBetweenCheckBoxes + gizmoSettingsCheckBoxesRect.y,
                width = gizmoSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_GizmoSettings_showPerkTab",
                ref GlobalSettings.showPerkTab,
                false, null, null, false,
                null,
                "RR_ToolTip_Preferences_showPerkTab");

            CheckboxLabeled(new Rect
            {
                x = gizmoSettingsCheckBoxesRect.x,
                y = positionIndex++ * spaceBetweenCheckBoxes + gizmoSettingsCheckBoxesRect.y,
                width = gizmoSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_GizmoSettings_showDescriptionTab",
                ref GlobalSettings.showDescriptionTab,
                false, null, null, false,
                null,
                "RR_ToolTip_Preferences_showDescriptionTab");

            GUI.EndGroup();
        }

        private void DoGeneralSettingsGroup(Rect generalSettingsRect)
        {
            GUI.BeginGroup(generalSettingsRect);

            Text.Font = GameFont.Medium;
            Rect generalSettingsTitleRect = new Rect(0, 0, generalSettingsRect.width, Text.LineHeight);
            Widgets.Label(generalSettingsTitleRect, "RR_Mtw_GeneralSettings_Title".Translate());

            Text.Font = GameFont.Small;

            Rect generalSettingsCheckboxesRect = new Rect(0, generalSettingsTitleRect.yMax, generalSettingsRect.width, 300);
            int jndex = 0;
            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                        "RR_Mtw_GeneralSettings_BurstingEnabled",
                        ref GlobalSettings.burstingEnabled,
                        false, null, null, false,
                        null,
                        "RR_ToolTip_Preferences_BurstingEnabled");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_ShowTattoosForCustomBodies",
            ref GlobalSettings.showBodyTatoosForCustomSprites,
            false, null, null, false,
            () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
            "RR_ToolTip_Preferences_ShowTattoosOnRRBodies");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_PreferDefaultOverNaked",
            ref GlobalSettings.preferDefaultOutfitOverNaked,
            false, null, null, false,
            () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
            "RR_ToolTip_Preferences_PreferDefaultClothingOverNaked");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
           "RR_Mtw_GeneralSettings_AlternateNorthHeadDepthForRRBodies",
           ref GlobalSettings.alternateNorthHeadPositionForRRBodytypes,
           false, null, null, false,
           () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
           "RR_ToolTip_Preferences_AlternateHeadPlacement");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_UseZoomPortraitStyle",
            ref GlobalSettings.useZoomPortraitStyle,
            false, null, null, false,
            () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
            "RR_ToolTip_Preferences_UseZoomPortraitStyle");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_MoodletsForWeightOpinions",
            ref GlobalSettings.moodletsForWeightOpinions,
            false, null, null, false,
            null,
            "RR_ToolTip_Preferences_WeightOpinionsGiveMoodlets");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_VaryMinWeightForBodyTypeByBodySize",
            ref GlobalSettings.varyMinWeightForBodyTypeByBodySize,
            false, null, null, false,
            () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
            "RR_ToolTip_Preferences_VariedBodyWeightRequirements");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_ShowSpecialDebugSettings",
            ref GlobalSettings.showSpecialDebugSettings,
            false, null, null, false,
            null,
            "RR_ToolTip_Preferences_ShowSpecialDebugSettings");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_UseLegacyLardySprite",
            ref GlobalSettings.useOldLardySprite,
            false, null, null, false,
            () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
            "RR_ToolTip_Preferences_UseLegacyLardySprite");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_UseAltMaleSprites",
            ref GlobalSettings.useAltMaleSprites,
            false, null, null, false,
            () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
            "RR_ToolTip_Preferences_UseAltMaleSprites");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_UseLegacyMaleSprites",
            ref GlobalSettings.useLegacyMaleSprites,
            false, null, null, false,
            () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
            "RR_ToolTip_Preferences_UseLegacyMaleSprites");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_OnlyUseStandardSprites",
            ref GlobalSettings.onlyUseStandardBodyType,
            false, null, null, false,
            () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
            "RR_ToolTip_Preferences_UseOnlyStandardSet");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
           "RR_Mtw_GeneralSettings_hidePacksForCustomBodies",
           ref GlobalSettings.hidePacksForCustomBodies,
           false, null, null, false,
           () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
           "RR_ToolTip_Preferences_HidePacksForCustomBodies");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
           "RR_Mtw_GeneralSettings_usePoundsWherePossible",
           ref GlobalSettings.usePoundsWherePossible,
           false, null, null, false,
           null,
           "RR_ToolTip_Preferences_UsePoundsWherePossible");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = generalSettingsTitleRect.yMax + spaceBetweenCheckBoxes * jndex++,
                width = generalSettingsRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
            "RR_Mtw_GeneralSettings_showAllPerks", 
            ref GlobalSettings.showAllPerks, 
            false, null, null, false, 
            null,
            "RR_ToolTip_Preferences_ShowAllPerks");


            generalSettingsCheckboxesRect.height = 30 * jndex;
            GUI.EndGroup();
        }

        private void DoExemptionSettingsGroup(Rect exemptionSettingsGroup)
        {
            GUI.BeginGroup(exemptionSettingsGroup);

            Text.Font = GameFont.Medium;
            Rect exemptionSettingsTitleRect = new Rect(0, 0, exemptionSettingsGroup.width, Text.LineHeight);
            Widgets.Label(exemptionSettingsTitleRect, "RR_Mtw_BodyChangeExemptionSettingsTitle".Translate());

            Text.Font = GameFont.Small;

            Rect exemptionSettingsCheckBoxesRect = new Rect(0, exemptionSettingsTitleRect.yMax, exemptionSettingsGroup.width, numberOfExemptionSettingsCheckboxes * spaceBetweenCheckBoxes);

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = exemptionSettingsCheckBoxesRect.y,
                width = exemptionSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_BodyChangeExemptionSettings_Male",
                ref GlobalSettings.bodyChangeMale,
                false, null, null, false,
                () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
                "RR_ToolTip_Preferences_DynamicBodiesFor_Males");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = exemptionSettingsCheckBoxesRect.y + 1 * spaceBetweenCheckBoxes,
                width = exemptionSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_BodyChangeExemptionSettings_Female",
                ref GlobalSettings.bodyChangeFemale,
                false, null, null, false,
                () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
                "RR_ToolTip_Preferences_DynamicBodiesFor_Females");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = exemptionSettingsCheckBoxesRect.y + 2 * spaceBetweenCheckBoxes,
                width = exemptionSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_BodyChangeExemptionSettings_HostileNPC",
                ref GlobalSettings.bodyChangeHostileNPC,
                false, null, null, false,
                () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
                "RR_ToolTip_Preferences_DynamicBodiesFor_HostileNPCS");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = exemptionSettingsCheckBoxesRect.y + 3 * spaceBetweenCheckBoxes,
                width = exemptionSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_BodyChangeExemptionSettings_FriendlyNPC",
                ref GlobalSettings.bodyChangeFriendlyNPC,
                false, null, null, false,
                () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
                "RR_ToolTip_Preferences_DynamicBodiesFor_FriendlyNPCS");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = exemptionSettingsCheckBoxesRect.y + 4 * spaceBetweenCheckBoxes,
                width = exemptionSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_BodyChangeExemptionSettings_Prisoner",
                ref GlobalSettings.bodyChangePrisoners,
                false, null, null, false,
                () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
                "RR_ToolTip_Preferences_DynamicBodiesFor_Prisoners");

            CheckboxLabeled(new Rect
            {
                x = 0,
                y = exemptionSettingsCheckBoxesRect.y + 5 * spaceBetweenCheckBoxes,
                width = exemptionSettingsCheckBoxesRect.width - bufferForCheckmarks,
                height = spaceBetweenCheckBoxes
            },
                "RR_Mtw_BodyChangeExemptionSettings_Slave",
                ref GlobalSettings.bodyChangeSlaves,
                false, null, null, false,
                () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); },
                "RR_ToolTip_Preferences_DynamicBodiesFor_Slaves");
            GUI.EndGroup();
        }

        private void DoGlobalMultpliersSettingsGroup(Rect globalMultipliersSettingsGroup)
        {
            GUI.BeginGroup(globalMultipliersSettingsGroup);

            //Category Title
            Text.Font = GameFont.Medium;
            Rect globalMultipliersSettingsTitleRect = new Rect(0, 0, globalMultipliersSettingsGroup.width, Text.LineHeight);
            Widgets.Label(globalMultipliersSettingsTitleRect, "RR_Mtw_GlobalMultipliersSettingsTitle".Translate());

            Rect globalMultipliersSettingsFieldRect = new Rect(0, globalMultipliersSettingsTitleRect.yMax, globalMultipliersSettingsTitleRect.width, 200);
            //globalMultipliersSettingsFieldRect.y += _metaFloat3;
            Text.Font = GameFont.Small;

            int numericFieldCount = 0;


            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.minimumAgeForCustomBody, numericFieldCount++, "RR_Mtw_MinimumAgeForCustomBody", GameFont.Small, () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); }, "RR_ToolTip_NumericPreferences_MinimumAgeForCustomBody");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.maximumAgeForCustomBody, numericFieldCount++, "RR_Mtw_MaximumAgeForCustomBody", GameFont.Small, () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); }, "RR_ToolTip_NumericPreferences_MaximumAgeForCustomBody");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightGainMultiplier, numericFieldCount++, "RR_Mtw_GlobalWeightGainMultiplierTitle", GameFont.Small, null, "RR_ToolTip_NumericPreferences_GlobalWGModifier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightGainMultiplierFemale, numericFieldCount++, "RR_Mtw_GlobalWeightGainMultiplierTitleFemale", GameFont.Small, null, "RR_ToolTip_NumericPreferences_FemaleWGModifier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightGainMultiplierMale, numericFieldCount++, "RR_Mtw_GlobalWeightGainMultiplierTitleMale", GameFont.Small, null, "RR_ToolTip_NumericPreferences_MaleWGModifier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightLossMultiplier, numericFieldCount++, "RR_Mtw_GlobalWeightLossMultiplierTitle", GameFont.Small, null, "RR_ToolTip_NumericPreferences_GlobalWLModifier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightLossMultiplierFemale, numericFieldCount++, "RR_Mtw_GlobalWeightLossMultiplierTitleFemale", GameFont.Small, null, "RR_ToolTip_NumericPreferences_FemaleWLModifier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightLossMultiplierMale, numericFieldCount++, "RR_Mtw_GlobalWeightLossMultiplierTitleMale", GameFont.Small, null, "RR_ToolTip_NumericPreferences_MaleWLModifier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.fullnessMultiplier, numericFieldCount++, "RR_Mtw_GlobalFullnessMultiplierTitle", GameFont.Small, null, "RR_ToolTip_NumericPreferences_FullnessMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.digestionRateMultiplier, numericFieldCount++, "RR_Mtw_GlobalDigestionRateMultiplierTitle", GameFont.Small, null, "RR_ToolTip_NumericPreferences_DigestionRateMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.stomachElasticityMultiplier, numericFieldCount++, "RR_Mtw_GlobalStomachElasticityMultiplierTitle", GameFont.Small, null, "RR_ToolTip_NumericPreferences_StomachElasticityMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.ticksPerHungerCheck, numericFieldCount++, "RR_Mtw_TicksPerHungerCheckTitle", GameFont.Small, null, "RR_ToolTip_NumericPreferences_TicksPerHungerCheck");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.ticksPerBodyChangeCheck, numericFieldCount++, "RR_Mtw_TicksPerBodyChangeCheckTitle", GameFont.Small, null, "RR_ToolTip_NumericPreferences_TicksPerBodyChangeCheck");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.softLimitMuliplier, numericFieldCount++, "RR_Mtw_GlobalSoftLimitMultiplier", GameFont.Small, null, "RR_ToolTip_NumericPreferences_SoftLimitMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.hardLimitMuliplier, numericFieldCount++, "RR_Mtw_GlobalHardLimitMultiplier", GameFont.Small, null, "RR_ToolTip_NumericPreferences_HardLimitMultiplier");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.minWeight, numericFieldCount++, "RR_Mtw_MinWeight", GameFont.Small, () => { CheckMaxMinThresholds(); }, "RR_ToolTip_NumericPreferences_MinWeight");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.maxWeight, numericFieldCount++, "RR_Mtw_MaxWeight", GameFont.Small, () => { CheckMaxMinThresholds(); }, "RR_ToolTip_NumericPreferences_MaxWeight");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightToBeBed, numericFieldCount++, "RR_Mtw_GlobalBlobIntoBedThreshold", GameFont.Small, null, "RR_ToolTip_NumericPreferences_MinWeightForBlobBed");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.weightToAdjustWiggleAngle, numericFieldCount++, "RR_Mtw_GlobalWeightToAdjustWiggleAngleThreshold", GameFont.Small, null, "RR_ToolTip_NumericPreferences_WeightForAltWiggleAngle");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.ticksBetweenWeightGainRequestProcess, numericFieldCount++, "RR_Mtw_GlobalTicksBetweenWeightGainRequestProcess", GameFont.Small, null, "RR_ToolTip_NumericPreferences_TicksPerWeightGainRequestProcess");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.meatMultiplierForWeight, numericFieldCount++, "RR_Mtw_meatMultiplierForWeight", GameFont.Small, null, "RR_ToolTip_NumericPreferences_MeatMultiplierForWeight");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.minForCapableMovement, numericFieldCount++, "RR_Mtw_minForCapableMovement", GameFont.Small, () => { RefreshMovementAbility(); }, "RR_ToolTip_NumericPreferences_MinForCapableMovement");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.maxVisualSizeGelLevel, numericFieldCount++, "RR_Mtw_maxVisualSizeGelLevel", GameFont.Small, () => { BodyTypeUtility.AssignBodyTypeCategoricalExemptions(true); }, "RR_ToolTip_NumericPreferences_MaxGelatinousSize");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.nutritionPerPerkLevel, numericFieldCount++, "RR_Mtw_nutritionPerLevel", GameFont.Small, null, "RR_ToolTip_NumericPreferences_NutritionPerPerkLevel");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.levelsGainedPerLevel, numericFieldCount++, "RR_Mtw_levelsGainedPerLevel", GameFont.Small, null, "RR_ToolTip_NumericPreferences_LevelsGainedPerLevel");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.minWeightChangeForNumberText, numericFieldCount++, "RR_Mtw_minWeightToThrowText", GameFont.Small, null, "RR_ToolTip_NumericPreferences_MinWeightChangeForThrowingText");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.milkMultiplierForWeight, numericFieldCount++, "RR_Mtw_milkMultiplierForWeight", GameFont.Small, null, "RR_Mtw_milkMultiplierForWeightDesc");
            NumberFieldLabeledWithRect(globalMultipliersSettingsFieldRect, ref GlobalSettings.maxMilkMultiplier, numericFieldCount++, "RR_Mtw_maxMilkMultiplier", GameFont.Small, null, "RR_Mtw_maxMilkMultiplierDesc");


            /**/
            globalMultipliersSettingsFieldRect.height = numericFieldCount * spaceBetweenNumberFields;

            GUI.EndGroup();
        }


        private void DoMainSettingsTitleGroup(Rect titleRect)
        {
            if (Prefs.DevMode)
            {
                Rect metaRect = new Rect(titleRect.x + (titleRect.width / 4), titleRect.y, titleRect.width / 5, Text.LineHeight);
                Widgets.TextFieldNumericLabeled<float>(metaRect, "Meta field 1 ", ref _metaFloat, ref _metaStrBuffer);

                Rect metaRect2 = new Rect(titleRect.x + (titleRect.width / 2), titleRect.y, titleRect.width / 5, Text.LineHeight);
                Widgets.TextFieldNumericLabeled<float>(metaRect2, "Meta field 2 ", ref _metaFloat2, ref _metaStrBuffer2);

                Rect metaRect3 = new Rect(titleRect.x + (0.75f * titleRect.width), titleRect.y, titleRect.width / 5, Text.LineHeight);
                Widgets.TextFieldNumericLabeled<float>(metaRect3, "Meta field 3 ", ref _metaFloat3, ref _metaStrBuffer3);
            }

            Widgets.Label(titleRect, "RR_Mtw_Title".Translate());
        }

        private void DoNutritionSettingsGroup(Rect nutritionSettingsGroup)
        {
            GUI.BeginGroup(nutritionSettingsGroup);

            //Category Title
            Text.Font = GameFont.Medium;
            Rect mapNutritionTitleRect = new Rect(0, 0, nutritionSettingsGroup.width, Text.LineHeight);
            Widgets.Label(mapNutritionTitleRect, "RR_Mtw_MapNutritionStatsTitle".Translate());

            float mapNutrition = Find.CurrentMap?.resourceCounter?.TotalHumanEdibleNutrition ?? 0;


            //Map Nutrition Section
            Text.Font = GameFont.Tiny;
            Rect rectMapNutContent = new Rect(0, mapNutritionTitleRect.yMax, nutritionSettingsGroup.width, 6 * Text.LineHeight);
            NutritionTable nutTable = TotalHumanEdibleNutritionOfType(Find.CurrentMap.resourceCounter);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("RR_Mtw_NutritionOverview_TotalNutrition".Translate() + ": " + mapNutrition.ToString("F1"));
            stringBuilder.AppendLine("RR_Mtw_NutritionOverview_SimpleMealNutrition".Translate() + ": " + nutTable.MealSimple.ToString("F1"));
            stringBuilder.AppendLine("RR_Mtw_NutritionOverview_FineMealNutrition".Translate() + ": " + nutTable.MealFine.ToString("F1"));
            stringBuilder.AppendLine("RR_Mtw_NutritionOverview_LavishMealNutrition".Translate() + ": " + nutTable.MealLavish.ToString("F1"));
            stringBuilder.AppendLine("RR_Mtw_NutritionOverview_UndesireableNutrition".Translate() + ": " + (nutTable.MealAwful + nutTable.RawBad + nutTable.DesperateOnly + nutTable.DesperateOnlyForHumanlikes).ToString("F1"));
            stringBuilder.AppendLine("RR_Mtw_NutritionOverview_OtherNutrition".Translate() + ": " +
                (mapNutrition -
                (nutTable.MealSimple +
                nutTable.MealFine +
                nutTable.MealLavish +
                nutTable.Undefined +
                nutTable.MealAwful +
                nutTable.RawBad +
                nutTable.DesperateOnly +
                nutTable.DesperateOnlyForHumanlikes)).ToString("F1"));

            Widgets.Label(rectMapNutContent, stringBuilder.ToString().Trim());

            Rect nutritionPerPawnLabel = new Rect(0, rectMapNutContent.yMax, nutritionSettingsGroup.width, Text.LineHeight);
            Widgets.Label(nutritionPerPawnLabel, "RR_Mtw_NutritionPerPawnLabel".Translate() + ": " + (mapNutrition / (Find.CurrentMap.mapPawns.ColonistCount + Find.CurrentMap.mapPawns.SlavesOfColonySpawned.Count)).ToString("F1"));

            GUI.EndGroup();
        }

        internal static NutritionTable TotalHumanEdibleNutritionOfType(ResourceCounter rc)
        {
            NutritionTable nutritionTable = new NutritionTable();

            float num;

            Dictionary<ThingDef, int> dict = (Dictionary<ThingDef, int>)Traverse.Create(rc).Field("countedAmounts").GetValue();

            foreach (KeyValuePair<ThingDef, int> keyValuePair in dict)
            {
                if (keyValuePair.Key.IsNutritionGivingIngestible && keyValuePair.Key.ingestible.HumanEdible)
                {
                    num = keyValuePair.Key.GetStatValueAbstract(StatDefOf.Nutrition, null) * (float)keyValuePair.Value;

                    switch (keyValuePair.Key.ingestible.preferability)
                    {
                        case (FoodPreferability.Undefined):
                            nutritionTable.Undefined += num;
                            break;
                        case FoodPreferability.NeverForNutrition:
                            nutritionTable.NeverForNutrition += num;
                            break;
                        case FoodPreferability.DesperateOnly:
                            nutritionTable.DesperateOnly += num;
                            break;
                        case FoodPreferability.DesperateOnlyForHumanlikes:
                            nutritionTable.DesperateOnlyForHumanlikes += num;
                            break;
                        case FoodPreferability.RawBad:
                            nutritionTable.RawBad += num;
                            break;
                        case FoodPreferability.RawTasty:
                            nutritionTable.RawTasty += num;
                            break;
                        case FoodPreferability.MealAwful:
                            nutritionTable.MealAwful += num;
                            break;
                        case FoodPreferability.MealSimple:
                            nutritionTable.MealSimple += num;
                            break;
                        case FoodPreferability.MealFine:
                            nutritionTable.MealFine += num;
                            break;
                        case FoodPreferability.MealLavish:
                            nutritionTable.MealLavish += num;
                            break;
                        default:
                            Log.Warning($"{keyValuePair.Key.label} had unexpected food preferability!");
                            nutritionTable.Undefined += num;
                            break;
                    }
                }
            }

            return nutritionTable;
        }

        static MethodInfo checkboxDrawMI = typeof(Widgets).GetMethod("CheckboxDraw", BindingFlags.Public | BindingFlags.Static);
        delegate void SwitchActionCallback();
        static Dictionary<string, int> titleToIDpair = new Dictionary<string, int>();

        static void CheckboxLabeled(Rect rect, string labelTag, ref bool checkOn, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, bool placeCheckboxNearText = false, SwitchActionCallback action = null, String descriptionText = null)
        {
            String label = labelTag.Translate();

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (placeCheckboxNearText)
            {
                rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
            }
            Widgets.Label(rect, label);

            HandleTooltips(rect, label, descriptionText);

            if (!disabled && Widgets.ButtonInvisible(rect, true))
            {
                checkOn = !checkOn;
                if (checkOn)
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                }
                else
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
                }
                if (GeneralUtility.IsNotNull(action))
                    action();
            }
            checkboxDrawMI.Invoke(null, new object[] { rect.x + rect.width - 24f, rect.y, checkOn, disabled, 24f, null, null });
            Text.Anchor = anchor;
        }

        private static void HandleTooltips(Rect rect, string label, string descriptionText)
        {
            if (descriptionText != null && Mouse.IsOver(rect))
            {
                if (!titleToIDpair.ContainsKey(label))
                {
                    int tooltipHash = 0;
                    foreach (char c in label)
                        tooltipHash += c;

                    foreach (var pair in titleToIDpair)
                    {
                        if (pair.Value == tooltipHash)
                            Log.Warning($"ID collision found! {pair.Key} collides with {label}");
                    }

                    titleToIDpair.Add(label, tooltipHash);
                }

                TooltipHandler.TipRegion(rect, () => descriptionText.Translate(), titleToIDpair[label]);
            }
        }

        static void CheckMaxMinThresholds() 
        {
            if (GlobalSettings.minWeight.threshold >= GlobalSettings.maxWeight.threshold)
            {
                GlobalSettings.minWeight.threshold = (int)GlobalSettings.minWeight.min;
                GlobalSettings.minWeight.stringBuffer = null;
                GlobalSettings.maxWeight.threshold = (int)99999;
                GlobalSettings.maxWeight.stringBuffer = null;
            }
        }

        static void NumberFieldLabeledWithRect<T>(
            Rect categoryRect, ref NumericFieldData<T> numericFieldData, int positionNumberInList, string translationLabel , GameFont font = GameFont.Small, SwitchActionCallback action = null, String descriptionTextTag = null) where T : struct
        {
            T cachedVal = numericFieldData.threshold;

            Text.Font = font;
            Rect boundingRect = new Rect(0, categoryRect.y + positionNumberInList * spaceBetweenNumberFields, categoryRect.width - numberFieldRightOffset, Text.LineHeight);
            Widgets.Label(boundingRect, translationLabel.Translate() + ": ");
            Widgets.TextFieldNumeric<T>(
                new Rect(boundingRect.xMax - bufferForNumberFields, boundingRect.y, bufferForNumberFields, Text.LineHeight),
                ref numericFieldData.threshold,
                ref numericFieldData.stringBuffer,
                Prefs.DevMode ? int.MinValue : numericFieldData.min,
                Prefs.DevMode ? int.MaxValue : numericFieldData.max);

            T newVal = numericFieldData.threshold;

            HandleTooltips(boundingRect, translationLabel, descriptionTextTag);

            if (action != null && !object.Equals(newVal, cachedVal))
                action();
        }

        static void DirtyAllHediffSetCaches() 
        {
            List<Pawn> pawns = GeneralUtility.GetAllGlobalHumanlikePawns();
            foreach (Pawn pawn in pawns)
            {
                if (pawn?.health?.hediffSet is HediffSet h)
                {
                    h.DirtyCache();
                }
            }
        }

        static void RefreshMovementAbility() 
        {
            List<Pawn> pawns = GeneralUtility.GetAllGlobalHumanlikePawns();
            foreach (Pawn pawn in pawns)
            {
                if (Utilities.HediffUtility.GetHediffOfDefFrom(RimRound.Defs.HediffDefOf.RimRound_Weight, pawn) is Hediff h)
                {
                    pawn.health.Notify_HediffChanged(h);
                }
            }
        }
    }
}
