﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;
using RimRound.Comps;
using RimRound.Utilities;

namespace RimRound.UI
{
    [StaticConstructorOnStartup]
    public class WeightGizmo_FullnessBar
    {
        public WeightGizmo_FullnessBar(Pawn pawn) 
        {
			fullnessAndDietStats = pawn.TryGetComp<FullnessAndDietStats_ThingComp>();
		}


		//call in the main gizmo's DrawOnGUI()
        public void DrawOnGUI(Rect rect, float yPos, FullnessAndDietStats_ThingComp comp) 
        {
			/*
			float yMin = rect.yMin;


            //The text in the middle top that acts as a title of the gizmo
            Text.Anchor = TextAnchor.UpperCenter;
            Text.Font = GameFont.Small;
            Widgets.Label(rect.x, ref yMin, rect.width, "DietaryTarget".Translate(), default(TipSignal));

            //This puts the Desired: X% text above the bar below the title
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(rect.x, ref yMin, rect.width, "DietaryTargetDesired".Translate() + ": " + this.SliderCurrentValue.ToStringPercent(), default(TipSignal));

			*/
			
            //Causes rect2 to highlight when hovering over the gizmo and the tooltip to pop up
			/*
            Text.Font = GameFont.Small;
            if (Mouse.IsOver(rect2) && !this.draggingBar)
            {
                Widgets.DrawHighlight(rect2);
                //change this eventually!
                TooltipHandler.TipRegion(rect2, () => this.GetTip(), 9493937);
            }
			*/

            this.DrawMeter(rect, yPos, comp);
        }


		public void UpdateBar(DietMode dietMode) 
		{
			if (dietMode == DietMode.Disabled)
				enabled = false;
			else
				enabled = true;


			UpdateSliders(dietMode);
		}


		void UpdateSliders(DietMode dietMode)
		{
			if (thresholdSlider?.BarPercentage is float percent1)
				thresholdSliderLastValue = percent1;

			if (maxSlider?.BarPercentage is float percent2)
				maxSliderLastValue = percent2;

			thresholdSlider = null;
			maxSlider = null;

			switch (dietMode)
			{
				case DietMode.Nutrition:
					return;
				case DietMode.Hybrid:
					maxSlider = new Slider(maxSliderLastValue, true, DisplayLimit);
					return;
				case DietMode.Fullness:
					thresholdSlider = new Slider(thresholdSliderLastValue, false, DisplayLimit);
					maxSlider = new Slider(maxSliderLastValue, false, DisplayLimit);
					return;
				case DietMode.Disabled:
					return;
				default:
					return;
			}
		}

		void CheckForDeathDialog() 
		{
			if ((maxSlider?.draggingBar ?? false) || (thresholdSlider?.draggingBar ?? false))
				if (GetRanges().First < fullnessAndDietStats.HardLimit && GetRanges().Second < fullnessAndDietStats.HardLimit)
					peaceForeverHeld = false;

			if (!deathDialogOpen && !peaceForeverHeld)
			{
				switch (fullnessAndDietStats.DietMode)
				{
					case DietMode.Disabled:
						break;
					case DietMode.Nutrition:
						break;
					case DietMode.Hybrid:
						if (maxSlider == null)
						{
							Log.Error("MaxSlider was null in DrawMeter()");
							break;
						}
						if (maxSlider.barValue >= fullnessAndDietStats.HardLimit)
						{
							WeightGizmo_DeathWarning.DisplayWarning((Pawn)fullnessAndDietStats.parent, new List<Slider>() { maxSlider });
							deathDialogOpen = true;
						}
						break;
					case DietMode.Fullness:
						if (maxSlider == null || thresholdSlider == null)
						{
							Log.Error("Slider(s) were null in DrawMeter()");
							break;
						}
						if (thresholdSlider.barValue >= fullnessAndDietStats.HardLimit || maxSlider.barValue >= fullnessAndDietStats.HardLimit)
						{
							WeightGizmo_DeathWarning.DisplayWarning((Pawn)fullnessAndDietStats.parent, new List<Slider>() { thresholdSlider, maxSlider });
							deathDialogOpen = true;
						}
						break;
					default:
						break;

				}
			}
		}

		//Used in Gizmo on GUI only. Draws sliders, dividers and dashes
		void DrawMeter(Rect inRect, float yPos, FullnessAndDietStats_ThingComp comp)
        {
            marginSize = 1f;

            yPosition = yPos;

            Rect fullnessBarRect = new Rect
            {
                width = inRect.width - 2 * marginSize,
                height = BarHeight,
                x = inRect.x + marginSize,
                y = yPosition
            };

            DrawFullnessIndicatorBar(fullnessBarRect);

            DrawTickMarks(fullnessBarRect);

            DrawSoftLimitAndHardLimitDividers(fullnessBarRect);

            DrawSlidersInRect(fullnessBarRect);

            CheckForDeathDialog();

            AddFullnessPercentageOnFullnessBar(fullnessBarRect, comp);
        }

