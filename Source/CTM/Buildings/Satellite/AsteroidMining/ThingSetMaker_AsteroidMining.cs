using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace D9CTM
{
    class ThingSetMaker_AsteroidMining : ThingSetMaker
    {
        private static List<ThingDef> ores => OreUtility.ores;
        private static Dictionary<ThingDef, float> weightedMineables => OreUtility.weightedMineables;
        private static Dictionary<ThingDef, ThingDef> mineable = new Dictionary<ThingDef, ThingDef>();

#pragma warning disable CS0649
        public List<ThingDefCountClass> guaranteedItems;
        public float amount;

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            foreach(ThingDefCountClass thing in guaranteedItems)
            {
                int count = thing.count;
                while (count > 0)
                {
                    Thing nextThing = ThingMaker.MakeThing(thing.thingDef);
                    if (count > thing.thingDef.stackLimit) nextThing.stackCount = thing.thingDef.stackLimit;
                    else nextThing.stackCount = count;
                    outThings.Add(nextThing);
                }
            }
            foreach(ThingDef td in ores)
            {
                weightedMineables.TryGetValue(td, out float value);
                int count = (int)(amount * value);
                while(count > 0)
                {
                    Thing nextThing = ThingMaker.MakeThing(td);
                    if (count > td.stackLimit) nextThing.stackCount = td.stackLimit;
                    else nextThing.stackCount = count;
                    outThings.Add(nextThing);
                }
            }
        }        

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            return ores;
        }
    }
}
