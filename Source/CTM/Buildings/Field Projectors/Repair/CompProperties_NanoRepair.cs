using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace D9CTM
{
    class CompProperties_NanoRepair : CompProperties
    {
        public float radius = 7f;
        public float repairHP10PerHour, healHP10PerHour;

        public CompProperties_NanoRepair()
        {
            compClass = typeof(CompNanoRepair);
        }
    }
}