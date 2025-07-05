using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimRoundExtraEvents.JobGivers
{
    public class JobGiver_WanderStimulatedBinge : JobGiver_Wander
    {
		public JobGiver_WanderStimulatedBinge() : base()
		{
			this.wanderRadius = 3f;
			this.ticksBetweenWandersRange = new IntRange(125, 200);
			this.wanderDestValidator = ((Pawn pawn, IntVec3 loc, IntVec3 root) => true);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.Position;
		}
	}
}
