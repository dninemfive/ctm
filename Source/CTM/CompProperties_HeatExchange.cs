using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9CTM
{
    class CompProperties_HeatExchange : CompProperties
    {
        public float standbyPower = 20000f; //rounded up from the per-tile yield of vanometric cells times the area of the heat exchange ((2400/2)*16)
        public float defaultTargetTemperature = 21f;
    }
}
