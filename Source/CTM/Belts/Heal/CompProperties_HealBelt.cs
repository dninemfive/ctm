﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9CTM
{
    class CompProperties_HealBelt : CompProperties
    {
        public float HealChance10PerHour = 1f, BloodLossReduction10PerHour = 0f, TendQuality = 0f;
        public int HealsPerProc = 1, HPPerHeal = 1, MaxWoundsToTend = 0;
        public bool IncreaseImmunity = false, TendWounds = false;

        public CompProperties_HealBelt()
        {
            compClass = typeof(CompHealBelt);
        }
    }
}
