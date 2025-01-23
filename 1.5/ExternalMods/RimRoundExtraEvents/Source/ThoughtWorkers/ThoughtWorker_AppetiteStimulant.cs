using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimRound.Comps;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimRoundExtraEvents.ThoughtWorkers
{
    public class ThoughtWorker_AppetiteStimulant : ThoughtWorker
    {
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(this.def.hediff, false);
			if (firstHediffOfDef == null || firstHediffOfDef.def.stages == null)
			{
				return ThoughtState.Inactive;
			}

			var comp = p.TryGetComp<FullnessAndDietStats_ThingComp>();

			if (comp is null)
			{
				return ThoughtState.Inactive;
			}

			float fullnessRatio = comp.CurrentFullness / comp.SoftLimit;

			switch (firstHediffOfDef.CurStageIndex)
            {
				case 3:
					if (comp.CurrentFullness > comp.SoftLimit) return ThoughtState.ActiveAtStage(7);
					if (fullnessRatio >= 0.9) return ThoughtState.ActiveAtStage(6);
					if (fullnessRatio >= 0.8) return ThoughtState.ActiveAtStage(5);

					if (fullnessRatio < 0.6) return ThoughtState.ActiveAtStage(0);
					if (fullnessRatio < 0.7) return ThoughtState.ActiveAtStage(1);
					if (fullnessRatio < 0.8) return ThoughtState.ActiveAtStage(2);
					break;
				case 2:
					if (fullnessRatio >= 0.9) return ThoughtState.ActiveAtStage(6);
					if (fullnessRatio >= 0.8) return ThoughtState.ActiveAtStage(5);
					if (fullnessRatio >= 0.7) return ThoughtState.ActiveAtStage(4);

					if (fullnessRatio < 0.5) return ThoughtState.ActiveAtStage(1);
					if (fullnessRatio < 0.6) return ThoughtState.ActiveAtStage(2);
					if (fullnessRatio < 0.7) return ThoughtState.ActiveAtStage(3);
					break;
				case 1:
					if (fullnessRatio >= 0.75) return ThoughtState.ActiveAtStage(5);
					if (fullnessRatio >= 0.5) return ThoughtState.ActiveAtStage(4);

					if (fullnessRatio < 0.25) return ThoughtState.ActiveAtStage(2);
					if (fullnessRatio < 0.5) return ThoughtState.ActiveAtStage(3);
					break;
				default:
					if (fullnessRatio >= 0.2) return ThoughtState.ActiveAtStage(4);
					if (fullnessRatio < 0.2) return ThoughtState.ActiveAtStage(3);
					break;
			}
			// shouldn't reach this
			return ThoughtState.Inactive;
		}
	}
}
