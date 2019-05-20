/* DEPRECATED - moved to Conveniences
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace D9CTM
{
    //TODO: actually add the gizmo to change radius
    public class Building_ModularTradeBeacon : Building
    {
        private float tradeRadius = 7.9f;
        private static List<IntVec3> tradeableCells = new List<IntVec3>();
        public IEnumerable<IntVec3> TradeableCells => TradeableCellsAround(base.Position, base.Map, this.tradeRadius);

        public static List<IntVec3> TradeableCellsAround(IntVec3 pos, Map map, float radius)
        {
            tradeableCells.Clear();
            if(!pos.InBounds(map))return tradeableCells;
            Region region = pos.GetRegion(map, RegionType.Set_Passable);
            if (region == null) return tradeableCells;
            RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null, delegate (Region r)
            {
                foreach (IntVec3 cell in r.Cells)
                {
                    if (cell.InHorDistOf(pos, radius))
                    {
                        tradeableCells.Add(cell);
                    }
                }
                return false;
            }, 13, RegionType.Set_Passable);
            return tradeableCells;
        }        

        public override IEnumerable<Gizmo> GetGizmos()
        {
            using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
            {
                if(enumerator.MoveNext())
                {
                    Gizmo baseGizmo = enumerator.Current;
                    yield return baseGizmo;
                }
            }
            yield return (Gizmo)new Command_Action {
                action = new Action(ChangeRadius);
                hotKey = KeyBindingDefOf.Misc2;
                defaultDesc = "CommandModularTradeBeaconChangeRadius".Translate();
            };
        }

        private void ChangeRadius()
        {

        }
    }
}
*/