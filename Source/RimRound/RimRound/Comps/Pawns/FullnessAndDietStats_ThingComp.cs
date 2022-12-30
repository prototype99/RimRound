﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using RimRound.Utilities;
using RimRound.Defs;
using RimWorld;
using Verse.AI;
using RimRound.UI;
using UnityEngine;
using System.Reflection;

namespace RimRound.Comps
{
    public class FullnessAndDietStats_ThingComp : ThingComp
    {
        public FullnessAndDietStats_ThingComp() 
        {
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Pair<float, float> ranges = GetRanges();
                cachedSliderVal1 = ranges.First;
                cachedSliderVal2 = ranges.Second;
            }

            Scribe_Values.Look<bool>(ref defaultBodyTypeForced, "defaultBodyTypeForced", false);
            Scribe_Values.Look<float>(ref cachedSliderVal1, "cachedSliderPos1", -1);
            Scribe_Values.Look<float>(ref cachedSliderVal2, "cachedSliderPos2", -1);

            Scribe_Values.Look<DietMode>(ref dietMode,                     "dietMode",                        DietMode.Disabled);
            Scribe_Values.Look<float>(ref currentFullness,                 "currentFullness",                 0);
            Scribe_Values.Look<float>(ref softLimitPersonal,               "softLimit",                       defaultSoftLimit);
            Scribe_Values.Look<float>(ref currentFullnessToNutritionRatio, "currentFullnessToNutritionRatio", defaultFullnessToNutritionRatio);
            Scribe_Values.Look<float>(ref hardLimitAdditionalPercentage,   "hardLimitAdditionalPercentage",   defaultHardLimitAdditionalPercentage);
            Scribe_Values.Look<float>(ref personalStomachElasticity,       "personalStomachElasticity",       defaultPersonalStomachElasticity);
            Scribe_Values.Look<float>(ref globalDigestionRateBonusMult,    "digestionRateBonusMult",          1f);
            Scribe_Values.Look<float>(ref globalDigestionRateBonusFlat,    "digestionRateBonusFlat",          0f);
            Scribe_Values.Look<float>(ref personalDigestionRateMult,       "personalDigestionRateMult",       1f);
            Scribe_Values.Look<float>(ref personalDigestionRateFlat,       "personalDigestionRateFlat",       0f);
            Scribe_Values.Look<float>(ref consumedNutrition,               "consumedNutrition",               0f);

            ExposePerkLevels();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (GlobalSettings.showPawnDietManagementGizmo && ShouldShowWeightGizmo())
                yield return this.weightGizmo;

