using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimRoundExtraEvents.ThingComps.ArchoFatStatue
{
    public class Dialog_ArchoFatStatue : Dialog_NodeTree
    {
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(720f, 600f);
			}
		}

		// Token: 0x06008C54 RID: 35924 RVA: 0x0030BB5D File Offset: 0x00309D5D
		public Dialog_ArchoFatStatue(Pawn negotiator, DiaNode startNode, bool delayInteractivity) : base(startNode, delayInteractivity, false, null)
		{
			this.negotiator = negotiator;
		}

		// Token: 0x06008C55 RID: 35925 RVA: 0x0030BB78 File Offset: 0x00309D78
		public override void DoWindowContents(Rect inRect)
		{
			Widgets.BeginGroup(inRect);
			Rect rect = new Rect(0f, 0f, inRect.width / 2f, 70f);
			Rect rect2 = new Rect(0f, rect.yMax, rect.width, 60f);
			Rect rect3 = new Rect(inRect.width / 2f, 0f, inRect.width / 2f, 70f);
			Rect rect4 = new Rect(inRect.width / 2f, rect.yMax, rect.width, 60f);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, this.negotiator.LabelCap);
			Text.Anchor = TextAnchor.UpperRight;
			//this is normally name
			Widgets.Label(rect3, "???");
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			GUI.color = new Color(1f, 1f, 1f, 0.7f);
			//replace with value, pawn weight(inc tier name), and final value?
			//after weight put a (-??%) penalty, final value is value adapted to that
			Widgets.Label(rect2, "SocialSkillIs".Translate(this.negotiator.skills.GetSkill(RimWorld.SkillDefOf.Social).Level));
			Text.Anchor = TextAnchor.UpperRight;
			//this is normally stuff like "Civil Outlander Union" and "Goodwill: ???" in the menu
			Widgets.Label(rect4, "???\n???");
			//Faction faction = this.commTarget.GetFaction();
			//if (faction != null)
			//{
			//	FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
			//	GUI.color = playerRelationKind.GetColor();
			//	Widgets.Label(new Rect(rect4.x, rect4.y + Text.CalcHeight(this.commTarget.GetInfoText(), rect4.width) + Text.SpaceBetweenLines, rect4.width, 30f), playerRelationKind.GetLabelCap());
			//}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			Widgets.EndGroup();
			float num = 147f;
			Rect rect5 = new Rect(0f, num, inRect.width, inRect.height - num);
			base.DrawNode(rect5);
		}

		protected Pawn negotiator;

		private const float TitleHeight = 70f;

		private const float InfoHeight = 60f;
	}
}
