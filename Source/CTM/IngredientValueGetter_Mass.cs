using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace D9CTM
{
    class IngredientValueGetter_Mass : IngredientValueGetter
    {
        public override float ValuePerUnitOf(ThingDef t)
        {
            return t.BaseMass;
        }
        
        public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
        {
            if (ing.IsFixedIngredient) return "BillRequires".Translate(ing.GetBaseCount() / ing.FixedIngredient.BaseMass, ing.filter.Summary);
            return "BillRequiresMass".Translate(ing.GetBaseCount(), ing.filter.Summary);
        }
    }
}
