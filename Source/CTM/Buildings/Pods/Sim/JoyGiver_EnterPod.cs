using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace D9CTM
{
    class JoyGiver_EnterPod : JoyGiver_InteractBuilding
    {
        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            return new Job(base.def.jobDef, t);
        }
    }
}
