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
using Terraria.GameContent.Creative;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Bars;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Magic.Books
{
    public class ColdFlare : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 16;
            Item.DamageType = DamageClass.Magic;
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Orange;
            Item.mana = 14;
            Item.UseSound = null;
            Item.autoReuse = false;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<ColdFlareCharge>();
            Item.shootSpeed = 1f;
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ColdFlareHeld>()] < 1)
            {
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<ColdFlareHeld>(),
                    0, 0, player.whoAmI);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ColdFlareCharge>()] < 1)
            {
                Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<ColdFlareCharge>(), damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.WaterBolt, 1)
                .AddIngredient(ItemID.IceTorch, 1)
                .AddIngredient<HielitiumBar>(7)
                .AddIngredient(ItemID.Book, 2)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }

    public class ColdFlarePlayer : ModPlayer
    {
        public float ChargeRatio;
    }

    public class ColdFlareHeld : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Magic/Books/ColdFlare";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<ColdFlare>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;

            Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            player.ChangeDir(direction.X > 0 ? 1 : -1);
            Projectile.direction = player.direction;
            Projectile.spriteDirection = player.direction;

            float charge = player.GetModPlayer<ColdFlarePlayer>().ChargeRatio;

            float holdDistance = 18f;
            Projectile.Center = player.MountedCenter + direction * holdDistance;

            float baseRotation = direction.ToRotation();
            if (player.direction == -1)
                baseRotation += MathHelper.Pi;

            float shake = 0f;
            if (charge > 0.05f)
            {
                shake = (float)Math.Sin(Main.GameUpdateCount * 0.55f) * 0.04f * charge
                      + (float)Math.Sin(Main.GameUpdateCount * 1.1f) * 0.02f * charge;
                Projectile.Center += new Vector2(
                    (float)Math.Sin(Main.GameUpdateCount * 0.7f) * charge * 1.2f,
                    (float)Math.Cos(Main.GameUpdateCount * 0.9f) * charge * 1.0f
                );
            }

            Projectile.rotation = baseRotation + shake * player.direction;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
                direction.ToRotation() - MathHelper.PiOver2 + shake * 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;

            SpriteEffects effects = Projectile.spriteDirection == -1
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            float charge = Main.player[Projectile.owner].GetModPlayer<ColdFlarePlayer>().ChargeRatio;
            float pulse = 0.55f + 0.45f * (0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.2f));
            float glowStrength = (0.35f + charge * 0.55f) * pulse;

            Color glowColor = new Color(90, 190, 255, 50) * glowStrength;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.4f + charge * 1.2f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 1.5f);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset,
                    null, glowColor, Projectile.rotation, origin, Projectile.scale * (1f + charge * 0.05f), effects, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
                null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }

    public class ColdFlareCharge : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private const float MaxCharge = 60f;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            var mp = player.GetModPlayer<ColdFlarePlayer>();

            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<ColdFlare>())
            {
                mp.ChargeRatio = 0f;
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            Projectile.Center = player.MountedCenter;

            if (player.channel)
            {
                if (Projectile.ai[0] < MaxCharge)
                    Projectile.ai[0]++;

                float charge = Projectile.ai[0] / MaxCharge;
                mp.ChargeRatio = charge;

                player.itemTime = 2;
                player.itemAnimation = 2;

                Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                player.ChangeDir(dir.X > 0 ? 1 : -1);
                player.itemRotation = (float)Math.Atan2(dir.Y * player.direction, dir.X * player.direction);

                if (Main.rand.NextBool(3))
                {
                    Vector2 pos = player.MountedCenter + dir * 28f + Main.rand.NextVector2Circular(8f, 8f);
                    float size = Main.rand.NextFloat(10f, 18f) * (0.6f + charge * 0.6f);
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        (dir * 0.4f + Main.rand.NextVector2Circular(0.4f, 0.4f)).ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                        new Color(100, 200, 255, 200) * (0.5f + charge * 0.5f),
                        Main.rand.Next(14, 24)
                    ));
                }

                Lighting.AddLight(player.MountedCenter, 0.2f + charge * 0.35f, 0.4f + charge * 0.4f, 0.6f + charge * 0.5f);

                if ((int)Projectile.ai[0] == (int)MaxCharge && Projectile.localAI[0] == 0f)
                {
                    Projectile.localAI[0] = 1f;
                    SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.5f, Pitch = 0.4f }, player.Center);
                }
                return;
            }

            Fire(player);
            mp.ChargeRatio = 0f;
            Projectile.Kill();
        }

        private void Fire(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
                return;

            if (!player.CheckMana(player.HeldItem.mana, true))
                return;

            float charge = MathHelper.Clamp(Projectile.ai[0] / MaxCharge, 0f, 1f);
            Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            Vector2 spawn = player.MountedCenter + dir * 16f;

            int count = 1;
            if (charge >= 0.45f) count = 2;
            if (charge >= 0.85f) count = 3;

            float speed = MathHelper.Lerp(7.5f, 11f, charge);
            int damage = (int)(Projectile.damage * MathHelper.Lerp(1f, 1.45f, charge));
            float kb = Projectile.knockBack * MathHelper.Lerp(1f, 1.3f, charge);

            for (int i = 0; i < count; i++)
            {
                float spread = count == 1 ? 0f : MathHelper.Lerp(-0.18f, 0.18f, i / (float)(count - 1));
                Vector2 vel = dir.RotatedBy(spread) * speed;
                int id = Projectile.NewProjectile(
                    player.GetSource_ItemUse(player.HeldItem),
                    spawn,
                    vel,
                    ModContent.ProjectileType<ColdFlareP>(),
                    damage,
                    kb,
                    player.whoAmI
                );
                if (id >= 0)
                    Main.projectile[id].scale = MathHelper.Lerp(1f, 1.2f, charge);
            }

            SoundEngine.PlaySound(SoundID.Item43 with { Volume = 0.85f, Pitch = -0.1f + charge * 0.35f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.45f, Pitch = 0.2f }, player.Center);
        }
    }

    public class ColdFlareP : ModProjectile
    {
        private Vector2 oldPos = Vector2.Zero;
        private readonly VertexStrip vertexStrip = new VertexStrip();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 18;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            oldPos = Projectile.Center;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 120, false);
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.direction * 0.22f;

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.2f);

            if (Main.rand.NextBool(2))
            {
                Vector2 vel = Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.6f, 0.6f);
                float size = Main.rand.NextFloat(12f, 22f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(100, 210, 255, 200) * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(16, 28)
                ));
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.55f, 0.75f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
                return false;
            }

            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;

            Projectile.velocity *= 0.85f;
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.15f }, Projectile.Center);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                float size = Main.rand.NextFloat(16f, 28f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(90, 200, 255, 210) * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(18, 32)
                ));
            }
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = 0.1f }, Projectile.Center);
        }

        public override Color? GetAlpha(Color lightColor) => new Color(160, 230, 255, 180);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            try
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                GameShaders.Misc["MagicMissile"].Apply(null);

                vertexStrip.PrepareStripWithProceduralPadding(
                    Projectile.oldPos,
                    Projectile.oldRot,
                    progress => Color.Lerp(new Color(100, 210, 255, 200), new Color(160, 235, 255, 60), progress),
                    progress => 42f * Projectile.scale * (1f - progress * 0.85f),
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
                        new Color(100, 210, 255, 0) * fade * 0.5f, Projectile.oldRot[k], origin, Projectile.scale * (1f - k * 0.03f), SpriteEffects.None, 0);
                }
            }

            if (oldPos != Vector2.Zero && oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                Color trailColor = new Color(80, 190, 255, 0) * 0.65f;
                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 4.2f;

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.85f, trailScaleY), SpriteEffects.None, 0f);

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor * 0.4f, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.4f, trailScaleY * 1.35f), SpriteEffects.None, 0f);
            }

            Color outline = new Color(100, 210, 255) * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.6f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outline, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, new Color(180, 235, 255, 200), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}