        private void AddFullnessPercentageOnFullnessBar(Rect fullnessBarRect, FullnessAndDietStats_ThingComp comp)
        {
            Text.Font = GlobalSettings.largeDietGizmo ? GameFont.Small : GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
			if (GlobalSettings.largeDietGizmo)
				Widgets.Label(fullnessBarRect, CurrentFullnessAsPercentOfSoftLimit.ToStringPercent());
			else
				DrawFullnessLabel(fullnessBarRect, comp);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private void DrawSlidersInRect(Rect fullnessBarRect)
        {
            if (maxSlider != null)
            {
                maxSlider.DrawSlider(fullnessBarRect, SliderTex, DisplayLimit);
            }
            if (thresholdSlider != null)
            {
                thresholdSlider.DrawSlider(fullnessBarRect, SliderTex, DisplayLimit);
            }
        }

		private void DrawFullnessLabel(Rect rect, FullnessAndDietStats_ThingComp comp)
		{
            if (comp.DietMode != DietMode.Disabled)
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(
                    new Rect
                    {
                        x = rect.x,
                        y = rect.yMin + 3f,
                        width = rect.width,
                        height = rect.height
                    },
                    $"{(this.CurrentFullnessAsPercentOfSoftLimit * 100).ToString("F0")}% ({comp.CurrentFullness.ToString("F1")}/{comp.SoftLimit.ToString("F1")}L)");
				Text.Anchor = TextAnchor.UpperLeft;
			}
        }


        private void DrawFullnessIndicatorBar(Rect fullnessBarRect)
        {
            bool flag = Mouse.IsOver(fullnessBarRect);

            //Makes the Main bars (the main indicator plus the black background)
            if (enabled)
                Widgets.FillableBar(fullnessBarRect, CurrentFullnessAsPercentOfDisplayLimit, flag ? StrengthHighlightTex : StrengthTex, EmptyBarTex, true);
            else
                Widgets.FillableBar(fullnessBarRect, CurrentFullnessAsPercentOfDisplayLimit, DisabledTex, EmptyBarTex, true);
        }

        private void DrawTickMarks(Rect rect1)
        {

            //Draws the little ticks at the bottom
            float baseTickLiters = 0.2f;
			float stepMultiplier = 5f;
			int maxTicksOnBar = 20;
            float dashInterval = baseTickLiters;

            int numberOfDashes = (int)(DisplayLimit / dashInterval);
			while (numberOfDashes > maxTicksOnBar) {
				dashInterval *= stepMultiplier;
                numberOfDashes = (int)(DisplayLimit / dashInterval);
            }


            int currentDashNumber = 0;

            float dashPercentInterval = dashInterval / DisplayLimit;

            for (int i = numberOfDashes; i > 0; --i)
            {
                ++currentDashNumber;
                DrawDash(rect1, currentDashNumber * dashPercentInterval, CurrentFullnessAsPercentOfDisplayLimit);
            }
        }

        private void DrawSoftLimitAndHardLimitDividers(Rect rect1)
        {
            //Soft Limit Dash
            DrawDivider(
                rect1,
                SoftLimitAsPercentage,
                CurrentFullnessAsPercentOfDisplayLimit,
                DividerTexSoft,
                DividerTexSoft
                );

            //Hard Limit Dash
            DrawDivider(
                rect1,
                HardLimitAsPercentage,
                CurrentFullnessAsPercentOfDisplayLimit,
                DividerTexHard,
                DividerTexHard
                );
        }

        //Used only in DrawBar(). Draws the little rectangles (dashes) on the meter.
        void DrawDivider(Rect rect, float percent, float curValue, Texture2D underTex, Texture2D exceedTex)
		{
			Rect divider = new Rect
			{
				x = rect.x + 3f + (rect.width - 8f) * percent,
				y = rect.y,
				width = 2f,
				height = rect.yMax - rect.yMin
			};

			if (curValue < percent)
			{
				GUI.DrawTexture(divider, underTex);
				return;
			}
			GUI.DrawTexture(divider, exceedTex);
		}

		//Used only in DrawBar(). Draws the little rectangles (dashes) on the meter.
		void DrawDash(Rect rect, float placementPercent, float curValue)
		{
			Rect position = new Rect
			{
				x = rect.x + 3f + (rect.width - 8f) * placementPercent,
				y = rect.y + rect.height - 7f,
				width = 2f,
				height = 4f
			};
			if (curValue < placementPercent)
			{
				GUI.DrawTexture(position, BaseContent.GreyTex);
				return;
			}
			GUI.DrawTexture(position, BaseContent.BlackTex);
		}

		//Change this! Use only on Gizmo on GUI
		private string GetTip()
		{
			string text = "Billy";//"DesiredConnectionStrengthDesc".Translate(this.connection.parent.Named("TREE"), this.connection.ConnectedPawn.Named("CONNECTEDPAWN"), this.connection.ConnectionStrengthLossPerDay.ToStringPercent().Named("FALL")).Resolve();
			string text2 = "Help me";//this.connection.AffectingBuildingsDescription("CurrentlyAffectedBy");
			if (!text2.NullOrEmpty())
			{
				text = text + "\n\n" + text2;
			}
			return text;
		}
		//Used in Gizmo on GUI only
		public float GetWidth(float maxWidth)
		{
			return 212f;
		}


