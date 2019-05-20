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
            return "BillRequires".Translate(ing.GetBaseCount(), ing.filter.Summary); //TODO: check if this returns an accurate description
        }
        //should probably be implemented at some point
        /*
        public override string ExtraDescriptionLine(RecipeDef r)
        {
            if (r.ingredients.Any((IngredientCount ing) => ing.filter.AllowedThingDefs.Any((ThingDef td) => td.smallVolume && !r.GetPremultipliedSmallIngredients().Contains(td))))
            {
                return "BillRequiresMayVary".Translate(10.ToStringCached());
            }
            return null;
        }
        */
    }
}
