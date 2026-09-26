using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles;
using Waybound.Helpers;

namespace Waybound.Content.Items.Weapons.Summon.Sentry
{
    public class RemoteControllerOfDesertHunter : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.QueenSpiderStaff);
            Item.damage = 21;
            Item.mana = 10;
            Item.width = 40;
            Item.height = 40;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.knockBack = 2.5f;
            Item.UseSound = SoundID.Item52;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.shoot = ModContent.ProjectileType<RemoteControllerOfDesertHunterP>();
            Item.shootSpeed = 0f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.statMana < 40)
                    return false;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<RemoteControllerOfDesertHunterP>()] < 1)
                    return false;
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.UseSound = SoundID.Item11;
                return true;
            }

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item52;

            Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
            if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileCut[tile.TileType])
                return false;
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                if (!player.CheckMana(40, true))
                    return false;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<RemoteControllerOfDesertHunterP>())
                    {
                        if (p.ModProjectile is RemoteControllerOfDesertHunterP turret)
                            turret.FireManualRocket(player);
                        break;
                    }
                }
                return false;
            }

            player.FindSentryRestingSpot(type, out int worldX, out int worldY, out int pushYUp);
            var projectile = Projectile.NewProjectileDirect(source, new Vector2(worldX, worldY - pushYUp), velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;
            player.UpdateMaxTurrets();
            return false;
        }
    }

    public class RemoteControllerOfDesertHunterP : ModProjectile
    {
        public NPC target;
        private float firePulse;
        private float manualPulse;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 52;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity = new Vector2(0, 12);
            base.OnSpawn(source);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public void FireManualRocket(Player owner)
        {
            Vector2 pos = Projectile.Center + new Vector2(0, -10);
            Vector2 aim = (Main.MouseWorld - pos).SafeNormalize(Vector2.UnitX);
            Vector2 vel = aim.RotatedByRandom(MathHelper.ToRadians(8f)) * Main.rand.NextFloat(6f, 8f);
            pos += aim * 12f;

            int id = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                pos,
                vel,
                ModContent.ProjectileType<RemoteControllerOfDesertHunterP1>(),
                Projectile.damage,
                3f,
                owner.whoAmI
            );

            if (id >= 0)
            {
                NPC near = null;
                if (WayboundHelper.ClosestNPC(ref near, pos, 750, 0, true) && near != null)
                    Main.projectile[id].ai[0] = near.whoAmI;
                else
                    Main.projectile[id].ai[0] = -1;
            }

            firePulse = 1f;
            manualPulse = 1f;
            SoundEngine.PlaySound(SoundID.Item11, Projectile.Center);
            Lighting.AddLight(pos, 1.5f, 0.5f, 0.3f);
            owner.PlayerScreen().fastScreenShake = 2.5f * (1000 - Vector2.Distance(owner.Center, Projectile.Center)) / 1000;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (firePulse > 0f)
                firePulse = MathHelper.Lerp(firePulse, 0f, 0.12f);
            if (manualPulse > 0f)
                manualPulse = MathHelper.Lerp(manualPulse, 0f, 0.08f);

            float aimRot;
            bool hasTarget = WayboundHelper.ClosestNPC(ref target, Projectile.Center, 750, 0, true) && target != null && target.active;

            if (hasTarget)
            {
                aimRot = (target.Center - Projectile.Center).ToRotation();
            }
            else
            {
                aimRot = (Main.MouseWorld - Projectile.Center).ToRotation();
            }

            if (aimRot < 0)
                aimRot += MathHelper.TwoPi;

            if (Projectile.ai[1] < MathHelper.Pi / 2 && aimRot > MathHelper.TwoPi / 3)
                Projectile.ai[1] += MathHelper.TwoPi;
            else if (aimRot < MathHelper.Pi / 2 && Projectile.ai[1] > MathHelper.TwoPi / 3)
                Projectile.ai[1] -= MathHelper.TwoPi;

            Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], aimRot, 0.1f);

            if (hasTarget)
            {
                if (Math.Abs(Projectile.ai[1] - aimRot) < 10)
                {
                    Projectile.ai[0]++;
                    if (Projectile.ai[0] > 60)
                    {
                        Vector2 pos = Projectile.Center + new Vector2(0, -10);
                        Vector2 vel = (target.Center - pos).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(5f, 7f);
                        pos += vel.SafeNormalize(Vector2.UnitX) * 12f;
                        int proj1 = Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, vel, ModContent.ProjectileType<RemoteControllerOfDesertHunterP1>(), Projectile.damage / 2, 3, Main.myPlayer);
                        Main.projectile[proj1].ai[0] = target.whoAmI;
                        owner.PlayerScreen().fastScreenShake = 3.5f * (1000 - Vector2.Distance(owner.Center, Projectile.Center)) / 1000;
                        Lighting.AddLight(pos, 1.5f, 0.75f, 0.5f);
                        SoundEngine.PlaySound(SoundID.Item11, Projectile.Center);
                        firePulse = 1f;
                        Projectile.ai[0] = 0;
                    }
                }
            }
            else
            {
                Projectile.ai[0] = 0;
            }

            if (owner.dead || !owner.active)
                Projectile.Kill();

            Lighting.AddLight(Projectile.Center, 1.5f, 0.75f, 0.5f);
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanHitNPC(NPC target) => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            var effects = (Projectile.ai[1] > MathHelper.Pi / 2 && Projectile.ai[1] < MathHelper.Pi * 3 / 2) ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Rectangle rect1 = new(0, 0, 50, 22);
            Rectangle rect2 = new(8, 24, 34, 30);
            Vector2 drawOrigin1 = new Vector2(rect1.Width * 0.5f, rect1.Height * 0.5f);
            Vector2 drawOrigin2 = new Vector2(rect2.Width * 0.5f, rect2.Height * 0.5f);
            Vector2 pos1 = Projectile.Center - Main.screenPosition + new Vector2(0, -10);
            Vector2 pos2 = Projectile.Center - Main.screenPosition + new Vector2(0, 10);

            float pulse = Math.Max(firePulse, manualPulse);
            Color baseColor = Projectile.GetAlpha(lightColor);
            Color gunColor = baseColor;

            if (pulse > 0.05f)
            {
                gunColor = Color.Lerp(baseColor, new Color(255, 50, 30), pulse * 0.85f);
                Color glow = new Color(255, 40, 25, 0) * pulse * 0.7f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(2.5f * pulse, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                    Main.EntitySpriteDraw(texture, pos1 + offset, rect1, glow, Projectile.ai[1], drawOrigin1, Projectile.scale * (1f + pulse * 0.08f), effects, 0);
                }
            }

            if (manualPulse > 0.05f)
            {
                Color bodyGlow = new Color(255, 60, 30, 0) * manualPulse * 0.5f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(3f * manualPulse, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                    Main.EntitySpriteDraw(texture, pos2 + offset, rect2, bodyGlow, Projectile.rotation, drawOrigin2, Projectile.scale, SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(texture, pos2, rect2, baseColor, Projectile.rotation, drawOrigin2, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, pos1, rect1, gunColor, Projectile.ai[1], drawOrigin1, Projectile.scale, effects, 0);
            return false;
        }
    }

    public class RemoteControllerOfDesertHunterP1 : ModProjectile
    {
        public override string Texture => "Waybound/Content/NPCs/Bosses/Themis/ThemisRocket";
        public NPC target;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            DrawOriginOffsetY = -3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.localAI[1] = MathHelper.ToRadians(Main.rand.NextFloat(-90, 90));
            base.OnSpawn(source);
        }

        public override void AI()
        {
            int idx = (int)Projectile.ai[0];
            bool hasTarget = idx >= 0 && idx < Main.maxNPCs && Main.npc[idx].active;
            if (hasTarget)
                target = Main.npc[idx];

            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
            Projectile.localAI[0]++;

            if (Projectile.localAI[0] < 10)
            {
                Projectile.velocity = (Projectile.velocity + (Projectile.velocity.RotatedBy(Projectile.localAI[1])).SafeNormalize(Vector2.UnitX) * 1f).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
            }
            else if (Projectile.localAI[0] < 35 && hasTarget)
            {
                Projectile.velocity = (Projectile.velocity + (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 1.35f).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
            }

            Projectile.velocity *= 1.015f;

            Vector2 pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4, 6);
            Dust dust = Dust.NewDustPerfect(pos, DustID.Smoke, Vector2.Zero, Scale: Main.rand.NextFloat(1.25f, 1.75f));
            dust.noGravity = true;
            pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4, 6);
            dust = Dust.NewDustPerfect(pos, DustID.InfernoFork, Vector2.Zero, 0, new Color(255, 100, 50), Main.rand.NextFloat(1.25f, 1.75f));
            dust.noGravity = true;
            Lighting.AddLight(Projectile.position, 1.5f, 0.75f, 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<DesertExplosion>(), Projectile.damage * 2, 4, Main.myPlayer);
            }
        }
    }
}