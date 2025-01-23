using RimRound.Comps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimRound.Utilities;
using Verse.AI;

namespace RimRoundExtraEvents.Hediffs
{
    public class Hediff_AppetiteStimulant : Hediff_High
    {

        public override float Severity
        {
            get => base.Severity;
            set
            {
                int curStageIndex = this.CurStageIndex;
                base.Severity = value;
                if ((this.CurStageIndex != curStageIndex || this.currentDigestionBonus == 0) && this.pawn.health.hediffSet.hediffs.Contains(this))
                {
                    float newDigestionBonus = Hediff_AppetiteStimulant.stageDigestionRateBonus[this.CurStageIndex];

                    if (this.currentDigestionBonus != newDigestionBonus)
                    {
                        RimRoundStatBonuses statBonuses = new RimRoundStatBonuses();

                        statBonuses.digestionRateMultiplier = newDigestionBonus - this.currentDigestionBonus;

                        StatChangeUtility.ChangeRimRoundStats(this.pawn, statBonuses);

                        this.currentDigestionBonus = newDigestionBonus;
                    }

                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (this.pawn.IsHashIntervalTick(30))
            {
                switch (this.CurStageIndex)
                {
                    case 2:
                        var comp = this.pawn.TryGetComp<FullnessAndDietStats_ThingComp>();

                        if (comp is null)
                        {
                            break;
                        }

                        float fullnessRatio = comp.CurrentFullness / comp.SoftLimit;

                        if (fullnessRatio < 0.1)
                        {
                            this.pawn.mindState.mentalStateHandler.TryStartMentalState(
                                MentalStates.MentalStateDefOf.RREE_Stimulated_Binging_Food,
                                null,
                                false,
                                false,
                                false,
                                null,
                                false,
                                false,
                                false);
                        }
                        break;
                    case 3:
                        this.pawn.mindState.mentalStateHandler.TryStartMentalState(
                            MentalStates.MentalStateDefOf.RREE_Stimulated_Binging_Food,
                            null,
                            false,
                            false,
                            false,
                            null,
                            false,
                            false,
                            false);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();

            RimRoundStatBonuses statBonuses = new RimRoundStatBonuses();

            statBonuses.digestionRateMultiplier = this.currentDigestionBonus * -1;

            StatChangeUtility.ChangeRimRoundStats(this.pawn, statBonuses);
        }

        public override string Description
        {
            get
            {
                if (this.CurStage is null)
                {
                    return this.def.Description;
                }
                
                switch (this.CurStageIndex)
                {
                    case 0:
                        return "A state of elevated hunger induced by appetite stimulants.";
                    case 1:
                        return "A state of elevated hunger induced by appetite stimulants.";
                    case 2:
                        return "A state of elevated hunger induced by appetite stimulants.";
                    case 3:
                        return "A state of unnaturally elevated hunger induced by repeated use of appetite stimulants. The victim can no";
                    default:
                        return this.def.Description;
                }
            }
        }

        public override string TipStringExtra
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (StatDrawEntry statDrawEntry in HediffStatsUtility.SpecialDisplayStats(this.CurStage, this))
                {
                    if (statDrawEntry.ShouldDisplay())
                    {
                        stringBuilder.AppendLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                    }
                }

                string label = "Digestion rate";
                string value = this.currentDigestionBonus.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset);

                stringBuilder.Append("  - " + label + ": " + value);
                return stringBuilder.ToString();
            }
        }

        public float currentDigestionBonus = 0.0f;

        static List<float> stageDigestionRateBonus = new List<float>()
        {
            0.1f,
            0.25f,
            0.5f,
            1.0f,
        };

    }
}
