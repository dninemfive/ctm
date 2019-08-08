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
        public List<Pawn> pawns //should only ever have one pawn, this is just in case
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
            GainJoy(1);
            if (ShouldEject) par.EjectContents();
        }

        public override void CompTickRare()
        {
            base.CompTick();
            LearnSkills(250);
            GainJoy(250);
            if (ShouldEject) par.EjectContents();
        }

        public void LearnSkills(int ticks)
        {
            foreach (Pawn p in pawns) foreach (SkillDef sd in skills) p.skills.Learn(sd, xp * ticks, false);
        }

        public void GainJoy(int ticks)
        {
            foreach(Pawn p in pawns) p.needs.joy.GainJoy((parent.def.GetStatValueAbstract(StatDefOf.JoyGainFactor) * 0.36f) / (2500f/ticks), parent.def.building.joyKind);
        }

        public bool ShouldEject
        {
            get
            {
                IEnumerable<Pawn> pawnsNeedingEjection = from x in pawns where (!par.Forced && x.needs.joy.CurLevel > 0.9999f) || x.needs.food.CurCategory != HungerCategory.Fed || x.needs.rest.CurCategory == RestCategory.Exhausted || !x.GetTimeAssignment().allowJoy select x;
                return pawnsNeedingEjection.Any();
            }
        }
    }
}
