using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace D9CTM
{
    class WorldComponent_ArchotechTracker : WorldComponent
    {
        WorldComponent_ArchotechTracker(World world) : base(world)
        {

        }
        public List<CompArchotech> Archotechs => archotechs.ToList();
        private IEnumerable<CompArchotech> archotechs
        {
            get
            {
                foreach (Map map in Find.Maps)
                {
                    IEnumerable<Building> archotechThings = map.listerBuildings.allBuildingsColonist.Where(x => x.def.HasComp(typeof(CompArchotech)));
                    foreach(Thing t in archotechThings) yield return t.TryGetComp<CompArchotech>();
                }
            }
        }
        public void EndGame()
        {
            GenGameEnd.EndGameDialogMessage("GameOverArchotechTakeover".Translate(), false, new UnityEngine.Color(0,0,0));
        }
    }
}
