using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace D9CTM
{
    public static class Utils
    {
        public static bool Immunizable(HediffWithComps h)
        {
            foreach (HediffComp c in h.comps) if (c is HediffComp_Immunizable) return true;
            return false;
        }
    }
}
