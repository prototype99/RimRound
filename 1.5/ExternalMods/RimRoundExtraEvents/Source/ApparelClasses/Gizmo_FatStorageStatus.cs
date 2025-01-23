using RimRound.Comps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimRoundExtraEvents.ApparelClasses
{
    [StaticConstructorOnStartup]
    public class Gizmo_FatStorageStatus : Gizmo
    {
        public Gizmo_FatStorageStatus()
        {
            this.Order = -68f;
        }

		public override float GetWidth(float maxWidth)
		{
			return 140f
				+ 48f; //for buttons rectangle
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			//Large rectangle that contains the whole gizmo
			Rect baseRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
			Widgets.DrawWindowBackground(baseRect);

			//The highlighted rectangle if you hover over the main gizmo.
			Rect innerRect = baseRect.ContractedBy(6f);

			//The rectangle that contains the label
			Rect labelRect = innerRect;
			labelRect.width = innerRect.width - 32f;
			labelRect.height = baseRect.height / 2f;

			Text.Font = GameFont.Tiny;
			Widgets.Label(labelRect, this.comp.parent.LabelCap);

			//Rendering the actual fat storage bar
			Gizmo_FatStorageBar barRenderer = new Gizmo_FatStorageBar(this.comp, this);
			barRenderer.DrawOnGUI(innerRect, innerRect.yMax - Gizmo_FatStorageBar.BarHeight - 2f);


			Rect buttonContainer = new Rect
			{
				x = innerRect.x + innerRect.width - 48f,
				y = innerRect.y,
				width = 48f,
				height = 32f
			};

			//TODO: make the buttons shift to the middle automatically
			//Calculate the amount of buttons, then increment a value with each button

			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Tiny;

			if (this.comp.Props.hasFunnelModeButton)
            {
				this.AddFunnelModeButton(buttonContainer);
			}

			if (this.comp.Props.hasRemoveButton)
			{
				this.AddRemoveButton(buttonContainer);
			}

			if (this.comp.Props.hasAddButton)
			{
				this.AddAddButton(buttonContainer);
			}

			this.AddTextField(buttonContainer);

			return new GizmoResult(GizmoState.Clear);
		}

		private void AddFunnelModeButton(Rect buttonContainer)
		{
			Rect funnelModeButtonContainer = new Rect
			{
				x = buttonContainer.x + buttonContainer.width - 15f,
				y = buttonContainer.y + buttonContainer.height - 15f,
				width = 15f,
				height = 15f
			};

			if (this.comp.FunnelModeButtonDisabled())
			{
				Texture2D inactiveButtonSymbol = this.comp.FunnelModeActive ? funnelModeOnButtonInactiveIcon : funnelModeOffButtonInactiveIcon;
				Widgets.ButtonImageFitted(funnelModeButtonContainer, inactiveButtonSymbol);
				return;
			}

			if (Mouse.IsOver(funnelModeButtonContainer))
			{
				Widgets.DrawHighlight(funnelModeButtonContainer);
			}

			Texture2D buttonSymbol = this.comp.FunnelModeActive ? funnelModeOnButtonIcon : funnelModeOffButtonIcon;
			if (Widgets.ButtonImageFitted(funnelModeButtonContainer, buttonSymbol))
			{
				this.comp.FunnelModeButtonPressed();
			}

			Text.Font = GameFont.Small;
			TooltipHandler.TipRegion(funnelModeButtonContainer, "RREE_ToolTip_FatStorageGizmo_FunnelModeButton".Translate());
			Text.Font = GameFont.Tiny;
		}

		private void AddRemoveButton(Rect buttonContainer)
		{
			Rect removeButtonContainer = new Rect
			{
				x = buttonContainer.x + buttonContainer.width - 2 * 15f - 2f,
				y = buttonContainer.y + buttonContainer.height - 15f,
				width = 15f,
				height = 15f
			};

			if (this.comp.RemoveButtonDisabled())
			{
				Widgets.ButtonImageFitted(removeButtonContainer, removeButtonInactiveIcon);
				return;
			}

			if (Mouse.IsOver(removeButtonContainer))
			{
				Widgets.DrawHighlight(removeButtonContainer);
			}

			if (Widgets.ButtonImageFitted(removeButtonContainer, removeButtonIcon))
			{
				this.comp.RemoveButtonPressed(-1 * this.comp.gizmoVal);
			}
		}

		private void AddAddButton(Rect buttonContainer)
		{
			Rect addButtonContainer = new Rect
			{
				x = buttonContainer.x + buttonContainer.width - 3 * 15f - 2 * 2f,
				y = buttonContainer.y + buttonContainer.height - 15f,
				width = 15f,
				height = 15f
			};

			if (this.comp.AddButtonDisabled())
			{
				Widgets.ButtonImageFitted(addButtonContainer, addButtonInactiveIcon);
				return;
			}

			if (Mouse.IsOver(addButtonContainer))
			{
				Widgets.DrawHighlight(addButtonContainer);
			}

			if (Widgets.ButtonImageFitted(addButtonContainer, addButtonIcon))
			{
				this.comp.AddButtonPressed(this.comp.gizmoVal);
			}
		}

		private void AddTextField(Rect buttonContainer)
        {
			Text.Font = GameFont.Tiny;

			Rect textFieldRect = new Rect
			{
				x = buttonContainer.x,
				y = buttonContainer.y,
				width = 48f,
				height = 15f
			};
			Widgets.TextFieldNumeric(textFieldRect, ref this.comp.gizmoVal, ref this.comp.gizmoBuffer);
		}

		public virtual bool ShouldHideContents()
		{
			return this.comp.ContentsCurrentlyHidden;
		}

		public virtual bool ShouldHideCapacity()
		{
			return this.comp.CapacityCurrentlyHidden; 
		}

		public virtual bool ShouldHideBar()
		{
			return this.comp.ContentsCurrentlyHidden || this.comp.CapacityCurrentlyHidden;
		}

		public virtual bool ShouldBarBeRed()
		{
			return this.comp.FatStored > this.comp.FatStoredSoftLimit;
		}

		public CompFatStorage comp;

		private static readonly Texture2D removeButtonIcon = ContentFinder<Texture2D>.Get("UI/FatStorageGizmo/removeSymbol", true);
		private static readonly Texture2D addButtonIcon = ContentFinder<Texture2D>.Get("UI/FatStorageGizmo/addSymbol", true);
		private static readonly Texture2D funnelModeOnButtonIcon = ContentFinder<Texture2D>.Get("UI/FatStorageGizmo/funnelModeOnSymbol", true);
		private static readonly Texture2D funnelModeOffButtonIcon = ContentFinder<Texture2D>.Get("UI/FatStorageGizmo/funnelModeOffSymbol", true);

		private static readonly Texture2D removeButtonInactiveIcon = ContentFinder<Texture2D>.Get("UI/FatStorageGizmo/removeInactiveSymbol", true);
		private static readonly Texture2D addButtonInactiveIcon = ContentFinder<Texture2D>.Get("UI/FatStorageGizmo/addInactiveSymbol", true);
		private static readonly Texture2D funnelModeOnButtonInactiveIcon = ContentFinder<Texture2D>.Get("UI/FatStorageGizmo/funnelModeOnInactiveSymbol", true);
		private static readonly Texture2D funnelModeOffButtonInactiveIcon = ContentFinder<Texture2D>.Get("UI/FatStorageGizmo/funnelModeOffInactiveSymbol", true);
	}
}