		public void ResetDietSettings() 
		{
			SetRanges(lowerDefaultValue, upperDefaultValue);
		}

		public void SetRanges(float first, float second)
		{
			//If either slider is disabled
			if (thresholdSlider == null || maxSlider == null)
			{
				//If the bar is disabled
				if (thresholdSlider == null && maxSlider == null)
				{
					return;
				}

				//If the max slider is null
				else if (maxSlider == null)
				{
					thresholdSlider.BarPercentage = first;
				}

				//If the max isn't null but the threshold is
				else if (thresholdSlider == null)
				{
					maxSlider.BarPercentage = first;
				}
				else
				{
					return;
				}
			}
			//If neither are disabled
			else
			{
				thresholdSlider.BarPercentage = first;
				maxSlider.BarPercentage = second;
			}
		}

		//returns a pair of floats for the actual values (not percents) of the sliders in least to greatest order. If there is one, it returns (value, -1) if neither exist (-1, -1)
		public Pair<float, float> GetRanges()
		{
			if (thresholdSlider == null || maxSlider == null)
			{
				if (thresholdSlider == null && maxSlider == null)
				{
					return new Pair<float, float>(-1, -1);
				}
				else if (maxSlider == null)
				{
					return new Pair<float, float>(thresholdSlider.barValue, -1);
				}
				else if (thresholdSlider == null)
				{
					return new Pair<float, float>(maxSlider.barValue, -1);
				}
				else 
				{
					return new Pair<float, float>(-1 ,-1);
				}
			}
			else 
			{
				return thresholdSlider.barValue <= maxSlider.barValue ?
					new Pair<float, float>(thresholdSlider.barValue, maxSlider.barValue) :
					new Pair<float, float>(maxSlider.barValue, thresholdSlider.barValue);
			}
		}


		//Textures
		private static readonly Texture2D StrengthTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Orange);
		private static readonly Texture2D StrengthHighlightTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.LightOrange);
		private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));
		//private static readonly Texture2D StrengthTargetTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.DarkOrange);
		private static readonly Texture2D SliderTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.DarkOrange);
		private static readonly Texture2D DisabledTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Grey);

		private static readonly Texture2D DividerTexHard = ContentFinder<Texture2D>.Get("UI/Misc/NeedUnitDivider", true);
		private static readonly Texture2D DividerTexSoft = ContentFinder<Texture2D>.Get("UI/Misc/NeedUnitDivider", true);

		public float yPosition
		{
			get
			{
				return yposition;
			}
			private set
			{
				yposition = value;
			}
		}
		private float yposition;


		public static float BarHeight 
		{
			get 
			{
				return GlobalSettings.largeDietGizmo ? 30f : 26f + Values.debugPos;
			}
		}

		

		float marginSize;


		public float CurrentFullnessAsPercentOfSoftLimit
		{
			get
			{
				return fullnessAndDietStats.CurrentFullness / fullnessAndDietStats.SoftLimit;
			}
			private set
			{
				fullnessAndDietStats.CurrentFullness = value * fullnessAndDietStats.SoftLimit;
			}

		}


		public float SoftLimitAsPercentage
		{
			get
			{
				return fullnessAndDietStats.SoftLimit / DisplayLimit;
			}
		}

		public float HardLimitAsPercentage
		{
			get
			{
				return fullnessAndDietStats.HardLimit / DisplayLimit;
			}
		}

		public float CurrentFullnessAsPercentOfDisplayLimit
		{
			get
			{
				return fullnessAndDietStats.CurrentFullness / DisplayLimit;
			}
			private set
			{
				fullnessAndDietStats.CurrentFullness = value * DisplayLimit;
			}

		}

		public float CurrentFullnessAsPercentOfHardLimit
		{
			get
			{
				return fullnessAndDietStats.CurrentFullness / fullnessAndDietStats.HardLimit;
			}
			private set
			{
				fullnessAndDietStats.CurrentFullness = value * fullnessAndDietStats.HardLimit;
			}
		}


		//How much of the bar is occupied by the death stuffing option
		private float deathDisplayPercentage = 0.05f;

        public float DisplayLimit
        {
            get
            {
				return fullnessAndDietStats.HardLimit / (1f - deathDisplayPercentage);
            }
        }


		private float lowerDefaultValue = 0.3f;
		private float upperDefaultValue = 0.8f;

		Slider thresholdSlider;
		Slider maxSlider;
		private float thresholdSliderLastValue;
		private float maxSliderLastValue;

		public bool deathDialogOpen = false;
		//Did they confirm they wanted to keep the slider above the Hard Limit?
		public bool peaceForeverHeld = false;

		FullnessAndDietStats_ThingComp fullnessAndDietStats;
		bool enabled = true;
    }
}
