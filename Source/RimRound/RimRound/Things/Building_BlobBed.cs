using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRound.Things
{
    public class Building_BlobBed : Building_Bed
    {
        public Pawn originatingPawn;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref originatingPawn, "originatingPawn");
        }
    }
}
