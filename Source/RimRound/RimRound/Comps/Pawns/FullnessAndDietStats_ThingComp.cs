using System.Collections.Generic;
using System.Linq;
using Verse;

using RimRound.Utilities;
using RimWorld;
using Verse.AI;
using RimRound.UI;
using UnityEngine;
using Verse.Sound;
using RimWorld.Planet;
using RimRound.AI;

namespace RimRound.Comps
{
    public class FullnessAndDietStats_ThingComp : ThingComp
    {
        // Do not cache this; pawn needs can initialize after the comp is constructed/spawned
        public bool Disabled {
            get {
                Pawn pawn = parent.AsPawn();
                if (pawn == null) return true;
                if (!(pawn.RaceProps?.Humanlike ?? false)) return true;
                return pawn.needs?.food == null;
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            base.PostDeSpawn(map, mode);
            DespawnSustainers();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            DespawnSustainers();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Disabled) { return; }

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Pair<float, float> ranges = GetRanges();
                cachedSliderVal1 = ranges.First;
                cachedSliderVal2 = ranges.Second;
            }

            Scribe_Values.Look(ref defaultBodyTypeForced, "defaultBodyTypeForced");
            Scribe_Values.Look(ref _isConnectedToFeedingMachine, "isConnectedToFeedingMachine");

            Scribe_Values.Look(ref cachedSliderVal1, "cachedSliderPos1", -1);
            Scribe_Values.Look(ref cachedSliderVal2, "cachedSliderPos2", -1);

            Scribe_Values.Look(ref dietMode, "dietMode", DietMode.Disabled);
            Scribe_Values.Look(ref preCaravanDietMode, "preCaravanDietMode", DietMode.Disabled);
            Scribe_Values.Look(ref currentFullness, "currentFullness");
            Scribe_Values.Look(ref softLimitPersonal, "softLimit", defaultSoftLimit);
            Scribe_Values.Look(ref currentFullnessToNutritionRatio, "currentFullnessToNutritionRatio", defaultFullnessToNutritionRatio);
            Scribe_Values.Look(ref consumedNutrition, "consumedNutrition");
            Scribe_Values.Look(ref cumulativeSeverityGained, "suddenWGCumSeverity");

            Scribe_Values.Look(ref debugSoftLimitDelta, "debugSoftLimitDelta");

            ExposeStatBonuses();
            ExposePerkLevels();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                InitValuesOnLoad();
        }

        bool _loadedDietBars;
        private void InitValuesOnLoad()
        {
            InitBarsIfNull();
            UpdateDietBars();
            InitializePerksIfNull();
        }

        private void InitBarsIfNull()
        {
            if (parent.AsPawn().Dead || parent.AsPawn().needs?.food == null)
                return;

            if (nutritionbar == null)
                nutritionbar = new WeightGizmo_NutritionBar((Pawn)parent);

            if (fullnessbar == null)
                fullnessbar = new WeightGizmo_FullnessBar((Pawn)parent);

            if (weightGizmo == null)
                weightGizmo = new WeightGizmo(this);
        }

