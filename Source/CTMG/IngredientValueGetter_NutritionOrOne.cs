using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace D9CTM
{
    /// <summary>
    /// Gets nutrition from inputs, but returns 1 for non-nutritious ingredients to allow for mixing food and non-food ingredients.
    /// 
    /// NOTE: will not prevent the use of non-food ingredients if you select overly broad filters.
    /// </summary>
    public class IngredientValueGetter_NutritionOrOne : IngredientValueGetter
    {
        public override float ValuePerUnitOf(ThingDef t)
        {
            if (!t.IsNutritionGivingIngestible) return 1f;
            return t.GetStatValueAbstract(StatDefOf.Nutrition);
        }
        public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
        {
            return "BillRequiresNutrition".Translate(ing.GetBaseCount()) + " (" + ing.filter.Summary + ")";
        }
    }
}
