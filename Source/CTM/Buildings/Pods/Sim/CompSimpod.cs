using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompSimpod : ThingComp
    {
        public CompProperties_Simpod Props => (CompProperties_Simpod)base.props;
        public List<SkillDef> skills => Props.SkillsToTrain;
        public float xp => Props.XpPerTick;
        public bool outdoors => Props.SatisfiesOutdoors;
        public Building_Pod par;
        public List<Pawn> pawns
        {
            get
            {
                List<Pawn> ret = new List<Pawn>();
                foreach(Thing t in par.GetDirectlyHeldThings())
                {
                    Pawn p = t as Pawn;
                    if (p != null) ret.Add(p);
                }
                return ret;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            par = base.parent as Building_Pod;
            if (par == null) Log.Error("[Accessible Archotech] A thing with comp CompSimpod is not a Building_Pod.");
        }

        public override void CompTick()
        {
            base.CompTick();
            LearnSkills(1);
        }

        public override void CompTickRare()
        {
            base.CompTick();
            LearnSkills(250);
        }

        public void LearnSkills(int ticks)
        {
            foreach (Pawn p in pawns) foreach (SkillDef sd in skills) p.skills.Learn(sd, xp * ticks, false);
        }
    }
}
