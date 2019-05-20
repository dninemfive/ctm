using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace D9CTM
{
    class SimpodModExtension : DefModExtension
    {
        public List<SkillDef> SkillsToTrain;
        public JoyKindDef JoyKind;
        public Boolean SatisfiesOutdoors;
    }
}
