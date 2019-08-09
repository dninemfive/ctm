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
        private const float JoyFactor = JoyTunings.BaseJoyGainPerHour / GenDate.TicksPerHour;
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
        public Pawn pawn
        {
            get
            {
                if (pawns.Count <= 0) return null;
                return pawns.ElementAt(0);
            }
        }
        public float FactorFromPassion(Passion p)
        {
            switch (p)
            {
                case Passion.None: return 0.75f;
                default:
                case Passion.Minor: return 1f;
                case Passion.Major: return 1.25f;
            }
        }

        public float JoyFactorFromInterest
        {
            get
            {
                float sum = 0f;
                foreach(SkillDef sd in skills) sum += FactorFromPassion(pawn.skills.GetSkill(sd).passion);
                return sum / skills.Count;
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
            if (pawn != null)
            {
                LearnSkills(1);
                GainJoy(1);
            }
            if (ShouldEject) par.EjectContents();
        }

        public override void CompTickRare()
        {
            base.CompTick();
            if (pawn != null)
            {
                LearnSkills(250);
                GainJoy(250);
            }
            if (ShouldEject) par.EjectContents();
        }

        public void LearnSkills(int ticks)
        {
            foreach (SkillDef sd in skills) pawn.skills.Learn(sd, xp * ticks, false);
        }

        public void GainJoy(int ticks)
        {
            pawn.needs.joy.GainJoy(parent.def.GetStatValueAbstract(StatDefOf.JoyGainFactor) * JoyFactor * JoyFactorFromInterest * ticks, parent.def.building.joyKind);
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
