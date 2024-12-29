using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using RimRound.UI;
using RimRound.Utilities;

namespace RimRound.Comps
{
    public class SleepingPosition_ThingComp : ThingComp
    {
        private bool? disabled = null;

        public bool Disabled
        {
            get
            {
                if (disabled == null)
                {
                    disabled = !this.parent.AsPawn().RaceProps.Humanlike || this.parent.AsPawn()?.needs?.food == null;
                }
                return disabled.GetValueOrDefault();
            }
        }

        public SleepingPosition_ThingComp() : base()
        {
            sleepPositionGizmo = new SleepingPositionGizmo(this);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Disabled) { yield break; }

            if (!this.parent.AsPawn().IsColonist && !this.parent.AsPawn().IsPrisonerOfColony && !Prefs.DevMode)
                yield break;

            if (GlobalSettings.showSleepPostureManagementGizmo)
                yield return sleepPositionGizmo;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Disabled) { return; }

            Scribe_Values.Look<int>(ref this.sleepIndex, "sleepingPos", Values.RandomInt(0, 3), false);
        }

        public int sleepIndex;
        readonly SleepingPositionGizmo sleepPositionGizmo;
    }
}
