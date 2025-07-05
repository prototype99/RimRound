using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.ThingComps
{
    public class CompAddHediffRadius : ThingComp
    {
		public CompProperties_AddHediffRadius Props
		{
			get
			{
				return (CompProperties_AddHediffRadius)this.props;
			}
		}

		//TODO: check line of sight
		//TODO: make craftable like a sculpture
		//TODO: make range be affected by quality?
		//TODO: work out something for making the effect 

		public override void CompTickRare()
		{
			//Map map = this.parent.Map;
			//List<Faction> mapFactions = map.mapPawns.FactionsOnMap();

			//foreach (Faction faction in mapFactions)
			//{
			//	List<Pawn> pawns = map.mapPawns.FreeHumanlikesSpawnedOfFaction(faction);
			//	foreach (Pawn pawn in pawns)
			//             {
			//		this.applyHediffIfWithinRange(pawn);
			//             }
			//}

			Map map = this.parent.Map;
			List<Pawn> pawns = (List<Pawn>)map.mapPawns.AllPawnsSpawned;

			foreach (Pawn pawn in pawns)
			{
				if (pawn is null || !pawn.RaceProps.Humanlike) continue;

				this.applyHediffIfWithinRange(pawn);
			}
		}

		protected void applyHediffIfWithinRange(Pawn pawn)
		{
			if (this.parent.Position.DistanceTo(pawn.Position) > this.Props.effectRadius)
				return;

			Hediff hediff = RimRound.Utilities.HediffUtility.GetHediffOfDefFrom(this.Props.hediffDeff, pawn);

			if (hediff is null)
			{
				hediff = RimRound.Utilities.HediffUtility.AddHediffOfDefTo(this.Props.hediffDeff, pawn);
			}

			if (hediff.Severity < this.Props.setSeverity)
			{
				hediff.Severity = this.Props.setSeverity;
			}

			if (hediff.Severity < this.Props.maxSeverity)
            {
				float severityPerRareTick = this.Props.severityPerHour / 10;
				float severityToMax = this.Props.maxSeverity - hediff.Severity;
				hediff.Severity += Math.Min(severityPerRareTick, severityToMax);
				return;
            }
		}
	}
}