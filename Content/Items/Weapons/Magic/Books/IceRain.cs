using System;
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

using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Magic.Books
{
    // weapon for the game start
    public class IceRain : ModItem
    {
        public const int ManaPerIcicle = 6;

        public override void SetDefaults()
        {
            Item.damage = 7;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 0; 
            Item.knockBack = 2.5f;
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 80);
            Item.shoot = ModContent.ProjectileType<IceRainHold>();
            Item.shootSpeed = 0f;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }

    }
     // i mean the mana system could be easier
    public class IceRainHold : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Magic/Books/IceRainIcicle";

        private const int MaxIcicles = 3;
        private const int ChargeInterval = 28;
        private const int FireInterval = 6;

        private readonly int[] slots = { -1, -1, -1 };
        private int count;
        private int chargeTimer;
        private bool firing;
        private int fireIndex;
        private int fireTimer;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override bool? CanDamage() => false;
        public override bool ShouldUpdatePosition() => false;
        public override bool PreDraw(ref Color lightColor) => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;
            Projectile.timeLeft = 2;

            if (Projectile.owner != Main.myPlayer)
                return;

            bool holdingBook = player.HeldItem.type == ModContent.ItemType<IceRain>();
            if (!player.active || player.dead || player.CCed || player.noItems || !holdingBook)
            {
                Projectile.Kill();
                return;
            }

            Vector2 aimDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);

            if (!firing)
            {
                player.ChangeDir(aimDir.X >= 0f ? 1 : -1);
                int dir = player.direction;
                player.itemRotation = (float)Math.Atan2(aimDir.Y * dir, aimDir.X * dir);
                player.itemAnimation = player.itemAnimationMax;
                player.itemTime = player.itemTimeMax;

                if (player.channel)
                {
                    if (count < MaxIcicles)
                    {
                        if (chargeTimer > 0)
                            chargeTimer--;
                        else
                            TrySpawnIcicle(player);
                    }
                }
                else
                {
                    if (count == 0)
                    {
                        Projectile.Kill();
                        return;
                    }
                    firing = true;
                    fireIndex = 0;
                    fireTimer = 0;
                }
            }
            else
            {
                fireTimer--;
                if (fireTimer <= 0)
                {
                    if (fireIndex < count)
                    {
                        FireIcicle(player, fireIndex, aimDir);
                        fireIndex++;
                        fireTimer = FireInterval;
                    }
                    else
                    {
                        Projectile.Kill();
                    }
                }
            }
        }

        private void TrySpawnIcicle(Player player)
        {
            int cost = Math.Max(1, (int)(IceRain.ManaPerIcicle * player.manaCost));
            if (!player.CheckMana(cost, true))
                return;

            player.manaRegenDelay = (int)player.maxRegenDelay;

            int slot = count;
            Vector2 pos = IceRainIcicle.SlotPosition(player, slot);

            int idx = Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, Vector2.Zero,
                ModContent.ProjectileType<IceRainIcicle>(), Projectile.damage, Projectile.knockBack,
                player.whoAmI, 0f, slot);

            if (idx >= 0 && idx < Main.maxProjectiles)
                slots[slot] = idx;

            count++;
            chargeTimer = ChargeInterval;
            SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.6f, Pitch = -0.3f + 0.25f * slot }, pos);
        }

        private void FireIcicle(Player player, int i, Vector2 fallbackDir)
        {
            Projectile p = GetIcicle(slots[i]);
            if (p == null || p.ai[0] != 0f)
                return;

            Vector2 dir = (Main.MouseWorld - p.Center).SafeNormalize(fallbackDir);
            int stage = p.frame;

            p.velocity = dir * (14f + 2f * stage);
            p.ai[0] = 1f;
            p.timeLeft = 150;
            p.penetrate = stage == 2 ? 2 : 1;
            p.netUpdate = true;

            SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.7f, Pitch = -0.1f + 0.15f * stage }, p.Center);
            for (int d = 0; d < 8; d++)
            {
                Vector2 vel = dir.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 6f);
                float scale = Main.rand.NextFloat(0.7f, 1.3f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    p.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    new Color(150, 215, 255, 200),
                    Main.rand.Next(14, 24)
                ));
            }
        }

        private static Projectile GetIcicle(int idx)
        {
            if (idx < 0 || idx >= Main.maxProjectiles)
                return null;
            Projectile p = Main.projectile[idx];
            if (!p.active || p.type != ModContent.ProjectileType<IceRainIcicle>())
                return null;
            return p;
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            for (int i = 0; i < slots.Length; i++)
            {
                Projectile p = GetIcicle(slots[i]);
                if (p != null && p.owner == Projectile.owner && p.ai[0] == 0f)
                    p.Kill();
            }
        }
    }

    public class IceRainIcicle : ModProjectile
    {
        private const int Stage1Time = 26;
        private const int Stage2Time = 62;
        private static readonly int[] Sizes = { 10, 14, 18 };

        private static readonly Vector2[] SlotOffsets =
        {
            new Vector2(0f, -62f),
            new Vector2(-30f, -50f),
            new Vector2(30f, -50f),
        };

        private int State => (int)Projectile.ai[0];

        public static Vector2 SlotPosition(Player player, int slot)
        {
            slot = Math.Clamp(slot, 0, SlotOffsets.Length - 1);
            float bob = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + slot * 2.1f) * 3f;
            Vector2 off = SlotOffsets[slot];
            return player.Center + new Vector2(off.X, (off.Y + bob) * player.gravDir);
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Sizes[0];
            Projectile.height = Sizes[0];
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override bool? CanDamage() => State == 0 ? false : null;
        public override bool ShouldUpdatePosition() => State != 0;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.localAI[0]++;

            switch (State)
            {
                case 0: ChargingAI(player); break;
                case 1: FlightAI(); break;
                default: ShardAI(); break;
            }

            float k = 0.1f + Projectile.frame * 0.05f;
            Lighting.AddLight(Projectile.Center, k, 0.3f + k, 0.5f + k * 2f);
        }

        private void ChargingAI(Player player)
        {
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            float timer = Projectile.localAI[0];
            Projectile.timeLeft = 300;

            Vector2 target = SlotPosition(player, (int)Projectile.ai[1]);
            Projectile.Center = timer <= 1f ? target : Vector2.Lerp(Projectile.Center, target, 0.35f);

            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitY);
                if (timer % 6f == 0f)
                    Projectile.netUpdate = true;
            }

            float targetRot = Projectile.velocity.SafeNormalize(Vector2.UnitY).ToRotation() - MathHelper.PiOver2;
            Projectile.rotation = timer <= 1f ? targetRot : Utils.AngleLerp(Projectile.rotation, targetRot, 0.3f);

            int stage = timer < Stage1Time ? 0 : timer < Stage2Time ? 1 : 2;
            if (stage != Projectile.frame)
            {
                Projectile.frame = stage;
                Projectile.Resize(Sizes[stage], Sizes[stage]);
                Projectile.localAI[1] = timer;
                for (int d = 0; d < 8 + stage * 4; d++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    float scale = 1.0f + stage * 0.25f;
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SysVector2(scale),
                        new Color(150, 215, 255, 200),
                        Main.rand.Next(14, 24)
                    ));
                }
            }

            float appear = MathHelper.SmoothStep(0f, 1f, Math.Min(1f, timer / 12f));
            float pop = Projectile.localAI[1] > 0f
                ? 1f + 0.3f * (1f - MathHelper.Clamp((timer - Projectile.localAI[1]) / 10f, 0f, 1f))
                : 1f;
            Projectile.scale = appear * pop;

            if (timer == 1f)
            {
                for (int d = 0; d < 6; d++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                    float scale = Main.rand.NextFloat(0.8f, 1.3f);
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        (Projectile.Center + offset).ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SysVector2(scale),
                        new Color(150, 215, 255, 200),
                        Main.rand.Next(14, 24)
                    ));
                }
            }

            if (Main.rand.NextBool(7 - stage * 2))
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2CircularEdge(14f + stage * 3f, 14f + stage * 3f);
                Vector2 vel = (Projectile.Center - pos) * 0.04f;
                float scale = Main.rand.NextFloat(0.5f, 0.9f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    new Color(150, 215, 255, 200),
                    Main.rand.Next(14, 24)
                ));
            }
        }

        private void FlightAI()
        {
            if (Projectile.velocity.Length() < 20f)
                Projectile.velocity *= 1.02f;

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (Main.rand.NextBool(2))
            {
                Vector2 pos = Projectile.Center - Projectile.velocity * 0.5f + Main.rand.NextVector2Circular(4f, 4f);
                Vector2 vel = Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                float scale = Main.rand.NextFloat(0.6f, 1.0f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    new Color(150, 215, 255, 200),
                    Main.rand.Next(14, 24)
                ));
            }
        }

        private void ShardAI()
        {
            if (Projectile.localAI[0] == 1f)
            {
                Projectile.Resize(8, 8);
                Projectile.scale = 0.75f;
            }

            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.35f, 14f);
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (Main.rand.NextBool(4))
            {
                Vector2 vel = Main.rand.NextVector2Circular(0.4f, 0.4f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(0.6f),
                    new Color(150, 215, 255, 200),
                    Main.rand.Next(14, 24)
                ));
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (State == 1)
                modifiers.SourceDamage *= 1f + 0.45f * Projectile.frame;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (State == 1 && Main.rand.NextBool(3))
                target.AddBuff(BuffID.Frostburn, 60 + 60 * Projectile.frame);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => true;

        public override void OnKill(int timeLeft)
        {
            bool wasCharging = State == 0;
            if (!wasCharging)
                SoundEngine.PlaySound(SoundID.Item27, Projectile.position);

            int count = State == 2 ? 4 : 8 + Projectile.frame * 4;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                float scale = Main.rand.NextFloat(0.8f, 1.5f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    new Color(150, 215, 255, 200),
                    Main.rand.Next(14, 24)
                ));
            }
            if (State == 1 && Projectile.frame == 2 && Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-6f, -2.5f));
                    int idx = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel,
                        Type, Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner, 2f, 0f);
                    if (idx >= 0 && idx < Main.maxProjectiles)
                        Main.projectile[idx].timeLeft = 45;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, 3, 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float t = Main.GlobalTimeWrappedHourly;

            if (State == 0)
            {
                Main.EntitySpriteDraw(tex, pos, frame, Color.Lerp(lightColor, Color.White, 0.7f),
                    Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
                float outline = 0.15f + 0.12f * Projectile.frame + 0.08f * (float)Math.Sin(t * 8f);
                Main.EntitySpriteDraw(tex, pos, frame, new Color(150, 220, 255, 0) * outline,
                    Projectile.rotation, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0);

                return false;
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float k = 1f - i / (float)Projectile.oldPos.Length;
                Vector2 p = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(tex, p, frame, new Color(120, 200, 255, 0) * (0.4f * k),
                    Projectile.oldRot[i], origin, Projectile.scale * (0.8f + 0.2f * k), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, pos, frame, Color.Lerp(lightColor, Color.White, 0.6f),
                Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}