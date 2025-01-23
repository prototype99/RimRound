using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.JobGivers
{
    public class JobGiver_StimulatedBingeFood : JobGiver_BingeFood
    {
        protected override int IngestInterval(Pawn pawn)
        {
            return 10; //120
        }
    }
}
