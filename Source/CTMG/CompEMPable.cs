using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace D9CTM
{
    class CompEMPable : CompApparelScoreOffset
    {
        public new CompProperties_EMPable Props => (CompProperties_EMPable)base.props;
        public int CooldownTicks = 0;
        public bool EMPed => CooldownTicks > 0;
        // generally not used unless the EMP cooldown lasts long enough to leave combat but might as well
        public override float ApparelScoreOffset => EMPed ? 0f : base.ApparelScoreOffset;

        public override void CompTick()
        {
            if (CooldownTicks > 0) CooldownTicks--;
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (Props.damageDefs.Contains(dinfo.Def)) CooldownTicks = Props.cooldownTicks;
        }        
    }
    class CompProperties_EMPable : CompProperties_ApparelScoreOffset
    {
#pragma warning disable CS0649
        public List<DamageDef> damageDefs;
        public int cooldownTicks = 250;
#pragma warning restore CS0649

        public CompProperties_EMPable()
        {
            base.compClass = typeof(CompEMPable);
        }
    }
}
