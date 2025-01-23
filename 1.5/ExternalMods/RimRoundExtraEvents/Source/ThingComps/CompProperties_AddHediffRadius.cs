using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRoundExtraEvents.ThingComps
{
    public class CompProperties_AddHediffRadius : CompProperties
    {
        public CompProperties_AddHediffRadius()
        {
            this.compClass = typeof(CompAddHediffRadius);
        }

        // Hediff to be applied
        public HediffDef hediffDeff;
        // Range at which the effect works
        // The display radius is set separately in the thing def as specialDisplayRadius
        public float effectRadius;
        // If severity is below this, set it to the value
        public float setSeverity;
        // Severity added per hour, up to maxSeverity
        // If the hediff passively loses severity, remember to set this higher
        public float severityPerHour;
        // Max value to which severity is added (setting ignores this)
        public float maxSeverity;
    }
}