        public void ExposeStatBonuses()
        {

            Scribe_Values.Look(ref statBonuses.weightGainMultiplier, "weightGainMultiplier");
            Scribe_Values.Look(ref statBonuses.weightLossMultiplier, "weightLossMultiplier");
            Scribe_Values.Look(ref statBonuses.digestionRateMultiplier, "digestionRateMultiplier");
            Scribe_Values.Look(ref statBonuses.softLimitMultiplier, "softLimitMultiplier");
            Scribe_Values.Look(ref statBonuses.stomachElasticityMultiplier, "stomachElasticityMultiplier");
            Scribe_Values.Look(ref statBonuses.hardLimitAdditionalPercentageMultiplier, "hardLimitAdditionalPercentageMultiplier");

            Scribe_Values.Look(ref statBonuses.weightGainMultBonus, "weightGainBonus");
            Scribe_Values.Look(ref statBonuses.weightLossMultBonus, "weightLossBonus");


            Scribe_Values.Look(ref statBonuses.digestionRateFlatBonus, "digestionRateFlatBonus");
            Scribe_Values.Look(ref statBonuses.softLimitFlatBonus, "softLimitFlatBonus");
            Scribe_Values.Look(ref statBonuses.stomachElasticityFlatBonus, "stomachElasticityFlatBonus");

            Scribe_Values.Look(ref statBonuses.hardLimitAdditionalPercentageMultBonus, "hardLimitAdditionalPercentageBonus");
            Scribe_Values.Look(ref statBonuses.fullnessGainedMultBonus, "fullnessGainedMultiplierBonus");
            Scribe_Values.Look(ref statBonuses.movementFlatBonus, "flatMoveBonus");
            Scribe_Values.Look(ref statBonuses.manipulationFlatBonus, "flatManipulationBonus");
            Scribe_Values.Look(ref statBonuses.eatingFlatBonus, "flatEatingSpeedBonus");
            Scribe_Values.Look(ref statBonuses.movementPenaltyMitigationMultBonus_Weight, "movementPenaltyMitigationMultBonus_Weight");
            Scribe_Values.Look(ref statBonuses.movementPenaltyMitigationMultBonus_Fullness, "movementPenaltyMitigationMultBonus_Fullness");
            Scribe_Values.Look(ref statBonuses.eatingSpeedReductionMitigationMultBonus_Fullness, "eatingSpeedReductionMitigationMultBonus_Fullness");
            Scribe_Values.Look(ref statBonuses.manipulationPenaltyMitigationMultBonus_Weight, "manipulationPenaltyMitigationMultBonus_Weight");
            Scribe_Values.Look(ref statBonuses.painMitigationMultBonus_Fullness, "painMitigationMultBonus_Fullness");


            return;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Disabled || (!parent.AsPawn().IsColonist && !parent.AsPawn().IsPrisonerOfColony && !Prefs.DevMode)) { yield break; }

            if (GlobalSettings.showPawnDietManagementGizmo && ShouldShowWeightGizmo())
                yield return weightGizmo;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (Disabled) { return; }


            if (!(((Pawn)parent)?.RaceProps.Humanlike ?? false)) return;
            if (Utilities.HediffUtility.GetHediffOfDefFrom(Defs.HediffDefOf.RimRound_Weight, parent.AsPawn()) is null)
                Utilities.HediffUtility.AddHediffOfDefTo(Defs.HediffDefOf.RimRound_Weight, parent.AsPawn());

            if (Utilities.HediffUtility.GetHediffOfDefFrom(Defs.HediffDefOf.RimRound_Fullness, parent.AsPawn()) is null)
                Utilities.HediffUtility.AddHediffOfDefTo(Defs.HediffDefOf.RimRound_Fullness, parent.AsPawn());

            InitValuesOnLoad();

            // If this is called before InitValuesOnLoad(), sliders will be set to incorrect postions.
            if (!_loadedDietBars)
            {
                SetRangesByValue(cachedSliderVal1, cachedSliderVal2);
                _loadedDietBars = true;
            }

