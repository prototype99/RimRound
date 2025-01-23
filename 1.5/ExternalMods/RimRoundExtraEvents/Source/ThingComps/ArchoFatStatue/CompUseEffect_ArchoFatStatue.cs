using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.ThingComps.ArchoFatStatue
{
    public class CompUseEffect_ArchoFatStatue : CompUseEffect
    {

		public CompProperties_Useable_ArchoFatStatue Props
		{
			get
			{
				return (CompProperties_Useable_ArchoFatStatue)this.props;
			}
			//TODO: work out what kinda gifts to give
			//TODO: maybe make it not work on psychically deaf pawns,
			// the hediff giving some level of psychic sensitivity
		}

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);

			//TODO: open archoFatStatue dialog

			//TODO: make this use the dialog maker
			//Find.WindowStack.Add(new Dialog_ArchoFatStatue(usedBy, this, false));
			//LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
			//PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(this.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradeShip".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, false, true);
			//TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon, Array.Empty<string>());
		}

		public override AcceptanceReport CanBeUsedBy(Pawn p)
		{
			//TODO: maybe give the warning in the dialog

			if (this.cooldownTicks > 0)
			{
				//TODO: replace error text, make it say there's a cooldown or something
				return new AcceptanceReport("AlreadyUsed".Translate());
			}
			//TODO: make it not work if the pawn has the curse hediff
			//if (!MechanitorUtility.IsMechanitor(p))
			//{
			//	failReason = "RequiresMechanitor".Translate();
			//	return false;
			//}
			//TODO: make it not work if the pawn is too fat
			//if (!MechanitorUtility.IsMechanitor(p))
			//{
			//	failReason = "RequiresMechanitor".Translate();
			//	return false;
			//}
			return AcceptanceReport.WasAccepted;
		}

		protected int cooldownTicks = 0;
	}
}
