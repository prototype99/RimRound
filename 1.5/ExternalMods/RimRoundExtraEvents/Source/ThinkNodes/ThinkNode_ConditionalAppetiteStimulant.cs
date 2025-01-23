using RimRound.Comps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.ThinkNodes
{
    public class ThinkNode_ConditionalAppetiteStimulant : ThinkNode_ConditionalHasHediff
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (!base.Satisfied(pawn)) { return false; }


            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(this.hediff, false);

            if (firstHediffOfDef is null) { return false; }


            var comp = pawn.TryGetComp<FullnessAndDietStats_ThingComp>();

            if (comp is null) { return false; }


            float fullnessRatio = comp.CurrentFullness / comp.SoftLimit;

            switch (firstHediffOfDef.CurStageIndex)
            {
                case 3:
                    //return true;
                    return fullnessRatio < 1.0;
                case 2:
                    return fullnessRatio < 0.5;
                default: return false;
            }
        }
    }
}