            HistoryAutoRecorderGroupWeight.Instance().AddHistoryRecorders(parent.AsPawn());
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Disabled) { return; }

            if (!parent.Spawned && !parent.AsPawn().IsCaravanMember())
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

            if (parent.IsHashIntervalTick(60))
                CumulativeSeverityKilosGained -= immunitySeverityDecay;


            if (parent == null || !parent.Spawned) return;
            DoFootstepSounds();
            DoEmptyStomachSounds();
            DoGurgleSounds();

            if (!IsConnectedToFeedingMachine)
            {
                DoBurpSounds();
            }

            DoStomachStretchSounds();

            DoSloshSounds();

            if (!IsConnectedToFeedingMachine && parent.IsHashIntervalTick(TICK_CHECK_INTERVAL_FOR_BREATHING))
            {
                DoBreathingSounds();
            }
        }

        private void DespawnSustainers() 
        {
            sloshSound?.End();
            breathSound?.End();

            sloshSound = null;
            breathSound = null;
        }

        private void DoStomachStretchSounds()
        {
            const float SECONDS_BETWEEN_STRETCH_SOUNDS = 4;
            SoundDef soundDef = SoundUtility.GetStomachStretchingSoundByFullness(this);
            SoundUtility.PlayOneShotForPawnIfNotWaiting(parent.AsPawn(), soundDef, SECONDS_BETWEEN_STRETCH_SOUNDS);
        }

        private bool _isConnectedToFeedingMachine;
        public bool IsConnectedToFeedingMachine 
        {
            get => _isConnectedToFeedingMachine;
            set => _isConnectedToFeedingMachine = value;
        }

        const int TICK_CHECK_INTERVAL_FOR_BREATHING = 60 * 3;
        private void DoBreathingSounds()
        {
            BodyTypeDef thresholdForBreathingAllTheTime = Defs.BodyTypeDefOf.F_050_MorbidlyObese;
            const float fullnessPercentForBreathing = 1f; // As percent of soft limit

            if (!IsConnectedToFeedingMachine && ((parent?.AsPawn()?.pather?.Moving ?? false) ||
                (fullnessbar?.CurrentFullnessAsPercentOfSoftLimit ?? 0) >= fullnessPercentForBreathing ||
                BodyTypeUtility.PawnIsOverWeightThreshold(parent.AsPawn(), thresholdForBreathingAllTheTime)))
            {
                if (breathSound != null && !breathSound.Ended) return;
                SoundDef soundDef = SoundUtility.GetBreathingSoundByWeightOpinionAndGender(parent.AsPawn());
                breathSound = soundDef.TrySpawnSustainer(SoundInfo.InMap(parent.AsPawn()));
            }
            else
            {
                breathSound?.End();
                breathSound = null;
            }
        }

        private void DoSloshSounds() 
        {
            if (!parent.Spawned)
            { 
                return; 
            }

            const int TICKS_PER_SECOND = 60;
            if (SloshDurationSeconds <= 0) 
            {
                return;
            }

            if (SloshStartTick + SloshDurationSeconds * TICKS_PER_SECOND <= Find.TickManager.TicksAbs)
            {
                SloshStartTick = 0;
                SloshDurationSeconds = 0;
                sloshSound?.End();
                sloshSound = null;
                return;
            }

            if (sloshSound != null) return;
            SoundDef sound = SoundUtility.GetStomachSloshByWeight(this);
            if (sound != null && SoundUtility.PawnShouldPlaySound(parent.AsPawn()))
            {
                sloshSound = sound.TrySpawnSustainer(SoundInfo.InMap(parent.AsPawn()));
            }
        }


        private void DoGurgleSounds()
        {
            const float SECONDS_BETWEEN_GURGLE_SOUNDS = 10;
            SoundDef soundDef = SoundUtility.GetStomachGurgleSoundsByWeight(this);
            SoundUtility.PlayOneShotForPawnIfNotWaiting(parent.AsPawn(), soundDef, SECONDS_BETWEEN_GURGLE_SOUNDS);
        }

        private void DoBurpSounds() 
        {
            const float PERCENT_VARIATION = 0.4f;
            float randomDelay = (1 + ((float)Values.random.NextDouble() - 0.5f) * PERCENT_VARIATION) * GlobalSettings.soundBurpDelaySeconds.threshold;
            SoundDef soundDef = SoundUtility.GetBurpSoundsByWeight(this);
            SoundUtility.PlayOneShotForPawnIfNotWaiting(parent.AsPawn(), soundDef, randomDelay);
        }

        private void DoEmptyStomachSounds()
        {
            const float SECONDS_BETWEEN_EMPTY_SOUND = 10;
            SoundDef soundDef = SoundUtility.GetEmptyStomachSoundsByWeight(this);
            SoundUtility.PlayOneShotForPawnIfNotWaiting(parent.AsPawn(), soundDef, SECONDS_BETWEEN_EMPTY_SOUND);
        }

        private void DoFootstepSounds()
        {
            SoundDef footstepSound = SoundUtility.GetFootStepSoundsByWeightAndMovement(this);
            footstepSound?.PlayOneShot(SoundInfo.InMap(new TargetInfo(parent)));
        }

        private List<string> _perkNamesForSaving = new List<string>();
        private List<int> _perkLevelValuesForSaving = new List<int>();

        private void ExposePerkLevels()
        {
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving when perkLevels?.PerkToLevels == null:
                    return;
                case LoadSaveMode.Saving:
                {
                    _perkNamesForSaving = new List<string>();
                    _perkLevelValuesForSaving = new List<int>();

                    foreach (KeyValuePair<string, int> x in perkLevels.PerkToLevels)
                    {
                        _perkNamesForSaving.Add(x.Key);
                        _perkLevelValuesForSaving.Add(x.Value);
                    }
                    _perkLevelsToSpendForSaving = perkLevels.availablePoints;
                    _currentLevelForSaving = perkLevels.currentLevel;

                    Scribe_Values.Look(ref _currentLevelForSaving, "currentLevelForSaving");
                    Scribe_Values.Look(ref _perkLevelsToSpendForSaving, "perkLevelsToSpend");
                    Scribe_Collections.Look(ref _perkNamesForSaving, "perkNamesForSaving", LookMode.Value);
                    Scribe_Collections.Look(ref _perkLevelValuesForSaving, "perkValuesForSaving", LookMode.Value);
                    break;
                }
                case LoadSaveMode.LoadingVars:
                {
                    if (_perkLevelValuesForSaving is null)
                        _perkLevelValuesForSaving = new List<int>();
                    if (_perkNamesForSaving is null)
                        _perkNamesForSaving = new List<string>();

                    Scribe_Values.Look(ref _currentLevelForSaving, "currentLevelForSaving");
                    Scribe_Values.Look(ref _perkLevelsToSpendForSaving, "perkLevelsToSpend");
                    Scribe_Collections.Look(ref _perkNamesForSaving, "perkNamesForSaving", LookMode.Value);
                    Scribe_Collections.Look(ref _perkLevelValuesForSaving, "perkValuesForSaving", LookMode.Value);
                    // Initialize the perk levels dictionary somewhere else (like PostLoadInit())
                    break;
                }
            }
        }

        private void InitializePerksIfNull()
        {
            if (perkLevels is null)
                perkLevels = new PerkLevels();

            if (!(perkLevels.PerkToLevels is null))
                return;

            perkLevels.PerkToLevels = new Dictionary<string, int>();

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
            if (_perkNamesForSaving is null || _perkLevelValuesForSaving is null || !_perkNamesForSaving.Any() || !_perkLevelValuesForSaving.Any())
                return;


            perkLevels.availablePoints = _perkLevelsToSpendForSaving;
            perkLevels.currentLevel = _currentLevelForSaving;

            for (int i = 0; i < _perkNamesForSaving.Count; ++i)
            {
                if (perkLevels.PerkToLevels.ContainsKey(_perkNamesForSaving[i]))
                    perkLevels.PerkToLevels[_perkNamesForSaving[i]] = _perkLevelValuesForSaving[i];
            }
        }

        public void ProcessWeightLossRequests(int ticksBetweenChecks)
        {
            if (!parent.IsHashIntervalTick(ticksBetweenChecks))
                return;

            if (activeWeightLossRequests.Count <= 0) return;
            int currentTick = Find.TickManager.TicksGame;
            if (activeWeightLossRequests.Peek().tickToApplyOn > currentTick)
                return;

            WeightGainRequest gainRequest = activeWeightLossRequests.Dequeue();

            ChangeWeightAndUpdateSprite(gainRequest);


        }

        public void ProcessWeightGainRequests(int ticksBetweenChecks)
        {
            if (!parent.IsHashIntervalTick(ticksBetweenChecks))
                return;

            if (activeWeightGainRequests.Count <= 0) return;
            int currentTick = Find.TickManager.TicksGame;
            if (activeWeightGainRequests.Peek().tickToApplyOn > currentTick)
                return;

            WeightGainRequest gainRequest = activeWeightGainRequests.Dequeue();


            float gainedSeverity = ChangeWeightAndUpdateSprite(gainRequest);
            CreateWLRequestIfNonZeroDuration(currentTick, gainRequest, gainedSeverity);
        }


        /// <summary>
        /// Actually applies weight gain request to pawn and updates sprite.
        /// </summary>
        /// <param name="gainRequest">Weight gain request to process</param>
        /// <returns>Change in severity</returns>
        private float ChangeWeightAndUpdateSprite(WeightGainRequest gainRequest)
        {
            float actualGainedSeverity = Utilities.HediffUtility.AddHediffSeverity(
                 Defs.HediffDefOf.RimRound_Weight,
                 parent.AsPawn(),
                 Utilities.HediffUtility.KilosToSeverityWithoutBaseWeight(gainRequest.amountToGain),
                 false,
                 false,
                 gainRequest.useMultipliers);

            PawnBodyType_ThingComp pbtThingComp = parent.TryGetComp<PawnBodyType_ThingComp>();
            if (pbtThingComp == null) return actualGainedSeverity;
            bool bodyUpdated = BodyTypeUtility.UpdatePawnSprite(parent.AsPawn(), pbtThingComp.PersonallyExempt, pbtThingComp.CategoricallyExempt);

            if (!bodyUpdated) return actualGainedSeverity;
            SoundDef bwomfSound = SoundUtility.GetBwomfSoundByWeight(parent.AsPawn());
            if (SoundUtility.PawnShouldPlaySound(parent.AsPawn()))
            {
                bwomfSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(parent)));
            }

            return actualGainedSeverity;
        }


        private void CreateWLRequestIfNonZeroDuration(int currentTick, WeightGainRequest gainRequest, float severityChangeFromPriorGain)
        {
            if (gainRequest.duration > 0)
            {
                activeWeightLossRequests.Enqueue(
                    new WeightGainRequest(
                        -1*Utilities.HediffUtility.SeverityToKilosWithoutBaseWeight(severityChangeFromPriorGain), 
                        currentTick + gainRequest.duration, 
                        0, 
                        gainRequest.triggerMessages,
                        false));
            }
        }

        public void PassiveWeightLossTick()
        {
            Utilities.HediffUtility.AddHediffSeverity(
                Defs.HediffDefOf.RimRound_Weight,
                (Pawn)parent,
                Utilities.HediffUtility.NutritionToSeverity(-1 * parent.AsPawn().needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed, true) * GlobalSettings.ticksPerHungerCheck.threshold));
        }

        public void ActiveWeightGainTick(float nutrition)
        {
            Utilities.HediffUtility.AddHediffSeverity(
                Defs.HediffDefOf.RimRound_Weight,
                (Pawn)parent,
                Utilities.HediffUtility.NutritionToSeverity(nutrition));
        }

        public void FullnessCheckTick()
        {

            float severity = CurrentFullness > 0 ? CurrentFullness / HardLimit : 0.01f;
            Utilities.HediffUtility.SetHediffSeverity(Defs.HediffDefOf.RimRound_Fullness, (Pawn)parent, severity);
        }

        public void RuptureStomachCheckTick()
        {
            float severity = CurrentFullness > 0 ? CurrentFullness / HardLimit : 0.01f;

            if (!(severity > Defs.HediffDefOf.RimRound_Fullness.stages.Last().minSeverity)) return;
            float vomitChance = Values.RandomFloat(0, 1);
            if (vomitChance >= 0.50)
                ((Pawn)parent).jobs.StartJob(
                    JobMaker.MakeJob(JobDefOf.Vomit),
                    JobCondition.InterruptForced,
                    null, true);

            RuptureStomach();

            CurrentFullness = SoftLimit * (1 - Values.RandomFloat(0.1f, 0.4f));
        }

        public void StomachGrowthTick()
        {
            if (CurrentFullness > SoftLimit)
                softLimitPersonal += StomachElasticity * GlobalSettings.ticksPerHungerCheck.threshold;
        }

        public float DigestionTick()
        {
            float amountToSubtract = DigestionRate * GlobalSettings.ticksPerHungerCheck.threshold;

            if (CurrentFullness > amountToSubtract)
            {
                CurrentFullness -= amountToSubtract;
                return amountToSubtract;
            }

            float tmp = CurrentFullness;
            CurrentFullness = 0;
            return tmp;
        }

        public void RuptureStomach()
        {
            BodyPartRecord pawnStomach = ((Pawn)parent).RaceProps.body.GetPartsWithDef(DefDatabase<BodyPartDef>.GetNamed("Stomach")).First();
            float currentStomachHealth = ((Pawn)parent).health.hediffSet.GetPartHealth(pawnStomach);
            const float afterRuptureStomachHealth = 2;

            if (currentStomachHealth <= afterRuptureStomachHealth)
                return;

            int numberOfWounds = Values.RandomInt(1, 5);
            const float damageVariationPercent = 0.50f;
            float remainingDamage = currentStomachHealth - afterRuptureStomachHealth;
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
            get => currentFullnessToNutritionRatio;
            set => currentFullnessToNutritionRatio = value;
        }

        public void UpdateRatio(float incomingNutrition, float incomingRatio = defaultFullnessToNutritionRatio)
        {
            //Weighted average of current values and incoming values  
            CurrentFullnessToNutritionRatio =
                (CurrentFullness + incomingRatio * incomingNutrition) /
                (CurrentFullness / CurrentFullnessToNutritionRatio + incomingNutrition);
        }


        public void AddFullnessCapacity(float liters) {
            softLimitPersonal = Mathf.Max(softLimitPersonal + liters, 0.5f);
        }

        public DietMode DietMode
        {
            get => dietMode;
            set
            {
                dietMode = value;
                UpdateDietBars();
            }

        }

        private DietMode dietMode = DietMode.Nutrition;

        public DietMode preCaravanDietMode = DietMode.Nutrition;

        public bool defaultBodyTypeForced;

        private BodyTypeDef defaultBodyType;

        public BodyTypeDef DefaultBodyType
        {
            get
            {
                if (defaultBodyType is null || BodyTypeUtility.IsRRBody(defaultBodyType))
                {

                    defaultBodyType = BodyTypeDefOf.Thin;
                }
                return defaultBodyType;
            }
            set => defaultBodyType = value;
        }

        //How much (in liters) the stomach grows when over the softlimit per tick.
        public float StomachElasticity
        {
            get
            {
                const float baseStomachElasticity = 0.00001f;
                float endlessIndulgenceMultBonus = perkLevels.PerkToLevels?["RR_Endless_Indulgence_Title"] ?? 0;
                float elasticityValue = baseStomachElasticity *
                        (GlobalSettings.stomachElasticityMultiplier.threshold *
                        (1 + endlessIndulgenceMultBonus) *
                        (1 + statBonuses.stomachElasticityMultiplier)) +
                        statBonuses.stomachElasticityFlatBonus;

                return Mathf.Clamp(elasticityValue, 0, float.MaxValue);
            }
        }

        public float DigestionRate
        {
            get
            {
                float digestionBeyondQuestionMult = (perkLevels.PerkToLevels?["RR_Digestion_Beyond_Question_Title"] ?? 0) * 0.2f;
                float gigaGurglingMult = (perkLevels.PerkToLevels?["RR_GigaGurgling_Title"] ?? 0) * 0.5f;
                float titaniumStomachMultBonus = (perkLevels.PerkToLevels?["RR_TitaniumStomach_Title"] ?? 0) * 1f;

                const float baseDigestionRate = 3.0f;

                return Mathf.Clamp(
                        GlobalSettings.digestionRateMultiplier.threshold *
                        baseDigestionRate *
                        parent.AsPawn().needs.food.FoodFallPerTickAssumingCategory(HungerCategory.Fed, true) *
                        (1 + statBonuses.digestionRateMultiplier + gigaGurglingMult + digestionBeyondQuestionMult + titaniumStomachMultBonus) *
                        HungerDroneUtility.GetCurrentHungerMultiplierFromDrone(parent.AsPawn()) +
                        statBonuses.digestionRateFlatBonus,
                    0,
                    float.MaxValue);
            }
        }

        public float WeightGainMultiplier
        {
            get
            {
                int apexAbsorbtionLevel = perkLevels.PerkToLevels?["RR_Apex_Absorption_Title"] ?? 0;
                int wg4000Level = perkLevels.PerkToLevels?["RR_WeightGain4000_Title"] ?? 0;
                int makesAllTheRulesLevel = perkLevels.PerkToLevels?["RR_MakesAllTheRules_Title"] ?? 0;
                int heavyRevianLevel = perkLevels.PerkToLevels?["RR_HeavyRevian_Title"] ?? 0;
                int thatLevel = perkLevels.PerkToLevels?["RR_That_Title"] ?? 0;
                int inescapableInfinityLevel = perkLevels.PerkToLevels?["RR_InescapableInfinity_Title"] ?? 0;
                int peakEvolutionLevel = perkLevels.PerkToLevels?["RR_PeakEvolution_Title"] ?? 0;

                // ReSharper disable once InvertIf
                if (makesAllTheRulesLevel > 0)
                {
                    Map currentMap = parent.Map;
                    // ReSharper disable once InvertIf
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

                return Mathf.Clamp((1 + statBonuses.weightGainMultiplier) *
                        GlobalSettings.weightGainMultiplier.threshold *
                        (parent.AsPawn().gender == Gender.Female ?
                            GlobalSettings.weightGainMultiplierFemale.threshold :
                            GlobalSettings.weightGainMultiplierMale.threshold) +
                        (statBonuses.weightGainMultBonus +
                        0.1f * apexAbsorbtionLevel +
                        0.2f * wg4000Level +
                        0.5f * heavyRevianLevel +
                        1.0f * peakEvolutionLevel +
                        4.0f * makesAllTheRulesLevel +
                        2.0f * thatLevel +
                        5.0f * inescapableInfinityLevel),
                    0, float.MaxValue);
            }
        }


        public float WeightLossMultiplier
        {
            get
            {
                int dietPlanLevel = perkLevels.PerkToLevels?["RR_Diet_Plan_Title"] ?? 0;
                return Mathf.Clamp((1 + statBonuses.weightLossMultiplier) *
                        GlobalSettings.weightLossMultiplier.threshold *
                        (parent.AsPawn().gender == Gender.Female ?
                            GlobalSettings.weightLossMultiplierFemale.threshold :
                            GlobalSettings.weightLossMultiplierMale.threshold) +
                        (statBonuses.weightLossMultBonus +
                        0.2f * dietPlanLevel),
                    0, float.MaxValue);
            }
        }




        public float RemainingFullnessUntil(float limit)
        {
            return limit - CurrentFullness;
        }


        //Current actual fullness. Displayed as the bar
        private float currentFullness;

        public float CurrentFullness
        {
            get => currentFullness;
            set => currentFullness = value;
        }


        //-------------------
        public float debugSoftLimitDelta;
        //In liters. represents threshold of stoach capacity. Initial value at pawn spawn.
        private float softLimitPersonal = defaultSoftLimit + Values.RandomFloat(softLimitVariation.x, softLimitVariation.y);
        public float SoftLimit
        {
            get
            {
                float blackHolePerkBonus = 50f * (perkLevels.PerkToLevels?["RR_BlackHole_Title"] ?? 0);
                float oneMoreBitePerkBonus = 1f * (perkLevels.PerkToLevels?["RR_OneMoreBite_Title"] ?? 0);
                float debugIncrease = debugSoftLimitDelta;

                return
                    Mathf.Clamp(softLimitPersonal *
                    (GlobalSettings.softLimitMuliplier.threshold *
                    (1 + statBonuses.softLimitMultiplier)) +
                    statBonuses.softLimitFlatBonus + blackHolePerkBonus + oneMoreBitePerkBonus + debugIncrease, 0, float.MaxValue);
            }
        }

        public bool SetAboveHardLimit => fullnessbar.peaceForeverHeld;

        //How much more than the soft limit the Hard Limit is.
        private const float hardLimitAdditionalPercentage = 0.3f;

        public float HardLimit
        {
            get
            {
                float limitBreakMultBonus = (perkLevels.PerkToLevels?["RR_LimitBreak_Title"] ?? 0) * 0.1f;

                return SoftLimit *
                    (1f +
                    Mathf.Clamp(
                        hardLimitAdditionalPercentage *
                        ((1 + limitBreakMultBonus) *
                        (1 + statBonuses.hardLimitAdditionalPercentageMultiplier) *
                        GlobalSettings.hardLimitMuliplier.threshold +
                        statBonuses.hardLimitAdditionalPercentageMultBonus), 0.3f, 10)
                    );
            }
        }



        bool ShouldShowWeightGizmo()
        {
            List<object> selectedPawns = Find.Selector.SelectedObjects.Where(o => !(o.AsPawn() is null)).ToList();

            return selectedPawns.Count <= 1;
        }

        public Pair<float, float> GetRanges()
        {
            if (nutritionbar is null || fullnessbar is null)
                return new Pair<float, float>(-1, -1);

            switch (DietMode)
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
            switch (DietMode)
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
            switch (DietMode)
            {
                case DietMode.Nutrition:
                    nutritionbar?.SetRanges(first, second);
                    return;
                case DietMode.Hybrid:
                    nutritionbar?.SetRanges(first, 0);
                    fullnessbar?.SetRanges(second, 0);
                    return;
                case DietMode.Fullness:
                    fullnessbar?.SetRanges(first, second);
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
            float maxFullness = HardLimit;

            switch (DietMode)
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

        public void UpdateDietBars()
        {
            nutritionbar?.UpdateBar(DietMode);
            fullnessbar?.UpdateBar(DietMode);
        }

        public float FullnessGainedMultiplier =>
            Mathf.Clamp(
                GlobalSettings.fullnessMultiplier.threshold + statBonuses.fullnessGainedMultBonus, 0, 10);

        public float ConsumedNutrition
        {
            get => consumedNutrition;
            set
            {
                consumedNutrition = value;
                if (!(consumedNutrition >=
                      GlobalSettings.nutritionPerPerkLevel.threshold * perkLevels.currentLevel)) return;
                int currentLevel = Mathf.FloorToInt(consumedNutrition / GlobalSettings.nutritionPerPerkLevel.threshold) + 1;
                int levelsGained = currentLevel - perkLevels.currentLevel;

                if (levelsGained < 0)
                    Log.Error("Error: Levels gained was negative.");

                perkLevels.currentLevel = currentLevel;
                perkLevels.availablePoints += levelsGained * GlobalSettings.levelsGainedPerLevel.threshold;
            }
        }


        public float SloshDurationSeconds { get; set; }
        public float SloshStartTick { get; set; }


        private float consumedNutrition;
        private int _perkLevelsToSpendForSaving;
        private int _currentLevelForSaving;

        public RimRoundStatBonuses statBonuses;

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

        public const float defaultFullnessToNutritionRatio = 1f; //i.e. 0.5 Fullness for 1 nutrition is 0.5f

        private float cumulativeSeverityGained;
        public float CumulativeSeverityKilosGained { get => cumulativeSeverityGained; set => cumulativeSeverityGained = value; }

        public const float severityUntilImmunity = 400;
        public const float immunitySeverityDecay = 0.5f;

        private Sustainer sloshSound;
        private Sustainer breathSound;
    }

    public class PerkLevels
    {
        public int currentLevel = 1;
        public int availablePoints;
        public Dictionary<string, int> PerkToLevels;
    }

    public struct WeightGainRequest
    {
        /// <param name="amountToGain">Amount of weight to gain in kilograms.</param>
        /// <param name="tickToApplyOn">Tick after which the request should be executed.</param>
        /// <param name="duration">Number of ticks for the weight to stay applied. If set to 0, weight gained is permanent.</param>
        /// <param name="triggerMessages">Whether weight gained from this should trigger the notifications at the top of the screen.</param>
        public WeightGainRequest(float amountToGain, int tickToApplyOn, int duration = 0, bool triggerMessages = false, bool useMultipliers = true)
        {
            this.amountToGain = amountToGain;
            this.tickToApplyOn = tickToApplyOn;
            this.duration = duration;
            this.triggerMessages = triggerMessages;
            this.useMultipliers = useMultipliers;
        }
        public float amountToGain;
        public int tickToApplyOn;
        public int duration;
        public bool triggerMessages;
        public bool useMultipliers;
    }


    public enum DietMode
    {
        Nutrition,
        Hybrid,
        Fullness,
        Disabled
    }
}
