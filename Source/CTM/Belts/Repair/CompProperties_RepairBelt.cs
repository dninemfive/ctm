using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9CTM
{
    class CompProperties_RepairBelt : CompProperties
    {
        public float RepairChance10PerHour = 1f;
        public int RepairsPerProc = 1, HPPerRepair = 1;

        public CompProperties_RepairBelt()
        {
            compClass = typeof(CompRepairBelt);
        }
    }
}
