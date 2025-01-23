using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.ThingComps.ArchoFatStatue
{
    public class CompProperties_Useable_ArchoFatStatue : CompProperties_UseEffect
    {
        
        public CompProperties_Useable_ArchoFatStatue()
        {
            this.compClass = typeof(CompUseEffect_ArchoFatStatue);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string text in base.ConfigErrors(parentDef))
            {
                yield return text;
            }
            //TODO: put in config errors for commune target stuff
            yield break;
        }

        //TODO: come up with other configs like cooldown
        //or, mincooldown, or something that lets the ICommunicable override the cooldown

        public int cooldownTicks = 60000; //60000 ticks = 1 day

        //TODO: for future things, use a world/game/mapcomponent
    }
}
