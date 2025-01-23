using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.HediffComps
{
    public class HediffCompProperties_RimRoundStats : HediffCompProperties
    {
        public HediffCompProperties_RimRoundStats()
        {
            this.compClass = typeof(HediffComp_RimRoundStats);
        }

        // Final value clamped to [0, inf)
        public float weightGainMultiplier;
        public float weightLossMultiplier;
        public float digestionRateMultiplier;
        public float softLimitMultiplier;
        public float stomachElasticityMultiplier;
        // Final value clamped [0.3, 10]
        public float hardLimitAdditionalPercentageMultiplier;


        // Final value clamped to [0, inf)
        public float weightGainMultBonus;
        public float weightLossMultBonus;

        //public float digestionRateMultBonus;
        //public float softLimitMultBonus;

        public float digestionRateFlatBonus;
        public float softLimitFlatBonus;


        //public float stomachElasticityMultBonus;
        public float stomachElasticityFlatBonus;
        // Final value clamped [0.3, 2]
        public float hardLimitAdditionalPercentageMultBonus;

        //Should be in range [0, inf)
        public float fullnessGainedMultBonus;

        public float movementFlatBonus;
        public float manipulationFlatBonus;
        public float eatingFlatBonus;

        //All below should be in range [0, 1]. 0 means full mitigation, 1 means none.
        public float movementPenaltyMitigationMultBonus_Weight;
        public float movementPenaltyMitigationMultBonus_Fullness;
        public float eatingSpeedReductionMitigationMultBonus_Fullness;
        public float manipulationPenaltyMitigationMultBonus_Weight;

        public float painMitigationMultBonus_Fullness;
    }
}
