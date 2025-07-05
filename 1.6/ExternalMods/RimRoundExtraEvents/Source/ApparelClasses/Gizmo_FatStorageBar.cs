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
    public class Gizmo_FatStorageBar
    {
        public Gizmo_FatStorageBar(CompFatStorage fatStorageComp, Gizmo_FatStorageStatus parentGizmo)
        {
            this.comp = fatStorageComp;
            this.parent = parentGizmo;
        }

        public void DrawOnGUI(Rect rect, float yPos)
        {
            this.DrawMeter(rect, yPos);
        }

        //Used in Gizmo on GUI only. Draws sliders, dividers and dashes
        private void DrawMeter(Rect inRect, float yPos)
        {
            this.marginSize = 1f;

            this.yPosition = yPos;

            Rect fullnessBarRect = new Rect
            {
                width = inRect.width - 2 * this.marginSize,
                height = Gizmo_FatStorageBar.BarHeight,
                x = inRect.x + this.marginSize,
                y = this.yPosition
            };

            DrawFullnessIndicatorBar(fullnessBarRect);

            DrawTickMarks(fullnessBarRect);

            DrawSoftLimitAndHardLimitDividers(fullnessBarRect);

            AddFullnessPercentageOnFullnessBar(fullnessBarRect);
        }

        private void DrawFullnessIndicatorBar(Rect fullnessBarRect)
        {
            bool flag = Mouse.IsOver(fullnessBarRect);

            Texture2D barTexture = this.parent.ShouldBarBeRed() ? OverSoftLimitTex : StrengthTex;
            Texture2D barHighlightTexture = this.parent.ShouldBarBeRed() ? OverSoftLimitHighlightTex : StrengthHighlightTex;

            //Makes the Main bars (the main indicator plus the black background)
            if (this.parent.ShouldHideBar())
                Widgets.FillableBar(fullnessBarRect, 1, DisabledTex, EmptyBarTex, true);
            else
                Widgets.FillableBar(fullnessBarRect, Math.Min(CurrentFatStoredAsPercentOfDisplayLimit, 1), flag ? barHighlightTexture : barTexture, EmptyBarTex, true);
        }

        private void DrawTickMarks(Rect rect1)
        {

            if (this.parent.ShouldHideBar()) return;

            //Draws the little ticks at the bottom

            //This is the amount in kgs each dash represents
            float dashInterval = this.SoftLimit / 10;

            int numberOfDashes = (int)(DisplayLimit / dashInterval);
            int currentDashNumber = 0;

            float dashPercentInterval = dashInterval / DisplayLimit;

            for (int i = numberOfDashes; i > 0; --i)
            {
                ++currentDashNumber;
                DrawDash(rect1, currentDashNumber * dashPercentInterval, CurrentFatStoredAsPercentOfDisplayLimit);
            }
        }

        private void DrawSoftLimitAndHardLimitDividers(Rect rect1)
        {

            if (this.parent.ShouldHideBar()) return;

            //Soft Limit Dash
            if (this.comp.FatStoredHardLimitPercent > 0.0f) //Don't draw the soft limit if it's also the hard limit
            DrawDivider(
                rect1,
                SoftLimitAsPercentage,
                CurrentFatStoredAsPercentOfDisplayLimit,
                DividerTexSoft,
                DividerTexSoft
                );

            //Hard Limit Dash
            DrawDivider(
                rect1,
                HardLimitAsPercentage,
                CurrentFatStoredAsPercentOfDisplayLimit,
                DividerTexHard,
                DividerTexHard
                );
        }

        private void AddFullnessPercentageOnFullnessBar(Rect fullnessBarRect)
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            DrawFullnessLabel(fullnessBarRect);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private void DrawFullnessLabel(Rect rect)
        {
            String percentageString = this.parent.ShouldHideBar() ? "???" : (this.CurrentFatStoredAsPercentOfSoftLimit * 100).ToString("F0");
            String contentsString = this.parent.ShouldHideContents() ? "???" : Gizmo_FatStorageBar.ShortenLabel(this.comp.FatStored, false);
            String capacityString = this.parent.ShouldHideCapacity() ? "???" : Gizmo_FatStorageBar.ShortenLabel(this.SoftLimit, true);

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
                    //OLD
                    //$"{(this.CurrentFatStoredAsPercentOfSoftLimit * 100).ToString("F0")}% ({this.comp.FatStored.ToString("F1")}/{this.SoftLimit.ToString("F1")}kgs)");
                    $"{percentageString}% ({contentsString}/{capacityString}kg)");
                Text.Anchor = TextAnchor.UpperLeft;
        }

        private static String ShortenLabel(float number, bool hasUnit)
        {
            if (number < 0f)
            {
                Log.Warning("class RimRoundExtraEvents.ApparelClasses.Gizmo_FatStorageBar method ShortenLabel() was fed a negative number, it can only accept positive numbers.");
                return number.ToString("F1");
            }

            int i = number > 0 ? (int)Math.Floor(Math.Log10((double)number) / 3d) : 0;

            float newNumber = (float)(number / Math.Pow(1000, i));

            if (i >= Gizmo_FatStorageBar.shorteners.Length)
            {
                Log.Warning("class RimRoundExtraEvents.ApparelClasses.Gizmo_FatStorageBar method ShortenLabel() was somehow fed a float larger than float.MaxValue.");
                return newNumber.ToString("F1") + "?" + (hasUnit ? " " : "");
            }

            String shortener = Gizmo_FatStorageBar.shorteners[i];

            return newNumber.ToString("F1") + shortener + (hasUnit && shortener != "" ? " " : "");
        }

        //Used only in DrawSoftLimitAndHardLimitDividers(). Draws the little rectangles (dashes) on the meter.
        private void DrawDivider(Rect rect, float percent, float curValue, Texture2D underTex, Texture2D exceedTex)
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

        //Used only in DrawTickMarks(). Draws the little rectangles (dashes) on the meter.
        private void DrawDash(Rect rect, float placementPercent, float curValue)
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

        //Textures
        private static readonly Texture2D OverSoftLimitTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.BrickRed);
        private static readonly Texture2D OverSoftLimitHighlightTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Red);
        private static readonly Texture2D StrengthTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Orange);
        private static readonly Texture2D StrengthHighlightTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.LightOrange);
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));
        //private static readonly Texture2D StrengthTargetTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.DarkOrange);
        private static readonly Texture2D DisabledTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Grey);

        private static readonly Texture2D DividerTexHard = ContentFinder<Texture2D>.Get("UI/Misc/NeedUnitDivider", true);
        private static readonly Texture2D DividerTexSoft = ContentFinder<Texture2D>.Get("UI/Misc/NeedUnitDivider", true);

        //Shorteners used by ShortenLabel(), going up to float.MaxValue just in case
        private static readonly string[] shorteners = {
            "", //None
            "k", //Thousand(Kilo)
            "m", //Million
            "b", //Billion
            "qa", //Quadrillion
            "qu", //Quintillion
            "sx", //Sextillion
            "sp", //Septillion
            "oc", //Octillion
            "no", //Nonillion
            "de", //Decillion
            "un" //Undecillion
        };

        //Bar rendering values
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

        float marginSize;

        public static float BarHeight
        {
            get
            {
                return 26f;
            }
        }

        //Bar limits in kgs
        public float SoftLimit
        {
            get
            {
                return this.comp.FatStoredSoftLimit;
            }
        }

        public float HardLimit
        {
            get
            {
                return this.comp.FatStoredHardLimit;
            }
        }

        //How much of the bar is occupied by the hard limit overflow
        private float breakDisplayPercentage = 0.01f;
        public float DisplayLimit
        {
            get
            {
                return this.comp.FatStoredHardLimit / (1f - this.breakDisplayPercentage);
            }
        }

        //Limits as percentage of DisplayLimit
        public float SoftLimitAsPercentage
        {
            get
            {
                return this.SoftLimit / this.DisplayLimit;
            }
        }

        public float HardLimitAsPercentage
        {
            get
            {
                return this.HardLimit / this.DisplayLimit;
            }
        }

        //Fat stored as percentage of limits
        public float CurrentFatStoredAsPercentOfSoftLimit
        {
            get
            {
                return this.comp.FatStored / this.SoftLimit;
            }
        }

        public float CurrentFatStoredAsPercentOfDisplayLimit
        {
            get
            {
                return this.comp.FatStored / DisplayLimit;
            }
        }

        protected CompFatStorage comp;
        protected Gizmo_FatStorageStatus parent;
    }
}