            if (Prefs.DevMode && GlobalSettings.showSpecialDebugSettings) 
            {
                yield return new Command_Action
                {
                    defaultLabel = "Fill stomach",
                    action = delegate ()
                    {
                        this.CurrentFullness = this.HardLimit - Values.MinRQ;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "Empty stomach",
                    action = delegate ()
                    {
                        this.CurrentFullness = 0;
                    }
                };
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (((Pawn)parent)?.RaceProps.Humanlike ?? false)
            {
                if (Utilities.HediffUtility.GetHediffOfDefFrom(Defs.HediffDefOf.RimRound_Weight, parent.AsPawn()) is null)
                    Utilities.HediffUtility.AddHediffOfDefTo(Defs.HediffDefOf.RimRound_Weight, parent.AsPawn());

                if (Utilities.HediffUtility.GetHediffOfDefFrom(Defs.HediffDefOf.RimRound_Fullness, parent.AsPawn()) is null)
                    Utilities.HediffUtility.AddHediffOfDefTo(Defs.HediffDefOf.RimRound_Fullness, parent.AsPawn());


                if (nutritionbar == null)
                    this.nutritionbar = new WeightGizmo_NutritionBar(((Pawn)parent));

                if (fullnessbar == null)
                    this.fullnessbar = new WeightGizmo_FullnessBar(((Pawn)parent));

                if (weightGizmo == null)
                    this.weightGizmo = new WeightGizmo(this);

                Update();
                SetRangesByValue(cachedSliderVal1, cachedSliderVal2);
                InitializePerks();
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.Spawned)
                return;

            ProcessWeightGainRequests(GlobalSettings.ticksBetweenWeightGainRequestProcess.threshold);
            ProcessWeightLossRequests(GlobalSettings.ticksBetweenWeightGainRequestProcess.threshold);

            if (parent?.IsHashIntervalTick(GlobalSettings.ticksPerHungerCheck.threshold) ?? false) 
            {
                float digestedAmt = DigestionTick() / CurrentFullnessToNutritionRatio;
                ((Pawn)parent).needs.food.CurLevel += digestedAmt;
                ConsumedNutrition += digestedAmt;
                ActiveWeightGainTick(digestedAmt);
                PassiveWeightLossTick();
                FullnessCheckTick();
                StomachGrowthTick();

                if (GlobalSettings.burstingEnabled)
                    RuptureStomachCheckTick();
            }
        }


        private List<string> _perkNamesForSaving = new List<string>();
        private List<int> _perkLevelValuesForSaving = new List<int>();
        private void ExposePerkLevels() 
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (perkLevels?.PerkToLevels == null)
                    return;

                _perkNamesForSaving.Clear();
                _perkLevelValuesForSaving.Clear();

                foreach (var x in perkLevels.PerkToLevels)
                {
                    _perkNamesForSaving.Add(x.Key);
                    _perkLevelValuesForSaving.Add(x.Value);
                }
                _perkLevelsToSpendForSaving = perkLevels.availablePoints;

                Scribe_Values.Look<int>(ref _perkLevelsToSpendForSaving, "perkLevelsToSpend", 0);
                Scribe_Collections.Look<string>(ref _perkNamesForSaving, "perkNamesForSaving", LookMode.Value);
                Scribe_Collections.Look<int>(ref _perkLevelValuesForSaving, "perkValuesForSaving", LookMode.Value);
            } 
            else if (Scribe.mode == LoadSaveMode.LoadingVars) 
            {
                if (_perkLevelValuesForSaving is null)
                    _perkLevelValuesForSaving = new List<int>();
                if (_perkNamesForSaving is null)
                    _perkNamesForSaving = new List<string>();

                Scribe_Values.Look<int>(ref _perkLevelsToSpendForSaving, "perkLevelsToSpend", 0);
                Scribe_Collections.Look<string>(ref _perkNamesForSaving, "perkNamesForSaving", LookMode.Value);
                Scribe_Collections.Look<int>(ref _perkLevelValuesForSaving, "perkValuesForSaving", LookMode.Value);
                // Initialize the perk levels dictionary somewhere else (like PostLoadInit())
            }

        }

        private void InitializePerks() 
        {
            if (perkLevels is null)
                perkLevels = new PerkLevels();

            perkLevels.PerkToLevels = new Dictionary<string, int>() { };

            for (int i = 0; i < Perks.basicPerks.Count; ++i)
            {
                perkLevels.PerkToLevels.Add(Perks.basicPerks[i].perkName, 0);
            }
            for (int i = 0; i < Perks.advancedPerks.Count; ++i)
            {
                perkLevels.PerkToLevels.Add(Perks.advancedPerks[i].perkName, 0);
            }
            for (int i = 0; i < Perks.elitePerks.Count; ++i)
            {
                perkLevels.PerkToLevels.Add(Perks.elitePerks[i].perkName, 0);
            }
            for (int i = 0; i < Perks.ultimatePerks.Count; ++i)
            {
                perkLevels.PerkToLevels.Add(Perks.ultimatePerks[i].perkName, 0);
            }

            // Set values from save
            if (_perkNamesForSaving is null || _perkLevelValuesForSaving is null || _perkNamesForSaving.Count() == 0 || _perkLevelValuesForSaving.Count() == 0)
                return;


            perkLevels.availablePoints = _perkLevelsToSpendForSaving;

            for (int i = 0; i < _perkNamesForSaving.Count(); ++i)
            {
                if (perkLevels.PerkToLevels.ContainsKey(_perkNamesForSaving[i]))
                    perkLevels.PerkToLevels[_perkNamesForSaving[i]] = _perkLevelValuesForSaving[i];
            }
        }

