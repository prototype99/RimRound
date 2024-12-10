using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRound.Comps.Buildings
{
    public class Comp_RestingWeightGainMod : ThingComp
    {
        public CompProperties_RestingWeightGainMod Props
        {
            get
            {
                return (CompProperties_RestingWeightGainMod)this.props;
            }
        }

        public float GetWeightGainAdditiveModifier() {
            QualityCategory qc;
            if (!QualityUtility.TryGetQuality(parent, out qc)) {
                return 0;
            }

            float qualityMultiplier = 1;
            switch (qc) {
                case QualityCategory.Awful:
                    qualityMultiplier = 0.7f;
                    break;
                case QualityCategory.Poor:
                    qualityMultiplier = 0.9f;
                    break;
                case QualityCategory.Normal:
                    qualityMultiplier = 1f;
                    break;
                case QualityCategory.Good:
                    qualityMultiplier = 1.1f;
                    break;
                case QualityCategory.Excellent:
                    qualityMultiplier = 1.3f;
                    break;
                case QualityCategory.Masterwork:
                    qualityMultiplier = 1.5f;
                    break;
                case QualityCategory.Legendary:
                    qualityMultiplier = 2.0f;
                    break;
                default:
                    qualityMultiplier = 1;
                    Log.Error("Ran default case in RestingWeightGainMod comp");
                    break;
            }
            return Props.additiveRestingWeightGainModifier * qualityMultiplier;
        }

        public override string CompInspectStringExtra()
        {
            return $"Weight Gain Modifier +{GetWeightGainAdditiveModifier()}";
        }
    }
}
