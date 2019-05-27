using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace D9CTM
{
    [StaticConstructorOnStartup]
    class CompOPRBeam : ThingComp
    {
        CompProperties_OrbitalBeam Props => (CompProperties_OrbitalBeam)base.props;
        //initializer
        //render beam
        // check whether beam should end (tickRare)
        private const float angle = 0f;
        private Sustainer sustainer;
        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();
        private float BeamEndHeight => Props.width * .5f;

        public bool ShouldBeActive { get; set; }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            CheckSpawnSustainer();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public void StartAnimation()
        {
            CheckSpawnSustainer();
        }

        private void CheckSpawnSustainer()
        {
            if (ShouldBeActive && Props.sound != null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    sustainer = Props.sound.TrySpawnSustainer(SoundInfo.InMap(base.parent, MaintenanceType.PerTick));
                });
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (sustainer != null)
            {
                sustainer.Maintain();
                if (!ShouldBeActive)
                {
                    sustainer.End();
                    sustainer = null;
                }
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            //just placeholders until I figure out what effect these have on actual rendering
            int TicksPassed = 100;
            int TicksLeft = 100;
            int fadeOutDuration = 99;
            if (ShouldBeActive)
            {
                Vector3 drawPos = base.parent.DrawPos;
                IntVec3 size = base.parent.Map.Size;
                float num = ((float)size.z - drawPos.z) * 1.41421354f;
                Vector3 a = Vector3Utility.FromAngleFlat(angle - 90f);
                Vector3 a2 = drawPos + a * num * 0.5f;
                a2.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                float num2 = Mathf.Min((float)TicksPassed / 10f, 1f);
                Vector3 b = a * ((1f - num2) * num);
                float num3 = 0.975f + Mathf.Sin((float)TicksPassed * 0.3f) * 0.025f;
                if (TicksLeft < fadeOutDuration)
                {
                    num3 *= (float)TicksLeft / (float)fadeOutDuration;
                }
                Color color = Props.color;
                color.a *= num3;
                MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(a2 + a * BeamEndHeight * 0.5f + b, Quaternion.Euler(0f, angle, 0f), new Vector3(Props.width, 1f, num));
                Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);
                Vector3 pos = drawPos + b;
                pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                Matrix4x4 matrix2 = default(Matrix4x4);
                matrix2.SetTRS(pos, Quaternion.Euler(0f, angle, 0f), new Vector3(Props.width, 1f, BeamEndHeight));
                Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
            }
        }
    }
}
