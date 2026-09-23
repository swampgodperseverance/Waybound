using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SysVector2 = System.Numerics.Vector2;
using Waybound.Common.Rarities;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Shortswords
{
    public class IcebornRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ModContent.RarityType<IceShimer>();
            Item.value = Item.sellPrice(silver: 80);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 8;
            Item.knockBack = 4f;
            Item.crit = 6;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<IcebornRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<IcebornRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class IcebornRapierHoldout : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Shortswords/IcebornRapier";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.usesLocalNPCImmunity = true;
        }

        private float AimResponsiveness = 0.82f;
        private const int MaxCharge = 65;
        private int charge = 0;
        private float holdoutDistance = 36f;
        private float targetDistance = 36f;
        private int lungeTimer = 0;
        private bool isLunging = false;
        private bool returning = false;
        private Vector2 lastPos;
        private bool particlesSpawned = false;
        private float glowProgress = 0f;
        private float fadeIn = 0f;
        private float fadeOut = 1f;
        private bool isFadingOut = false;
        private float lungeStrength = 1f;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.channel || player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<IcebornRapier>())
            {
                isFadingOut = true;
            }

            if (isFadingOut)
            {
                fadeOut -= 0.08f;

                if (fadeOut <= 0f)
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                fadeIn = MathHelper.Lerp(fadeIn, 1f, 0.15f);
            }

            if (Projectile.timeLeft < 30)
                Projectile.timeLeft = 120;

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true, true);
            lastPos = Projectile.Center;

            if (player.channel && Projectile.owner == Main.myPlayer && !isFadingOut)
            {
                if (!isLunging)
                {
                    charge++;
                    UpdateAim(playerCenter);

                    if (charge >= MaxCharge)
                    {
                        isLunging = true;
                        returning = false;
                        lungeTimer = 0;
                        lungeStrength = MathHelper.Clamp(charge / (float)MaxCharge, 0.4f, 1f);
                        targetDistance = MathHelper.Lerp(90f, 210f, lungeStrength);
                        particlesSpawned = false;
                    }
                }
                else
                {
                    lungeTimer++;

                    if (!returning)
                    {
                        holdoutDistance = MathHelper.Lerp(holdoutDistance, targetDistance, 0.08f);

                        if (holdoutDistance > targetDistance - 8f)
                        {
                            returning = true;
                            targetDistance = 36f;
                        }
                    }
                    else
                    {
                        holdoutDistance = MathHelper.Lerp(holdoutDistance, targetDistance, 0.1f);

                        if (holdoutDistance < 42f)
                        {
                            isLunging = false;
                            charge = 0;
                            holdoutDistance = 36f;
                            lungeStrength = 1f;
                        }
                    }

                    if (!particlesSpawned && lungeTimer == 2)
                    {
                        particlesSpawned = true;
                        SpawnLungeParticles(playerCenter);
                    }

                    if (isLunging)
                    {
                        SpawnTrailParticles();
                    }
                }
            }

            float targetGlow = (isLunging || charge >= MaxCharge - 10) ? 1f : (charge / (float)MaxCharge) * 0.4f;
            glowProgress = MathHelper.Lerp(glowProgress, targetGlow, 0.08f);

            Projectile.Center = playerCenter + Projectile.velocity * holdoutDistance;

            if (Projectile.velocity.X > 0f)
            {
                player.ChangeDir(1);
                Projectile.spriteDirection = 1;
            }
            else
            {
                player.ChangeDir(-1);
                Projectile.spriteDirection = -1;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            if (Projectile.spriteDirection == -1)
                Projectile.rotation += MathHelper.PiOver2;

            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        private void UpdateAim(Vector2 source)
        {
            Vector2 aim = Vector2.Normalize(Main.MouseWorld - source);

            if (aim.HasNaNs())
                aim = -Vector2.UnitY;

            aim = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(Projectile.velocity), aim, AimResponsiveness));

            if (aim != Projectile.velocity)
                Projectile.netUpdate = true;

            Projectile.velocity = aim;
        }

        private void SpawnLungeParticles(Vector2 playerCenter)
        {
            Vector2 tip = Projectile.Center;

            int baseCount = (int)MathHelper.Lerp(20, 55, lungeStrength);
            int tipCount = (int)MathHelper.Lerp(25, 70, lungeStrength);

            for (int i = 0; i < baseCount; i++)
            {
                Vector2 spawnPos = playerCenter + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 vel = Vector2.Zero;
                float scale = Main.rand.NextFloat(8f, 16f) * MathHelper.Lerp(0.6f, 1f, lungeStrength);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    new Color(160, 225, 255, 255) * fadeIn,
                    Main.rand.Next(35, 65)
                ));
            }

            for (int i = 0; i < tipCount; i++)
            {
                Vector2 spawnPos = tip + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = Vector2.Zero;
                float scale = Main.rand.NextFloat(10f, 20f) * MathHelper.Lerp(0.6f, 1f, lungeStrength);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    new Color(145, 210, 255, 255) * fadeIn,
                    Main.rand.Next(40, 75)
                ));
            }
        }

        private void SpawnTrailParticles()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 spawnPos = Vector2.Lerp(lastPos, Projectile.Center, i / 8f) + Main.rand.NextVector2Circular(2f, 2f);
                Vector2 vel = Vector2.Zero;
                float scale = Main.rand.NextFloat(14f, 26f) * MathHelper.Lerp(0.6f, 1f, lungeStrength);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    new Color(150, 215, 255, 200) * fadeIn,
                    Main.rand.Next(14, 28)
                ));
            }
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            if (isLunging)
            {
                int size = (int)MathHelper.Lerp(30, 70, lungeStrength);
                hitbox.Inflate(size, size);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (isLunging)
            {
                float damageMult = MathHelper.Lerp(0.7f, 1.35f, lungeStrength);
                modifiers.FinalDamage *= damageMult;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int buffTime = isLunging ? (int)MathHelper.Lerp(90, 180, lungeStrength) : 90;
            
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float alpha = fadeIn * fadeOut;

            if (glowProgress > 0.01f)
            {
                Color outline = new Color(120, 200, 255, 180) * glowProgress * alpha;

                for (int i = 0; i < 8; i++)
                {
                    Vector2 offset = new Vector2(3f, 0).RotatedBy(MathHelper.TwoPi * i / 8f);
                    Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset,
                        null, outline * 0.7f, Projectile.rotation, origin, Projectile.scale * 1.05f, effects, 0);
                }
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
                null, lightColor * alpha, Projectile.rotation, origin, Projectile.scale, effects, 0);

            return false;
        }
    }
}