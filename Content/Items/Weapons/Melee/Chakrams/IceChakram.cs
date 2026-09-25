using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Rarities;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Melee.Chakrams
{
    public class IceChakram : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glow = ModContent.Request<Texture2D>(Texture + "_Glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 glowOrigin = glow.Size() * 0.5f;

            float time = Main.GlobalTimeWrappedHourly * 2.5f;
            float pulse = 0.85f + MathF.Sin(time) * 0.15f;

            Texture2D megaspark = ModContent.Request<Texture2D>("Waybound/Particles/Megaspark", AssetRequestMode.ImmediateLoad).Value;
            Vector2 megaOrigin = megaspark.Size() * 0.5f;
            float aspect = megaspark.Width / (float)megaspark.Height;
            float baseScale = scale * 0.09f * pulse;
            Vector2 scaleVec = new Vector2(baseScale / aspect, baseScale);

            Vector2 glowPos = pos + new Vector2(0f, -6f);

            Main.EntitySpriteDraw(megaspark, glowPos, null,
                new Color(60, 130, 235) * 0.55f,
                time * 0.15f, megaOrigin, scaleVec, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(megaspark, glowPos, null,
                new Color(110, 185, 255) * 0.7f,
                -time * 0.25f, megaOrigin, scaleVec * 0.78f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(megaspark, glowPos, null,
                new Color(175, 220, 255) * 0.85f,
                time * 0.4f, megaOrigin, scaleVec * 0.52f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(megaspark, glowPos, null,
                new Color(235, 248, 255) * 0.95f,
                -time * 0.6f, megaOrigin, scaleVec * 0.28f, SpriteEffects.None, 0f);

            Main.EntitySpriteDraw(glow, pos, null, Color.White * 0.9f, rotation, glowOrigin, scale * 1.05f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(tex, pos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ModContent.RarityType<IceShimer>();
            Item.value = Item.sellPrice(silver: 80);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 15;
            Item.knockBack = 4f;
            Item.crit = 6;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<IceChakramProjectile>();
            Item.shootSpeed = 13f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<IceChakramProjectile>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class IceChakramProjectile : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Chakrams/IceChakram";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.localNPCHitCooldown = 12;
            Projectile.usesLocalNPCImmunity = true;
        }

        private bool returning = false;
        private int timer = 0;
        private Vector2 lastPos;
        private float fadeIn = 0f;
        private float glowProgress = 0f;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            timer++;

            Projectile.rotation += 0.35f * (returning ? -1f : 1f);

            if (timer < 40)
            {
                Projectile.velocity *= 0.965f;
            }
            else if (!returning)
            {
                returning = true;
            }

            if (returning)
            {
                Vector2 toPlayer = player.RotatedRelativePoint(player.MountedCenter, true, true) - Projectile.Center;

                if (toPlayer.Length() < 30f)
                {
                    Projectile.Kill();
                    return;
                }

                toPlayer.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * 15f, 0.14f);
            }

            SpawnTrailParticles();

            fadeIn = MathHelper.Lerp(fadeIn, 1f, 0.15f);
            glowProgress = MathHelper.Lerp(glowProgress, 1f, 0.08f);

            lastPos = Projectile.Center;

            Lighting.AddLight(Projectile.Center, 0.55f, 0.85f, 1.15f);
        }

        private void SpawnTrailParticles()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 spawnPos = Vector2.Lerp(lastPos, Projectile.Center, i / 4f) + Main.rand.NextVector2Circular(3f, 3f);
                Vector2 vel = Vector2.Zero;
                float scale = Main.rand.NextFloat(12f, 20f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    new Color(150, 215, 255, 200),
                    Main.rand.Next(14, 24)
                ));
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

            for (int i = 0; i < 18; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 0, default, 1.4f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(4.5f, 4.5f);
            }

            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Frost, 0f, 0f, 0, default, 1.2f);
                d.noGravity = true;
                d.velocity *= 2f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;

            float alpha = fadeIn;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float fade = 1f - (float)i / Projectile.oldPos.Length;
                float trailScale = Projectile.scale * (1f - i * 0.05f);

                Main.EntitySpriteDraw(
                    texture,
                    Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                    null,
                    new Color(150, 215, 255, 0) * fade * 0.5f * alpha,
                    Projectile.oldRot[i],
                    origin,
                    trailScale,
                    SpriteEffects.None,
                    0
                );
            }
         
        

            if (glowProgress > 0.01f)
            {
                Color outline = new Color(120, 200, 255, 180) * glowProgress * alpha;

                for (int i = 0; i < 8; i++)
                {
                    Vector2 offset = new Vector2(3f, 0).RotatedBy(MathHelper.TwoPi * i / 8f);
                    Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset,
                        null, outline * 0.7f, Projectile.rotation, origin, Projectile.scale * 1.05f, SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
                null, lightColor * alpha, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

    }
}