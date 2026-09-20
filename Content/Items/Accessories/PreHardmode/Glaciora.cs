using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Accessories.PreHardmode
{
    public class Glaciora : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 1, 50, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<GlacioraPlayer>().glacioraEquipped = true;
        }

        //public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        //{
        //    Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        //    Texture2D glow = ModContent.Request<Texture2D>(Texture + "_Glow", AssetRequestMode.ImmediateLoad).Value;
        //    Vector2 pos = Item.Center - Main.screenPosition;
        //    Vector2 origin = tex.Size() * 0.5f;
        //    Vector2 glowOrigin = glow.Size() * 0.5f;

        //    float time = Main.GlobalTimeWrappedHourly * 2.5f;
        //    float pulse = 0.85f + MathF.Sin(time) * 0.15f;

        //    Texture2D megaspark = ModContent.Request<Texture2D>("Waybound/Particles/Megaspark", AssetRequestMode.ImmediateLoad).Value;
        //    Vector2 megaOrigin = megaspark.Size() * 0.5f;
        //    float aspect = megaspark.Width / (float)megaspark.Height;
        //    float baseScale = scale * 0.09f * pulse;
        //    Vector2 scaleVec = new Vector2(baseScale / aspect, baseScale);

        //    Vector2 glowPos = pos + new Vector2(0f, -6f);

        //    Main.EntitySpriteDraw(megaspark, glowPos, null,
        //        new Color(60, 130, 235) * 0.55f,
        //        time * 0.15f, megaOrigin, scaleVec, SpriteEffects.None, 0f);
        //    Main.EntitySpriteDraw(megaspark, glowPos, null,
        //        new Color(110, 185, 255) * 0.7f,
        //        -time * 0.25f, megaOrigin, scaleVec * 0.78f, SpriteEffects.None, 0f);
        //    Main.EntitySpriteDraw(megaspark, glowPos, null,
        //        new Color(175, 220, 255) * 0.85f,
        //        time * 0.4f, megaOrigin, scaleVec * 0.52f, SpriteEffects.None, 0f);
        //    Main.EntitySpriteDraw(megaspark, glowPos, null,
        //        new Color(235, 248, 255) * 0.95f,
        //        -time * 0.6f, megaOrigin, scaleVec * 0.28f, SpriteEffects.None, 0f);

        //    Main.EntitySpriteDraw(glow, pos, null, Color.White * 0.9f, rotation, glowOrigin, scale * 1.05f, SpriteEffects.None, 0f);
        //    Main.EntitySpriteDraw(tex, pos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
        //    return false;
        //}
    }

    public class GlacioraPlayer : ModPlayer
    {
        public bool glacioraEquipped;
        private bool wasFalling;
        private float fallStartY;

        public override void ResetEffects()
        {
            glacioraEquipped = false;
        }

        public override void PreUpdate()
        {
            if (!glacioraEquipped)
            {
                wasFalling = false;
                return;
            }

            if (Player.velocity.Y > 0 && !Player.justJumped && !Player.mount.Active)
            {
                if (!wasFalling)
                {
                    wasFalling = true;
                    fallStartY = Player.position.Y;
                }
            }
            else
            {
                wasFalling = false;
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!glacioraEquipped)
                return;

            if (modifiers.DamageSource.SourceOtherIndex == 0)
            {
                modifiers.FinalDamage *= 0.5f;
            }
        }

        public override void PostUpdate()
        {
            if (!glacioraEquipped)
                return;

            if (Player.velocity.Y == 0 && wasFalling)
            {
                float fallDist = Player.position.Y - fallStartY;
                if (fallDist > 16f * 8f)
                {
                    SpawnIceFlash(fallDist);
                    wasFalling = false;
                }
            }
        }

        private void SpawnIceFlash(float fallDist)
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            float tiles = fallDist / 16f;
            float scaleMult = MathHelper.Clamp((tiles - 8f) / 40f, 0.35f, 1.35f);

            Vector2 pos = Player.Center;
            int proj = Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                pos,
                Vector2.Zero,
                ModContent.ProjectileType<GlacioraFlash>(),
                30,
                2f,
                Player.whoAmI,
                scaleMult
            );
            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].netUpdate = true;
            }
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.45f + scaleMult * 0.25f, Pitch = -0.15f }, pos);
        }
    }

    public class GlacioraFlash : ModProjectile
    {
        private float glowPulse;
        private float glowIntensity = 1.4f;
        private const int Lifetime = 28;
        private float sizeMult;
        private Vector2 spawnCenter;

        public override string Texture => "Waybound/Particles/Megaspark";

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.aiStyle = -1;
            Projectile.hide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            sizeMult = Projectile.ai[0];
            if (sizeMult <= 0f)
                sizeMult = 1f;

            spawnCenter = Projectile.Center;

            Projectile.scale = 2.2f * sizeMult;
            int size = (int)(70f * sizeMult);
            Projectile.width = size;
            Projectile.height = size;
            Projectile.Center = spawnCenter;
        }

        public override void AI()
        {
            Projectile.Center = spawnCenter;

            glowPulse += 0.12f;
            float lifeRatio = Projectile.timeLeft / (float)Lifetime;
            glowIntensity = MathHelper.Lerp(1.6f, 0f, 1f - lifeRatio * lifeRatio);

            if (Projectile.timeLeft == Lifetime - 1)
            {
                float radius = 90f * sizeMult;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Distance(Projectile.Center) < radius)
                    {
                        npc.AddBuff(BuffID.Frostburn, 60);
                        npc.AddBuff(BuffID.Frozen, 60);
                    }
                }
            }

            Lighting.AddLight(Projectile.Center, 0.45f * glowIntensity * sizeMult, 0.7f * glowIntensity * sizeMult, 1.15f * glowIntensity * sizeMult);

            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(35f * sizeMult, 22f * sizeMult),
                    DustID.IceTorch,
                    Main.rand.NextVector2Circular(1.2f, 1.2f),
                    80,
                    new Color(140, 210, 255),
                    Main.rand.NextFloat(0.7f, 1.15f) * sizeMult
                );
                d.noGravity = true;
                d.fadeIn = 1.05f;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.DisableCrit();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("Waybound/Particles/Megaspark", AssetRequestMode.ImmediateLoad).Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = glow.Size() * 0.5f;
            float aspect = glow.Width / (float)glow.Height;
            float swell = MathF.Sin(MathHelper.Pi * (1f - Projectile.timeLeft / (float)Lifetime));
            float scale = Projectile.scale * 0.095f * (0.45f + swell * 1.25f);
            float alpha = glowIntensity * swell;
            Vector2 scaleVec = new Vector2(scale / aspect, scale);

            Main.EntitySpriteDraw(glow, pos, null,
                new Color(60, 130, 235) * (alpha * 0.75f),
                glowPulse * 0.15f, glowOrigin, scaleVec, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(glow, pos, null,
                new Color(110, 185, 255) * (alpha * 0.9f),
                -glowPulse * 0.25f, glowOrigin, scaleVec * 0.78f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(glow, pos, null,
                new Color(175, 220, 255) * alpha,
                glowPulse * 0.4f, glowOrigin, scaleVec * 0.52f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(glow, pos, null,
                new Color(235, 248, 255) * alpha,
                -glowPulse * 0.6f, glowOrigin, scaleVec * 0.28f, SpriteEffects.None, 0f);
            return false;
        }
    }
}