        public void ProcessWeightLossRequests(int ticksBetweenChecks) 
        {
            if (!GeneralUtility.IsHashIntervalTick(ticksBetweenChecks))
                return;

            if (this.activeWeightLossRequests.Count > 0)
            {
                int currentTick = Find.TickManager.TicksGame;
                if (this.activeWeightLossRequests.Peek().tickToApplyOn > currentTick)
                    return;

                WeightGainRequest gainRequest = this.activeWeightLossRequests.Dequeue();

                ChangeWeightAndUpdateSprite(gainRequest);
            }


        }

        public void ProcessWeightGainRequests(int ticksBetweenChecks) 
        {
            if (!GeneralUtility.IsHashIntervalTick(ticksBetweenChecks))
                return;

            if (this.activeWeightGainRequests.Count > 0)
            {
                int currentTick = Find.TickManager.TicksGame;
                if (this.activeWeightGainRequests.Peek().tickToApplyOn > currentTick)
                    return;

                WeightGainRequest gainRequest = this.activeWeightGainRequests.Dequeue();


                float gainedSeverity = ChangeWeightAndUpdateSprite(gainRequest);
                CreateWLRequestIfNonZeroDuration(currentTick, gainRequest, gainedSeverity);

            }

            return;
        }


        /// <summary>
        /// Actually applies weight gain request to pawn and updates sprite.
        /// </summary>
        /// <param name="gainRequest">Weight gain request to process</param>
        /// <returns>Change in severity</returns>
        private float ChangeWeightAndUpdateSprite(WeightGainRequest gainRequest) 
        {
            float cachedSeverity = this.parent.AsPawn().WeightHediff().Severity;

            Utilities.HediffUtility.AddHediffSeverity(
                 Defs.HediffDefOf.RimRound_Weight,
                 this.parent.AsPawn(),
                 Utilities.HediffUtility.KilosToSeverityWithoutBaseWeight(gainRequest.amountToGain),
                 false,
                 false);

            float newSeverity = this.parent.AsPawn().WeightHediff().Severity;

            var pbtThingComp = parent.TryGetComp<PawnBodyType_ThingComp>();
            if (pbtThingComp != null)
                BodyTypeUtility.UpdatePawnSprite(parent.AsPawn(), pbtThingComp.PersonallyExempt, pbtThingComp.CategoricallyExempt);

            return newSeverity - cachedSeverity;
        }


        private void CreateWLRequestIfNonZeroDuration(int currentTick, WeightGainRequest gainRequest, float severityChangeFromPriorGain)
        {
            if (gainRequest.duration > 0)
            {
                if (gainRequest.amountToGain > 0)
                    this.activeWeightLossRequests.Enqueue(new WeightGainRequest(-gainRequest.amountToGain, currentTick + gainRequest.duration, 0, gainRequest.triggerMessages));
                else 
                {
                    this.activeWeightLossRequests.Enqueue(new WeightGainRequest(Utilities.HediffUtility.SeverityToKilosWithoutBaseWeight(severityChangeFromPriorGain), currentTick + gainRequest.duration, 0, gainRequest.triggerMessages));
                }
            }
        }

