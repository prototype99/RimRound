using RimRound.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.HediffComps
{
    public class HediffComp_RimRoundStats : HediffComp
    {
        public HediffCompProperties_RimRoundStats Props
        {
            get
            {
                return (HediffCompProperties_RimRoundStats)this.props;
            }
        }

        //TODO: make it display the funky rimround stats
        // Rimround doesn't have this, make your own in this comp

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            this.ApplyStatChange(1f);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            this.ApplyStatChange(-1f);
        }

        protected void ApplyStatChange(float factor)
        {
            RimRoundStatBonuses statBonuses = new RimRoundStatBonuses();

            statBonuses.weightGainMultiplier = this.Props.weightGainMultiplier * factor;
            statBonuses.weightLossMultiplier = this.Props.weightLossMultiplier * factor;
            statBonuses.digestionRateMultiplier = this.Props.digestionRateMultiplier * factor;
            statBonuses.softLimitMultiplier = this.Props.softLimitMultiplier * factor;
            statBonuses.stomachElasticityMultiplier = this.Props.stomachElasticityMultiplier * factor;

            statBonuses.hardLimitAdditionalPercentageMultiplier = this.Props.hardLimitAdditionalPercentageMultiplier * factor;


            statBonuses.weightGainMultBonus = this.Props.weightGainMultBonus * factor;
            statBonuses.weightLossMultBonus = this.Props.weightLossMultBonus * factor;

            statBonuses.digestionRateFlatBonus = this.Props.digestionRateFlatBonus * factor;
            statBonuses.softLimitFlatBonus = this.Props.softLimitFlatBonus * factor;


            statBonuses.stomachElasticityFlatBonus = this.Props.stomachElasticityFlatBonus * factor;

            statBonuses.hardLimitAdditionalPercentageMultBonus = this.Props.hardLimitAdditionalPercentageMultBonus * factor;

            statBonuses.fullnessGainedMultBonus = this.Props.fullnessGainedMultBonus * factor;

            statBonuses.movementFlatBonus = this.Props.movementFlatBonus * factor;
            statBonuses.manipulationFlatBonus = this.Props.manipulationFlatBonus * factor;
            statBonuses.eatingFlatBonus = this.Props.eatingFlatBonus * factor;


            statBonuses.movementPenaltyMitigationMultBonus_Weight = this.Props.movementPenaltyMitigationMultBonus_Weight * factor;
            statBonuses.movementPenaltyMitigationMultBonus_Fullness = this.Props.movementPenaltyMitigationMultBonus_Fullness * factor;
            statBonuses.eatingSpeedReductionMitigationMultBonus_Fullness = this.Props.eatingSpeedReductionMitigationMultBonus_Fullness * factor;
            statBonuses.manipulationPenaltyMitigationMultBonus_Weight = this.Props.manipulationPenaltyMitigationMultBonus_Weight * factor;

            statBonuses.painMitigationMultBonus_Fullness = this.Props.painMitigationMultBonus_Fullness * factor;

            StatChangeUtility.ChangeRimRoundStats(this.Pawn, statBonuses);
        }
    }
}
