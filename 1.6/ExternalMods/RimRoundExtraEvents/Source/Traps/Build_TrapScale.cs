using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.Traps
{
    public class Build_TrapScale : Building_Trap
    {
        protected override void SpringSub(Pawn p)
        {
            //base.GetComp<CompExplosive>().StartWick(null);
            //Scale that sets your weight to 400lbs
        }
    }
}