        public void PassiveWeightLossTick() 
        {
            Utilities.HediffUtility.AddHediffSeverity(
                Defs.HediffDefOf.RimRound_Weight, 
                ((Pawn)parent),
                Utilities.HediffUtility.NutritionToSeverity(-1 * (float)(this.parent.AsPawn().needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed, true)) * GlobalSettings.ticksPerHungerCheck.threshold));
        }

        public void ActiveWeightGainTick(float nutrition) 
        {
            Utilities.HediffUtility.AddHediffSeverity(
                Defs.HediffDefOf.RimRound_Weight,
                ((Pawn)parent),
                Utilities.HediffUtility.NutritionToSeverity(nutrition));
        }

        public void FullnessCheckTick() 
        {

            float severity = (CurrentFullness > 0 ? CurrentFullness / HardLimit : 0.01f);
            Utilities.HediffUtility.SetHediffSeverity(Defs.HediffDefOf.RimRound_Fullness, (Pawn)parent, severity);

            return;
        }

        public void RuptureStomachCheckTick() 
        {
            float severity = (CurrentFullness > 0 ? CurrentFullness / HardLimit : 0.01f);

            if (severity > Defs.HediffDefOf.RimRound_Fullness.stages.Last().minSeverity)
            {
                float vomitChance = Values.RandomFloat(0, 1);
                if (vomitChance >= 0.50)
                    ((Pawn)parent).jobs.StartJob(
                        JobMaker.MakeJob(RimWorld.JobDefOf.Vomit),
                        JobCondition.InterruptForced,
                        null, true, true, null, null, false, false);

                RuptureStomach();

                CurrentFullness = SoftLimit * (1 - Values.RandomFloat(0.1f, 0.4f));
            }

            return;
        }

        public void StomachGrowthTick()
        {
            if (CurrentFullness > SoftLimit)
            {
                SoftLimit += GlobalSettings.stomachElasticityMultiplier.threshold * ((perkLevels.PerkToLevels?["RR_Endless_Indulgence_Title"] ?? 0) + 1) * personalStomachElasticity * baseStomachElasticity * GlobalSettings.ticksPerHungerCheck.threshold;
                return;
            }
            return;
        }

        public float DigestionTick()
        {
            float amountToSubtract = DigestionRate * GlobalSettings.ticksPerHungerCheck.threshold;

            if (CurrentFullness > amountToSubtract)
            {
                CurrentFullness -= amountToSubtract;
                return amountToSubtract;
            }
            else
            {
                float tmp = CurrentFullness;
                CurrentFullness = 0;
                return tmp;
            }
        }

        public void RuptureStomach() 
        {
            BodyPartRecord pawnStomach = ((Pawn)parent).RaceProps.body.GetPartsWithDef(BodyPartDefOf.Stomach).First();
            float currentStomachHealth = ((Pawn)parent).health.hediffSet.GetPartHealth(pawnStomach);
            float afterRuptureStomachHealth = 2;

            if (currentStomachHealth <= afterRuptureStomachHealth)
                return;

            int numberOfWounds = Values.RandomInt(1, 5);
            float damageVariationPercent = 0.50f;
            float remainingDamage = (currentStomachHealth - afterRuptureStomachHealth);
            float damagePerWound = remainingDamage / numberOfWounds;

            while (remainingDamage > 0) 
            {
                float thisDmg = Values.RandomFloat(1 - damageVariationPercent, 1 + damageVariationPercent) * damagePerWound;

                if (thisDmg > remainingDamage)
                    thisDmg = remainingDamage;

                DamageInfo stomachRuptureDamageInfo = new DamageInfo(
                    Defs.DamageDefOf.RR_StomachBurst,
                    thisDmg, 0, -1, null, pawnStomach);

                remainingDamage -= thisDmg;

                ((Pawn)parent).TakeDamage(stomachRuptureDamageInfo);
            }
        }

        private float currentFullnessToNutritionRatio = defaultFullnessToNutritionRatio;

        public float CurrentFullnessToNutritionRatio 
        {
            get 
            {
                return currentFullnessToNutritionRatio;
            }
            set 
            {
                currentFullnessToNutritionRatio = value;
            }
        }

        public void UpdateRatio(float incomingNutrition, float incomingRatio = defaultFullnessToNutritionRatio) 
        {
            //Weighted average of current values and incoming values  
            CurrentFullnessToNutritionRatio =
                (CurrentFullness + incomingRatio * incomingNutrition) /
                ((CurrentFullness / CurrentFullnessToNutritionRatio) + incomingNutrition);
        }

        public float DigestionRate
        {
            get
            {
                return (
                    (float)(this.parent.AsPawn().needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed, true) * 
                    GlobalSettings.digestionRateMultiplier.threshold *
                    PersonalDigestionRateMult *
                    GlobalDigestionRateBonusMult * 
                    baseDigestionRate) +
                    PersonalDigestionRateFlat +
                    GlobalDigestionRateBonusFlat);
            }
        }


        public DietMode DietMode
        {
            get
            {
                return dietMode;
            }
            set
            {
                dietMode = value;
                this.Update();
            }

        }

        private DietMode dietMode = DietMode.Nutrition;

        public DietMode preCaravanDietMode;

        public bool defaultBodyTypeForced = false;

        private BodyTypeDef defaultBodyType;

        public BodyTypeDef DefaultBodyType 
        {
            get 
            {
                if (defaultBodyType is null || BodyTypeUtility.IsRRBody(defaultBodyType))
                {

                    defaultBodyType = RimWorld.BodyTypeDefOf.Thin;
                }
                return defaultBodyType;
            } 
            set => defaultBodyType = value; 
        }


        public float PersonalDigestionRateMult 
        {
            get 
            {
                float digestionBeyondQuestionMult = (perkLevels.PerkToLevels?["RR_Digestion_Beyond_Question_Title"] ?? 0) * 0.3f + 1;
                float gigaGurglingMult = (perkLevels.PerkToLevels?["RR_GigaGurgling_Title"] ?? 0) * 1f + 1;
                return gigaGurglingMult * digestionBeyondQuestionMult * HungerDroneUtility.GetCurrentHungerMultiplierFromDrone(this.parent.AsPawn());
            }
            set => personalDigestionRateMult = value;
        }
        private float personalDigestionRateMult = 1f;

        public float PersonalDigestionRateFlat
        {
            get 
            {
                return personalDigestionRateFlat;
            }
            set => personalDigestionRateFlat = value;
        }
        private float personalDigestionRateFlat = 0f;


        public float GlobalDigestionRateBonusMult 
        {
            get => globalDigestionRateBonusMult;
            set => globalDigestionRateBonusMult = value;
        }
        private static float globalDigestionRateBonusMult = 1f;

        public float GlobalDigestionRateBonusFlat
        {
            get => globalDigestionRateBonusFlat;
            set => globalDigestionRateBonusFlat = value;
        }
        private static float globalDigestionRateBonusFlat = 0f;


        public float PersonalWeightGainModifier 
        {
            get 
            {
                int apexAbsorbtionLevel = perkLevels.PerkToLevels?["RR_Apex_Absorption_Title"] ?? 0;
                int wg4000Level = perkLevels.PerkToLevels?["RR_WeightGain4000_Title"] ?? 0;
                int makesAllTheRulesLevel = perkLevels.PerkToLevels?["RR_Breakneck_Buffet_Title"] ?? 0;
                int heavyRevianLevel = perkLevels.PerkToLevels?["RR_HeavyRevian_Title"] ?? 0;

                if (makesAllTheRulesLevel > 0)
                {
                    Map currentMap = this.parent.Map;
                    if (currentMap != null)
                    {
                        Vector2 vector = Find.WorldGrid.LongLatOf(currentMap.Tile);

                        if (!(GenDate.HourFloat(Find.TickManager.TicksAbs, vector.x) is float time &&
                            (time >= 18.0 ||
                            time <= 6.0)))
                        {
                            makesAllTheRulesLevel = 0;
                        }
                    }
                }

                return _personalWeightGainModifier + 
                    0.1f * apexAbsorbtionLevel +
                    0.2f * wg4000Level +
                    0.5f * heavyRevianLevel +
                    4.0f * makesAllTheRulesLevel;
            }
            set 
            {
                _personalWeightGainModifier = value;
            }
        }

        float _personalWeightGainModifier = 1f;




        public float PersonalWeightLossModifier
        {
            get
            {
                int dietPlanLevel = perkLevels.PerkToLevels?["RR_Diet_Plan_Title"] ?? 0;
                return _personalWeightLossModifier +
                    0.2f * dietPlanLevel;
            }
            set
            {
                _personalWeightLossModifier = value;
            }
        }

        float _personalWeightLossModifier = 1f;




        public float RemainingFullnessUntil(float limit) 
        {
            return limit - CurrentFullness;
        }


        //Current actual fullness. Displayed as the bar
        private float currentFullness = 0.0f;

        public float CurrentFullness
        {
            get
            {
                return currentFullness;
            }
            set
            {
                currentFullness = value;
            }
        }


        //-------------------

        //In liters. represents threshold of stoach capacity. 
        private float softLimitPersonal = defaultSoftLimit + Values.RandomFloat(softLimitVariation.x, softLimitVariation.y);
        public float SoftLimit 
        {
            get 
            {
                return softLimitPersonal * GlobalSettings.softLimitMuliplier.threshold;
            }
            set 
            {
                softLimitPersonal = value;
            }
        }

        public bool SetAboveHardLimit 
        {
            get 
            {
                return fullnessbar.peaceForeverHeld;
            }
        }

        //How much more than the soft limit the Hard Limit is.
        public float hardLimitAdditionalPercentage = defaultHardLimitAdditionalPercentage;
        public float HardLimit
        {
            get
            {
                float limitBreakMult = (perkLevels.PerkToLevels?["RR_LimitBreak_Title"] ?? 0) * 0.1f + 1;
                return SoftLimit * (1f + hardLimitAdditionalPercentage) * GlobalSettings.hardLimitMuliplier.threshold * limitBreakMult;
            }
            set 
            {
                if (value <= SoftLimit) 
                {
                    Log.Error("Hard Limit set below soft limit!");
                }

                hardLimitAdditionalPercentage = (value / SoftLimit) - 1f;
            }
        }



        //Multiplier for how quickly the stomach grows when over the soft limit.
        public float personalStomachElasticity = defaultPersonalStomachElasticity;


        bool ShouldShowWeightGizmo()
        {
            List<object> selectedPawns = new List<object>();
            foreach (object o in Find.Selector.SelectedObjects)
            {
                if (o.AsPawn() is null)
                {
                    continue;
                }
                else
                {
                    selectedPawns.Add(o);
                }
            }

            if (selectedPawns.Count > 1)
                return false;
            else
                return true;
        }

        public Pair<float, float> GetRanges()
        {
            if (nutritionbar is null || fullnessbar is null)
                return new Pair<float, float>(-1, -1);

            switch (this.DietMode)
            {
                case DietMode.Nutrition:
                    return nutritionbar.GetRanges();
                case DietMode.Hybrid:
                    return new Pair<float, float>(
                        nutritionbar.GetRanges().First,
                        fullnessbar.GetRanges().First);
                case DietMode.Fullness:
                    return fullnessbar.GetRanges();
                case DietMode.Disabled:
                    return new Pair<float, float>(-1, -1);
                default:
                    Log.Error("WeightGizmo_ThingComp GetRanges() ran default case!");
                    return new Pair<float, float>(-1, -1);
            }
        }


        public void SetRangesByValue(float first, float second) 
        {
            float maxNutrition = nutritionbar.needFood.MaxLevel;
            float maxDisplayFullness = fullnessbar.DisplayLimit;
            switch (this.DietMode)
            {
                case DietMode.Nutrition:
                    nutritionbar.SetRanges(first / maxNutrition, second / maxNutrition);
                    return;
                case DietMode.Hybrid:
                    nutritionbar.SetRanges(first / maxNutrition, 0);
                    fullnessbar.SetRanges(second / maxDisplayFullness, 0);
                    return;
                case DietMode.Fullness:
                    fullnessbar.SetRanges(first / maxDisplayFullness, second / maxDisplayFullness);
                    return;
                case DietMode.Disabled:
                    return;
                default:
                    Log.Error("WeightGizmo_ThingComp SetRanges() ran default case!");
                    return;
            }

        }
        public void SetRangesByPercent(float first, float second)
        {
            switch (this.DietMode)
            {
                case DietMode.Nutrition:
                    if (nutritionbar != null)
                        nutritionbar.SetRanges(first, second);
                    return;
                case DietMode.Hybrid:
                    if (nutritionbar != null)
                        nutritionbar.SetRanges(first, 0);
                    if (fullnessbar != null)
                        fullnessbar.SetRanges(second, 0);
                    return;
                case DietMode.Fullness:
                    if (fullnessbar != null)
                        fullnessbar.SetRanges(first, second);
                    return;
                case DietMode.Disabled:
                    return;
                default:
                    Log.Error("WeightGizmo_ThingComp SetRanges() ran default case!");
                    return;
            }
        }


        //100% for fullness is hardlimit
        public void SetRangesPercent(float first, float second) 
        {
            if (first > 1 || second > 1)
            {
                Log.Error("Inputs for SetRangesPercent must be less than or equal to 1!");
                return;
            }
                
            float maxNutrition = parent.AsPawn().needs.food.MaxLevel;
            float maxFullness = this.HardLimit;

            switch (this.DietMode)
            {
                case DietMode.Nutrition:
                    nutritionbar.SetRanges(first * maxNutrition, second * maxNutrition);
                    return;
                case DietMode.Hybrid:
                    nutritionbar.SetRanges(first * maxNutrition, 0);
                    fullnessbar.SetRanges(second * maxFullness, 0);
                    return;
                case DietMode.Fullness:
                    fullnessbar.SetRanges(first * maxFullness, second * maxFullness);
                    return;
                case DietMode.Disabled:
                    return;
                default:
                    Log.Error("WeightGizmo_ThingComp SetRanges() ran default case!");
                    return;
            }

        }

        public void Update()
        {
            if (nutritionbar != null)
                nutritionbar.UpdateBar(this.DietMode);
            if (fullnessbar != null)
                fullnessbar.UpdateBar(this.DietMode);
        }

        public float ConsumedNutrition
        {
            get => consumedNutrition;
            set 
            {
                consumedNutrition = value;
                if (consumedNutrition >= perkLevels.nutritionPerLevel * perkLevels.currentLevel)
                {
                    int currentLevel = Mathf.FloorToInt(consumedNutrition / perkLevels.nutritionPerLevel) + 1;
                    perkLevels.currentLevel = currentLevel;
                    perkLevels.availablePoints += 1;
                }
            } 
        }
        private float consumedNutrition = 0;
        private int _perkLevelsToSpendForSaving = 0;


        public Queue<WeightGainRequest> activeWeightGainRequests = new Queue<WeightGainRequest>();
        public Queue<WeightGainRequest> activeWeightLossRequests = new Queue<WeightGainRequest>();

        public PerkLevels perkLevels = new PerkLevels();

        public WeightGizmo weightGizmo;
        public WeightGizmo_FullnessBar fullnessbar;
        public WeightGizmo_NutritionBar nutritionbar;

        float cachedSliderVal1 = 0.30f;
        float cachedSliderVal2 = 0.90f;

        public const float defaultSoftLimit = 0.9f;
        public static Vector2 softLimitVariation = new Vector2(-0.10f, 0.50f);
        public const float defaultHardLimitAdditionalPercentage = 0.3f;
        public const float defaultPersonalStomachElasticity = 1f;
        //How much (in liters) the stomach grows when over the softlimit per tick.
        public const float baseStomachElasticity = 0.00001f;
        public const float baseDigestionRate = 3.0f;
        public const float defaultFullnessToNutritionRatio = 1f; //i.e. 0.5 Fullness for 1 nutrition is 0.5f
    }

    public class PerkLevels
    {
        public int currentLevel = 1;
        public int availablePoints = 0;
        public float nutritionPerLevel = 20;
        public Dictionary<string, int> PerkToLevels;
    }





    public struct WeightGainRequest
    {
        public WeightGainRequest(float amountToGain, int tickToApplyOn, int duration = 0, bool triggerMessages = false) 
        {
            this.amountToGain = amountToGain;
            this.tickToApplyOn = tickToApplyOn;
            this.duration = duration;
            this.triggerMessages = triggerMessages;
        }
        public float amountToGain;
        public int tickToApplyOn;
        public int duration;
        public bool triggerMessages;
    }


    public enum DietMode
    {
        Nutrition,
        Hybrid,
        Fullness,
        Disabled
    }
}
