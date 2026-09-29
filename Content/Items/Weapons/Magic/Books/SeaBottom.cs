using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Magic.Books
{
    public class SeaBottom : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 28;
            Item.DamageType = DamageClass.Magic;
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(0, 1, 20, 0);
            Item.rare = ItemRarityID.Pink;
            Item.channel = true;
            Item.mana = 10;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<SeaBottomHoldout>();
            Item.shootSpeed = 14f;
        }
    }

    public class SeaBottomHoldout : ModProjectile
    {
        private float fadeOpacity;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 22;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = false;
            Projectile.timeLeft = 2;
            fadeOpacity = 0f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float bob = (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 5f) * 2.5f;
            Vector2 basePos = player.direction == 1
                ? player.Center - new Vector2(4f, 6f)
                : player.Center - new Vector2(27f, 6f);
            Projectile.position = basePos + new Vector2(0f, bob);
            Projectile.spriteDirection = player.direction;
            player.itemRotation = 0f;
            player.itemLocation = Projectile.Center;
            player.itemTime = 2;
            player.itemAnimation = 2;

            bool canChannel = !player.noItems && !player.CCed && !player.dead && player.active && player.channel && player.CheckMana(player.inventory[player.selectedItem].mana, false);

            if (canChannel)
            {
                fadeOpacity = MathHelper.Lerp(fadeOpacity, 1f, 0.18f);
                Projectile.timeLeft = 20;

                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.ai[0]++;
                    if (Projectile.ai[0] >= 9)
                    {
                        Projectile.ai[0] = 0;
                        if (player.CheckMana(player.inventory[player.selectedItem].mana, true))
                        {
                            for (int b = 0; b < 2; b++)
                            {
                                Vector2 spawnPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-12f, 12f), -10f);
                                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.1f, 1.1f), -player.inventory[player.selectedItem].shootSpeed * Main.rand.NextFloat(0.9f, 1.1f));

                                Projectile.NewProjectile(
                                    Projectile.InheritSource(Projectile),
                                    spawnPos,
                                    vel,
                                    ModContent.ProjectileType<SeaBottomProj>(),
                                    Projectile.damage,
                                    Projectile.knockBack,
                                    player.whoAmI
                                );
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                Vector2 pVel = Main.rand.NextVector2Circular(1.2f, 1.2f) + new Vector2(0f, -2.5f);
                                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                                    Projectile.Center.ToNumerics(),
                                    pVel.ToNumerics(),
                                    Main.rand.NextFloat(MathHelper.TwoPi),
                                    new System.Numerics.Vector2(Main.rand.NextFloat(10f, 16f)),
                                    new Color(70, 180, 255, 200),
                                    Main.rand.Next(14, 22)
                                ));
                            }
                        }
                    }
                }
            }
            else
            {
                fadeOpacity = MathHelper.Lerp(fadeOpacity, 0f, 0.22f);
                if (fadeOpacity < 0.05f)
                    Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color color = lightColor * fadeOpacity;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }

        public override bool? CanHitNPC(NPC target) => false;
        public override bool ShouldUpdatePosition() => false;

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }
    }

    public class SeaBottomProj : ModProjectile
    {
        private float spinSpeed;
        private int phase;
        private float orientTimer;
        private Vector2 oldPos = Vector2.Zero;
        private readonly VertexStrip vertexStrip = new VertexStrip();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.penetrate = 3;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.scale = 1f;
            Projectile.timeLeft = 90;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            oldPos = Projectile.Center;
        }

        public override void AI()
        {
            if (phase == 0)
            {
                spinSpeed = 0.48f;
                phase = 1;
                orientTimer = 0f;
            }

            if (phase == 1)
            {
                Projectile.rotation += spinSpeed * Projectile.direction;
                spinSpeed *= 0.972f;
                Projectile.velocity *= 0.955f;
                orientTimer++;

                if (orientTimer > 16)
                {
                    Vector2 toMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
                    if (toMouse != Vector2.Zero)
                    {
                        float targetRot = toMouse.ToRotation() + MathHelper.PiOver4;
                        Projectile.rotation = Projectile.rotation.AngleLerp(targetRot, 0.09f);
                    }
                }

                if (spinSpeed < 0.07f && orientTimer > 26)
                {
                    phase = 2;
                    Vector2 dashDir = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = dashDir * 16.5f;
                    Projectile.rotation = dashDir.ToRotation() + MathHelper.PiOver4;

                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 pVel = dashDir.RotatedByRandom(0.3f) * Main.rand.NextFloat(1.8f, 4f);
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            Projectile.Center.ToNumerics(),
                            pVel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(Main.rand.NextFloat(11f, 18f)),
                            new Color(80, 190, 255, 210),
                            Main.rand.Next(15, 24)
                        ));
                    }
                }
            }
            else if (phase == 2)
            {
                Projectile.velocity *= 0.994f;
                Vector2 toMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
                if (toMouse != Vector2.Zero)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse * Projectile.velocity.Length(), 0.07f);
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                }

                if (Main.rand.NextBool(3))
                {
                    Vector2 vel = -Projectile.velocity * 0.07f + Main.rand.NextVector2Circular(0.4f, 0.4f);
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(9f, 15f)),
                        new Color(90, 200, 255, 190),
                        Main.rand.Next(13, 20)
                    ));
                }
            }

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.22f);

            Lighting.AddLight(Projectile.Center, 0.25f, 0.5f, 0.85f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(12f, 20f)),
                    new Color(80, 190, 255, 210),
                    Main.rand.Next(15, 26)
                ));
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(170, 230, 255, 190);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            if (phase == 2)
            {
                try
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    GameShaders.Misc["MagicMissile"].Apply(null);
                    vertexStrip.PrepareStripWithProceduralPadding(
                        Projectile.oldPos,
                        Projectile.oldRot,
                        progress => Color.Lerp(new Color(40, 110, 180, 120), new Color(80, 160, 220, 25), progress),
                        progress => 22f * Projectile.scale * (1f - progress * 0.75f),
                        -Main.screenPosition + Projectile.Size / 2f,
                        true
                    );
                    vertexStrip.DrawTrail();
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    for (int k = 0; k < Projectile.oldPos.Length; k++)
                    {
                        if (Projectile.oldPos[k] == Vector2.Zero)
                            continue;
                        float fade = 1f - k / (float)Projectile.oldPos.Length;
                        Main.EntitySpriteDraw(texture, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null,
                            new Color(40, 110, 180, 0) * fade * 0.35f, Projectile.oldRot[k], origin, Projectile.scale * (1f - k * 0.04f), SpriteEffects.None, 0);
                    }
                }

                if (oldPos != Vector2.Zero && oldPos != Projectile.Center)
                {
                    Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                    Color trailColor = new Color(40, 120, 190, 0) * 0.4f;
                    float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                    float trailScaleY = trailLength / trailTex.Height * 2.6f;
                    Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                        new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                        trailColor, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                        new Vector2(trailTex.Width * 0.5f, 0f),
                        new Vector2(Projectile.scale * 0.55f, trailScaleY), SpriteEffects.None, 0f);
                    Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                        new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                        trailColor * 0.25f, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                        new Vector2(trailTex.Width * 0.5f, 0f),
                        new Vector2(Projectile.scale * 0.25f, trailScaleY * 1.2f), SpriteEffects.None, 0f);
                }
            }

            Color outline = new Color(80, 190, 255) * 0.35f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(1.8f, 0f).RotatedBy(MathHelper.TwoPi * i / 6f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outline, Projectile.rotation, origin, Projectile.scale * 1.02f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, new Color(180, 235, 255, 210), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}