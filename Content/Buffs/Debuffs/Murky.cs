using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Waybound.Content.Buffs.Debuffs
{
    public class Murky : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
        }
    }

    public class MurkyCloud
    {
        public Vector2 OffsetFromPlayer;
        public Vector2 Velocity;
        public float Scale;
        public int Life;
        public int MaxLife;
        public float Phase;
        public float PhaseSpeed;
        public float BaseAlpha;
    }

    public class MurkySystem : ModSystem
    {
        private static Texture2D _clodTexture;
        public static Texture2D ClodTexture
        {
            get
            {
                if (_clodTexture == null)
                    _clodTexture = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Cloud", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                return _clodTexture;
            }
        }

        public static readonly List<MurkyCloud> Clouds = new List<MurkyCloud>();

        private const int MaxClouds = 200;
        private const float SafeRadius = 70f;

        public override void PostUpdateEverything()
        {
            Player player = Main.LocalPlayer;

            bool active = !Main.gameMenu && player != null && player.active && player.HasBuff(ModContent.BuffType<Murky>());

            if (active)
            {
                if (Clouds.Count < MaxClouds)
                {
                    int spawns = Main.rand.Next(4, 8);
                    for (int i = 0; i < spawns; i++)
                        SpawnCloud();
                }
            }
            else
            {
                for (int i = 0; i < Clouds.Count; i++)
                    Clouds[i].Life += 4;
            }

            for (int i = Clouds.Count - 1; i >= 0; i--)
            {
                MurkyCloud c = Clouds[i];
                c.Life++;

                c.Phase += c.PhaseSpeed;

                c.OffsetFromPlayer += c.Velocity;
                c.Velocity *= 0.97f;

                c.OffsetFromPlayer.X += (float)Math.Sin(c.Phase) * 0.06f;
                c.OffsetFromPlayer.Y += (float)Math.Cos(c.Phase * 0.8f) * 0.05f;

                float dist = c.OffsetFromPlayer.Length();
                if (dist < SafeRadius)
                {
                    Vector2 push = c.OffsetFromPlayer.SafeNormalize(Vector2.UnitX) * (SafeRadius - dist);
                    c.OffsetFromPlayer += push;
                }

                if (c.Life >= c.MaxLife)
                    Clouds.RemoveAt(i);
            }
        }

        private void SpawnCloud()
        {
            float halfW = Main.screenWidth * 0.5f;
            float halfH = Main.screenHeight * 0.5f;

            Vector2 offset;
            int attempts = 0;

            do
            {
                offset = new Vector2(
                    Main.rand.NextFloat(-halfW, halfW),
                    Main.rand.NextFloat(-halfH, halfH)
                );
                attempts++;
            }
            while (offset.Length() < SafeRadius && attempts < 10);

            if (offset.Length() < SafeRadius)
                offset = offset.SafeNormalize(Vector2.UnitX) * SafeRadius;

            float scale = Main.rand.NextFloat(1.8f, 3.6f);
            int lifeTime = Main.rand.Next(220, 420);

            Clouds.Add(new MurkyCloud
            {
                OffsetFromPlayer = offset,
                Velocity = Main.rand.NextVector2Circular(0.5f, 0.5f),
                Scale = scale,
                Life = 0,
                MaxLife = lifeTime,
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                PhaseSpeed = Main.rand.NextFloat(0.015f, 0.04f),
                BaseAlpha = Main.rand.NextFloat(0.3f, 0.55f)
            });
        }

        public override void PostDrawTiles()
        {
            if (Clouds.Count == 0)
                return;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
                return;

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            Color cloudColor = new Color(20, 30, 90);
            Vector2 playerCenter = player.Center;
            Vector2 origin = new Vector2(ClodTexture.Width / 2f, ClodTexture.Height / 2f);

            foreach (MurkyCloud c in Clouds)
            {
                float fadeIn = MathHelper.Clamp(c.Life / 45f, 0f, 1f);
                float fadeOut = MathHelper.Clamp((c.MaxLife - c.Life) / 70f, 0f, 1f);
                float alpha = fadeIn * fadeOut * c.BaseAlpha;

                float pulse = 1f + (float)Math.Sin(c.Phase * 2f) * 0.05f;
                float finalScale = c.Scale * pulse;

                Vector2 worldPos = playerCenter + c.OffsetFromPlayer;

                Main.spriteBatch.Draw(
                    ClodTexture,
                    worldPos - Main.screenPosition,
                    null,
                    cloudColor * alpha,
                    0f,
                    origin,
                    finalScale,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.spriteBatch.End();
        }

        public override void OnWorldUnload()
        {
            Clouds.Clear();
        }
    }
}