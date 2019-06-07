using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompProperties_Simpod : CompProperties
    {
        public List<SkillDef> SkillsToTrain;
        public JoyKindDef JoyKind = null;
        public bool SatisfiesOutdoors = false;
        public float XpPerTick = 0.1f;

        public CompProperties_Simpod()
        {
            compClass = typeof(CompSimpod);
        }
    }
}
