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
        // Formerly a for-statement, but this is more concise. Kept for readability's sake.
        public static bool Immunizable(Hediff h)
        {
            return h.TryGetComp<HediffComp_Immunizable>() != null;
        }
    }
}
