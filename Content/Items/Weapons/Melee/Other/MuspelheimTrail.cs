using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Other
{
    public class SpearTrail
    {
        public const int Substeps = 3;
        private const int MaxLife = 40;

        private struct Pt
        {
            public Vector2 Tip;
            public Vector2 Fwd;
            public float Born;
        }

        private readonly List<Pt> pts = new List<Pt>();
        private readonly int life;
        private float tick;

        private static BasicEffect effect;
        private static readonly VertexPositionColorTexture[] verts = new VertexPositionColorTexture[1024];

        public SpearTrail(int life)
        {
            this.life = Math.Min(life, MaxLife);
        }

        public int Count => pts.Count;

        public void Step() => tick++;

        public void Add(Vector2 tip, Vector2 fwd, float subOffset)
        {
            pts.Add(new Pt { Tip = tip, Fwd = fwd, Born = tick - subOffset });
        }

        public void Prune()
        {
            while (pts.Count > 0 && tick - pts[0].Born > life)
                pts.RemoveAt(0);
        }

        public static void Unload()
        {
            BasicEffect e = effect;
            effect = null;
            if (e != null)
                Main.QueueMainThreadAction(() => e.Dispose());
        }

        public void Draw(float global, bool sideways, float length)
        {
            if (Main.dedServ || pts.Count < 3 || global <= 0.01f)
                return;

            GraphicsDevice gd = Main.instance.GraphicsDevice;

            if (effect == null || effect.IsDisposed)
            {
                effect = new BasicEffect(gd)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = true,
                    LightingEnabled = false,
                    World = Matrix.Identity
                };
            }

            effect.Texture = TextureAssets.MagicPixel.Value;
            effect.View = Main.GameViewMatrix.TransformationMatrix;
            effect.Projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            Main.spriteBatch.End();

            gd.BlendState = BlendState.Additive;
            gd.DepthStencilState = DepthStencilState.None;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.SamplerStates[0] = SamplerState.LinearClamp;
            effect.CurrentTechnique.Passes[0].Apply();

            Color black = new Color(0, 0, 0);

            if (!sideways)
            {
                Ribbon(false, 0.45f * global,
                    (0f, new Color(255, 110, 25)),
                    (-length * 0.45f, black));
                Ribbon(false, 0.8f * global,
                    (6f, new Color(255, 170, 60)),
                    (-length * 0.12f, new Color(60, 10, 0)));
                Ribbon(false, 1.0f * global,
                    (3.5f, new Color(255, 250, 215)),
                    (-3.5f, new Color(255, 200, 100)));
            }
            else
            {
                float w = length * 0.12f;
                Ribbon(true, 0.5f * global,
                    (-w, black), (0f, new Color(255, 110, 25)), (w, black));
                Ribbon(true, 0.8f * global,
                    (-6f, black), (0f, new Color(255, 170, 60)), (6f, black));
                Ribbon(true, 1.0f * global,
                    (-2.5f, new Color(255, 200, 100)), (0f, new Color(255, 250, 215)), (2.5f, new Color(255, 200, 100)));
            }

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void Ribbon(bool sideways, float intensity, params (float off, Color col)[] rows)
        {
            int n = pts.Count;
            if (n < 2 || rows.Length < 2)
                return;

            GraphicsDevice gd = Main.instance.GraphicsDevice;

            for (int r = 0; r < rows.Length - 1; r++)
            {
                int v = 0;
                for (int k = 0; k < n; k++)
                {
                    Pt p = pts[k];
                    float age = MathHelper.Clamp((tick - p.Born) / life, 0f, 1f);

                    float fade = MathF.Pow(1f - age, 1.6f) * intensity;
                    float taper = 1f - age * 0.5f;

                    Vector2 normal = sideways ? new Vector2(-p.Fwd.Y, p.Fwd.X) : p.Fwd;

                    Vector2 pa = p.Tip + normal * rows[r].off * taper - Main.screenPosition;
                    Vector2 pb = p.Tip + normal * rows[r + 1].off * taper - Main.screenPosition;

                    verts[v++] = new VertexPositionColorTexture(new Vector3(pa, 0f), rows[r].col * fade, Vector2.Zero);
                    verts[v++] = new VertexPositionColorTexture(new Vector3(pb, 0f), rows[r + 1].col * fade, Vector2.Zero);
                }

                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, v - 2);
            }
        }
    }
}