using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class HediffComp_BrainChip : HediffComp
    {
        HediffCompProperties_BrainChip Props => (HediffCompProperties_BrainChip)base.props;
        const int severityChangeInterval = GenDate.TicksPerDay; //severity without archotech

        int ticksToSeverityChange;

        float baseSeverity {
            get
            {
                return parent.def.initialSeverity; 
            }            
        }
        float severityChange
        {
            get
            {
                return Props.severityDelta;
            }
        }
        

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (ticksToSeverityChange <= 0)
            {
                if (ArchotechUtility.ArchotechIsActive()) parent.Severity += severityChange;
                else if (parent.Severity >= (baseSeverity + severityChange)) parent.Severity -= severityChange;
                ticksToSeverityChange = severityChangeInterval;
            }
            else ticksToSeverityChange--;
        }

    }
}
