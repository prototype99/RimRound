using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using RimRound.Comps;
using RimWorld;
using RimRound.Utilities;
using Verse.Sound;
using RimRound.Things;

namespace RimRound.UI
{
    public class MakeBlobIntoBedGizmo : Command_Toggle
    {
        public MakeBlobIntoBedGizmo(MakeBlobIntoBed_ThingComp comp, FullnessAndDietStats_ThingComp fndComp) 
        {
            this.comp = comp;
            this.fndComp = fndComp;
            defaultLabel = "Blob bed";
            defaultDesc = "Whether this pawn will serve as a resting place for other colonists";
            icon = Resources.BLOB_BED_ICON;
            isActive = () => comp.IsBed;
            toggleAction = () => { ToggleAction(); };
            Order = 401;
        }


        private void ToggleAction()
        {
            Resources.gizmoClick.PlayOneShotOnCamera(null);
            comp.IsBed = !comp.IsBed;
            if (comp.IsBed)
            {
                comp.SpawnBed();
            }
            else 
            {
                comp.ResetBed();
            }
        }




        MakeBlobIntoBed_ThingComp comp;
        FullnessAndDietStats_ThingComp fndComp;
    }
}
