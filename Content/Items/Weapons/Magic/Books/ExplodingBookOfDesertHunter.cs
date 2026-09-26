using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Projectiles;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Magic.Books
{
    public class ExplodingBookOfDesertHunter : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.Magic;
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1;
            Item.value = Item.sellPrice(0, 0, 50, 0);
            Item.rare = 2;
            Item.channel = true;
            Item.mana = 12;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<ExplodingBookOfDesertHunterP>();
            Item.shootSpeed = 12;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<DesertCore>(1)
                .AddIngredient<DesertWreckage>(8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class ExplodingBookOfDesertHunterP : ModProjectile
    {
        private int redPulseTimer = 0;

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 22;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.scale = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = false;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            float bob = (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 3f;
            Vector2 basePos = player.direction == 1
                ? player.Center - new Vector2(4f, 5f)
                : player.Center - new Vector2(27f, 5f);
            Projectile.position = basePos + new Vector2(0f, bob);

            Projectile.spriteDirection = player.direction;

            player.itemRotation = 0f;
            player.itemLocation = Projectile.Center;
            player.itemTime = 2;
            player.itemAnimation = 2;

            if (redPulseTimer > 0)
                redPulseTimer--;

            if (Projectile.ai[0] > 75 && Projectile.ai[1] == 0)
            {
                Projectile.ai[1] = 3 + Projectile.localAI[0];
                Projectile.ai[0] = 10;
                Projectile.localAI[0] = MathHelper.Min(Projectile.localAI[0] + 1, 3);
            }
            else if (Projectile.ai[1] > 0)
            {
                if (Projectile.ai[0] <= 0)
                {
                    Projectile.ai[1]--;
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Vector2 vel = (new Vector2(Main.screenPosition.X + Main.mouseX, Main.screenPosition.Y + Main.mouseY) - Projectile.Center).SafeNormalize(Vector2.UnitX);
                        Vector2 pos = Projectile.Center + vel.RotatedBy(MathHelper.ToRadians(90)) * Main.rand.NextFloat(-33, 33);
                        int k = 3;
                        while (!Collision.CanHit(player.Center, 1, 1, pos, 14, 14))
                        {
                            pos = Projectile.Center + vel.RotatedBy(MathHelper.ToRadians(90)) * Main.rand.NextFloat(-33 + k, 33 - k);
                            k += 3;
                        }
                        Projectile.NewProjectile(Projectile.InheritSource(Projectile), pos, vel * player.inventory[player.selectedItem].shootSpeed, ModContent.ProjectileType<ExplodingBookOfDesertHunterP1>(), Projectile.damage, Projectile.knockBack, Main.myPlayer);

                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2CircularEdge(0.5f, 1f).RotatedBy(vel.ToRotation());
                            Vector2 ppos = pos + Main.rand.NextVector2Circular(4f, 4f);
                            ParticleManager.NewParticle<FlameParticleOld>(
                                ppos,
                                speed * 7.5f,
                                new Color(255, 200, 120),
                                Main.rand.NextFloat(0.15f, 0.3f),
                                1f
                            );
                        }

                        redPulseTimer = 12;

                        player.CheckMana(player.inventory[player.selectedItem].mana, true);
                        Projectile.netUpdate = true;
                    }
                    Projectile.ai[0] = 10;
                }
                Projectile.ai[0]--;
            }
            else
                Projectile.ai[0]++;

            float iMax = (Projectile.ai[0] / 25f);
            int suctionCount = (int)(iMax * 2.5f);
            for (int i = 0; i < suctionCount; i++)
            {
                float radius = Main.rand.NextFloat(50f, 120f);
                Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius);
                Vector2 spawnPos = Projectile.Center + offset;

                Vector2 vel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(6f, 12f) + player.velocity * 0.5f;

                ParticleManager.NewParticle<FlameParticleOld>(
                    spawnPos,
                    vel,
                    new Color(255, 200, 120),
                    Main.rand.NextFloat(0.12f, 0.25f) * iMax,
                    1f
                );

                if (Main.rand.NextBool(6))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        spawnPos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(4f, 8f)),
                        new Color(255, 160, 50, 200),
                        Main.rand.Next(16, 28)
                    ));
                }
            }

            if (player.noItems || player.CCed || player.dead || !player.active || !player.channel || !player.CheckMana(player.inventory[player.selectedItem].mana, false))
                Projectile.Kill();
            else
                Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (redPulseTimer > 0)
            {
                float t = redPulseTimer / 12f;
                Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
                Rectangle frame = tex.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
                Vector2 origin = frame.Size() / 2f;
                Vector2 pos = Projectile.Center - Main.screenPosition;

                int layers = 6;
                for (int i = layers; i > 0; i--)
                {
                    float offset = i * 2f * t;
                    float alpha = (1f - (i / (float)layers)) * t * 0.55f;
                    Color outlineColor = new Color(255, 40, 40) * alpha;

                    for (int d = 0; d < 8; d++)
                    {
                        float angle = MathHelper.TwoPi * d / 8f;
                        Vector2 dir = angle.ToRotationVector2() * offset;
                        Main.EntitySpriteDraw(
                            tex,
                            pos + dir,
                            frame,
                            outlineColor,
                            Projectile.rotation,
                            origin,
                            Projectile.scale,
                            Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                            0f
                        );
                    }
                }
            }
            return true;
        }

        public override bool? CanHitNPC(NPC target) => false;
        public override bool ShouldUpdatePosition() => false;

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }
    }

    public class ExplodingBookOfDesertHunterP1 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.penetrate = 2;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.scale = 0.75f;
            Projectile.timeLeft = 360;
            Projectile.extraUpdates = 1;
            DrawOffsetX = -30;
            DrawOriginOffsetX = 11;
        }

        public override void AI()
        {
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);

            if (Projectile.spriteDirection == 1)
            {
                DrawOffsetX = -30;
                DrawOriginOffsetX = 11;
            }
            else
            {
                DrawOffsetX = 0;
                DrawOriginOffsetX = -11;
            }

            Vector2 perp = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            float wobble = (float)System.Math.Sin(Projectile.timeLeft * 0.35f) * 0.15f;
            Projectile.velocity += perp * wobble;
            Projectile.velocity += Main.rand.NextVector2Circular(0.05f, 0.05f);

            if (Main.rand.NextBool(2))
            {
                ParticleManager.NewParticle<FlameParticleOld>(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    new Color(255, 200, 120),
                    Main.rand.NextFloat(0.15f, 0.3f),
                    1f
                );
            }
            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(2f, 6f);
                Vector2 vel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.5f, 0.5f);

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    Color.White,
                    0.5f * Main.rand.NextFloat(0.3f, 0.5f),
                    1f
                );

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    Color.White,
                    0.5f * Main.rand.NextFloat(0.3f, 0.5f),
                    1f
                );
            }
            if (Main.rand.NextBool(4))
            {
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    (-Projectile.velocity * 0.1f).ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(4f, 8f)),
                    new Color(255, 160, 50, 200),
                    Main.rand.Next(16, 28)
                ));
            }

            Lighting.AddLight(Projectile.Center, 1.5f, 0.75f, 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage /= 2;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 25; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 9f);
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    new Color(255, 200, 120),
                    Main.rand.NextFloat(0.2f, 0.45f),
                    1f
                );

                if (Main.rand.NextBool(2))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(6f, 14f)),
                        new Color(255, 160, 50, 200),
                        Main.rand.Next(16, 28)
                    ));
                }
            }

            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(10f, 20f)),
                    new Color(255, 120, 40, 220),
                    Main.rand.Next(20, 34)
                ));
            }

        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * 0.85f;
        }
    }
}