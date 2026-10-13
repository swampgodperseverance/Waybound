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
using Terraria.ModLoader.IO;
using Waybound.Common.ModSystems;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Ranged.Bows
{    //gonna balance later
    public class Aneurysm : BaseHoldoutBow
    {
        public bool WheelMode;

        public override int ProjectileType => ModContent.ProjectileType<AneurysmPHoldout>();
        public override int Damage => 19;   
        public override int UseTime => 20;
        public override int ShotCooldown => 46;
        public override float HoldoutDistance => 16f;
        public override float BaseOffset => 6f;
        public override int AmmoType => AmmoID.Arrow;
        public override int Rarity => ItemRarityID.Green;
        public override int Value => Item.buyPrice(0, 1, 50, 0);
        public override float KnockBack => 2.5f;
        public override SoundStyle UseSound => SoundID.Item5;
        public override SoundStyle ShotSound => SoundID.Item5;
        public override int DustType => DustID.Blood;
        public override bool ItemGlow => true;
        public override Color ItemGlowColor => new Color(235, 45, 90);

        private int CurrentHoldout => WheelMode
            ? ModContent.ProjectileType<AneurysmP2Holdout>()
            : ModContent.ProjectileType<AneurysmPHoldout>();

        public override void SaveData(TagCompound tag)
        {
            tag["wheel"] = WheelMode;
        }

        public override void LoadData(TagCompound tag)
        {
            WheelMode = tag.GetBool("wheel");
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<AneurysmPHoldout>()] < 1
                && player.ownedProjectileCounts[ModContent.ProjectileType<AneurysmP2Holdout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, CurrentHoldout, damage, knockback, player.whoAmI);
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ || player.whoAmI != Main.myPlayer)
                return;

            ModKeybind key = VanillaKeybinds.ChangeWeaponStyle;
            if (key == null || !key.JustPressed)
                return;

            WheelMode = !WheelMode;

            Color c = WheelMode ? new Color(175, 105, 245) : new Color(235, 45, 90);
            CombatText.NewText(player.getRect(), c, WheelMode ? "Wheel" : "Arrow");
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = WheelMode ? 0.3f : -0.3f, Volume = 0.7f }, player.Center);

            for (int i = 0; i < 16; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 5f);
                AneurysmHoldoutBase.Spark(player.Center, vel, c, Main.rand.NextFloat(8f, 13f), 20);
            }

            if (player.channel)
                SwapHoldout(player);
        }

        private void SwapHoldout(Player player)
        {
            int arrowType = ModContent.ProjectileType<AneurysmPHoldout>();
            int wheelType = ModContent.ProjectileType<AneurysmP2Holdout>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && (p.type == arrowType || p.type == wheelType))
                    p.Kill();
            }

            Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, dir, CurrentHoldout,
                player.GetWeaponDamage(Item), player.GetWeaponKnockback(Item), player.whoAmI);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string key = "None";
            if (VanillaKeybinds.ChangeWeaponStyle != null)
            {
                List<string> keys = VanillaKeybinds.ChangeWeaponStyle.GetAssignedKeys();
                if (keys.Count > 0)
                    key = keys[0];
            }

            tooltips.Add(new TooltipLine(Mod, "AneurysmMode", "Mode: " + (WheelMode ? "Bouncing Wheel" : "Arrow") + " [" + key + "]")
            {
                OverrideColor = WheelMode ? new Color(175, 105, 245) : new Color(235, 45, 90)
            });
        }
    }

    public abstract class AneurysmHoldoutBase : BaseHoldoutProjectile
    {
        protected static readonly Color Blood = new Color(235, 45, 90);
        protected static readonly Color Violet = new Color(175, 105, 245);

        protected abstract bool Wheel { get; }

        public override int ShotCooldown => 46;
        public override float HoldoutDistance => 16f;
        public override float BaseOffset => 6f;
        public override int DustType => DustID.Blood;
        public override SoundStyle ShotSound => SoundID.Item5;
        public override float ProjectileSpeed => 11f;
        public override float SpawnOffset => 30f;
        public override bool AimGlow => true;

        private Color Accent => Wheel ? Violet : Blood;

        internal static void Spark(Vector2 pos, Vector2 vel, Color color, float size, int life)
        {
            if (Main.dedServ)
                return;

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size),
                color,
                life));
        }

        protected override void ShotVisuals(Player player)
        {
            base.ShotVisuals(player);

            if (Main.dedServ)
                return;

            if (!Wheel && !player.HasAmmo(player.HeldItem))
                return;

            Vector2 muzzle = Projectile.Center + Projectile.velocity * SpawnOffset;

            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 6f);
                Spark(muzzle, vel, Main.rand.NextBool(3) ? Violet : Accent, Main.rand.NextFloat(8f, 14f), Main.rand.Next(14, 24));
            }

            for (int i = 0; i < 3; i++)
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), DustID.Blood,
                    Projectile.velocity.RotatedByRandom(1.2f) * Main.rand.NextFloat(1f, 3f), 100, default, 1.2f);
            }
        }

        protected override void FireShot(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
                return;

            if (!Wheel && !player.HasAmmo(player.HeldItem))
                return;

            Vector2 spawnPos = Projectile.Center + Projectile.velocity * SpawnOffset;
            if (Collision.SolidCollision(spawnPos, 4, 4))
                spawnPos = player.Center + Projectile.velocity * (SpawnOffset + 5f);

            int type = Wheel ? ModContent.ProjectileType<AneurysmP2>() : ProjectileType;
            float speed = Wheel ? 9f : ProjectileSpeed;
            int damage = Wheel ? (int)(Projectile.damage * 1.3f) : Projectile.damage;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Projectile.velocity * speed, type, damage, Projectile.knockBack, player.whoAmI);

            _recoilProgress = RecoilAmount;
            _targetHoldoutDistance = 2f;

            SoundEngine.PlaySound(ShotSound with { Pitch = Wheel ? -0.2f : 0.3f }, Projectile.Center);

            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(spawnPos - new Vector2(4f), 8, 8, DustID.Blood, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f), 0, default, 1.4f);
                d.noGravity = true;
                d.alpha = 80;
            }
        }

        protected virtual void DrawExtra(float charge, float beat, float time, Color add)
        {
        }

        public override bool PreDraw(ref Color lightColor)
        {
            base.PreDraw(ref lightColor);

            if (Projectile.Opacity < 0.01f)
                return false;

            float charge = ChargeProgress;
            float time = Main.GlobalTimeWrappedHourly;
            float beat = MathF.Pow(MathF.Max(0f, MathF.Sin(time * (4f + charge * 12f))), 4f);
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color add = new Color(Accent.R, Accent.G, Accent.B, 0);

            Texture2D star = TextureAssets.Extra[98].Value;
            float glow = (0.15f + 0.6f * charge) * (0.4f + 0.6f * beat) * Projectile.Opacity;
            Main.EntitySpriteDraw(star, position, null, add * glow, 0f, star.Size() / 2f, new Vector2(0.18f + 0.18f * beat), SpriteEffects.None, 0);

            DrawExtra(charge, beat, time, add);

            return false;
        }
    }

    public class AneurysmPHoldout : AneurysmHoldoutBase
    {
        protected override bool Wheel => false;

        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Bows/Aneurysm";
        public override int ProjectileType => ModContent.ProjectileType<AneurysmP>();
        public override Vector3 LightColor => new Vector3(0.4f, 0.08f, 0.14f);
        public override Color GlowColor => Blood;
    }

    public class AneurysmP2Holdout : AneurysmHoldoutBase
    {
        protected override bool Wheel => true;

        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Bows/Aneurysm2";
        public override int ProjectileType => ModContent.ProjectileType<AneurysmP2>();
        public override float ProjectileSpeed => 9f;
        public override Vector3 LightColor => new Vector3(0.3f, 0.12f, 0.45f);
        public override Color GlowColor => Violet;

        protected override void DrawExtra(float charge, float beat, float time, Color add)
        {
            if (charge <= 0.15f)
                return;

            int wheelType = ModContent.ProjectileType<AneurysmP2>();
            Main.instance.LoadProjectile(wheelType);
            Texture2D wheel = TextureAssets.Projectile[wheelType].Value;

            Vector2 muzzle = Projectile.Center + Projectile.velocity * SpawnOffset * 0.9f - Main.screenPosition;
            float s = 0.65f * MathHelper.Clamp(charge * 1.4f, 0f, 1f);
            float spin = time * (4f + charge * 10f);

            Main.EntitySpriteDraw(wheel, muzzle, null, add * ((0.45f + 0.4f * beat) * Projectile.Opacity), spin, wheel.Size() / 2f, s * (1f + 0.08f * beat), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(wheel, muzzle, null, add * (0.25f * Projectile.Opacity), -spin * 0.6f, wheel.Size() / 2f, s * 1.15f, SpriteEffects.None, 0);
        }
    }

    public class AneurysmP : ModProjectile
    {
        private static readonly Color Blood = new Color(235, 45, 90);


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 360;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.localAI[0] > 24f)
                Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.07f, 16f);

            if (Main.rand.NextBool(3))
                AneurysmHoldoutBase.Spark(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.6f, 0.6f),
                    Blood, Main.rand.NextFloat(6f, 10f), 14);

            if (Main.rand.NextBool(8))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Vector2.Zero, 100, default, 1f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.35f, 0.06f, 0.12f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.HasBuff(BuffID.Bleeding))
                modifiers.FinalDamage *= 1.2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Bleeding, 240);

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.9f) * Main.rand.NextFloat(2f, 6f);
                AneurysmHoldoutBase.Spark(target.Center + Main.rand.NextVector2Circular(8f, 8f), vel, Blood, Main.rand.NextFloat(8f, 14f), Main.rand.Next(14, 24));
            }
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = -0.2f }, Projectile.Center);

            for (int i = 0; i < 6; i++)
                AneurysmHoldoutBase.Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), Blood, Main.rand.NextFloat(8f, 12f), 18);

            for (int i = 0; i < 6; i++)
                Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Color glow = new Color(Blood.R, Blood.G, Blood.B, 0);
            int len = Projectile.oldPos.Length;

            for (int k = 1; k < len; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;

                float f = 1f - k / (float)len;
                Main.EntitySpriteDraw(tex, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null, glow * (f * 0.5f),
                    Projectile.oldRot[k], origin, Projectile.scale * (0.6f + 0.4f * f), SpriteEffects.None, 0);
            }

            Vector2 pos = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(1.6f, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Main.EntitySpriteDraw(tex, pos + off, null, glow * 0.6f, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, pos, null, Color.Lerp(lightColor, Color.White, 0.5f), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class AneurysmP2 : ModProjectile
    {
        private const int MaxBounces = 6;

        private static readonly Color Blood = new Color(235, 45, 90);
        private static readonly Color Violet = new Color(150, 58, 225);

        public override string Texture => "Waybound/Content/NPCs/Bosses/Viscula/VisculaWheel";

        private int Bounces => (int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 420;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;

            if (Projectile.localAI[0] > 10f)
                Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.22f, 15f);

            Projectile.rotation += 0.28f * (Projectile.velocity.X >= 0f ? 1f : -1f);

            if (Main.rand.NextBool(3))
                AneurysmHoldoutBase.Spark(Projectile.Center + Main.rand.NextVector2CircularEdge(14f, 14f), -Projectile.velocity * 0.05f, Violet, Main.rand.NextFloat(7f, 11f), 14);

            if (Main.rand.NextBool(10))
                Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Main.rand.NextVector2Circular(1f, 1f), 100, default, 1f);

            Lighting.AddLight(Projectile.Center, new Vector3(0.55f, 0.2f, 0.8f) * 0.6f);
        }

        private void BounceFx(Vector2 pos, int count, float speed)
        {
            if (Main.dedServ)
                return;

            float off = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = (off + MathHelper.TwoPi * i / count).ToRotationVector2();
                AneurysmHoldoutBase.Spark(pos, dir * speed, i % 2 == 0 ? Violet : Blood, Main.rand.NextFloat(9f, 14f), 20);
            }

            for (int i = 0; i < 4; i++)
                Dust.NewDustPerfect(pos, DustID.Blood, Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.2f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 1f + 0.12f * Bounces;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Bleeding, 240);
            BounceFx(target.Center, 8, 4f);
            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.4f, Pitch = 0.2f }, Projectile.Center);

            if (Projectile.owner != Main.myPlayer)
                return;

            NPC next = null;
            float best = 520f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.whoAmI == target.whoAmI || !n.CanBeChasedBy(this))
                    continue;

                float d = Vector2.Distance(n.Center, Projectile.Center);
                if (d < best)
                {
                    best = d;
                    next = n;
                }
            }

            float speed = Math.Max(Projectile.velocity.Length(), 9f);
            if (next != null)
                Projectile.velocity = (next.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * speed;
            else
                Projectile.velocity = new Vector2(-Projectile.velocity.X * 0.8f, -Math.Abs(Projectile.velocity.Y) - 3f);

            Projectile.ai[0]++;
            Projectile.netUpdate = true;

            if (Bounces > MaxBounces)
                Projectile.Kill();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0]++;
            if (Bounces > MaxBounces)
                return true;

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.92f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.88f;

            if (Projectile.velocity.Y < 0f && Projectile.velocity.Y > -5f)
                Projectile.velocity.Y = -5f;

            BounceFx(Projectile.Center, 10 + Bounces, 3.5f);
            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.45f, Pitch = -0.1f + Bounces * 0.06f }, Projectile.Center);

            if (Projectile.owner == Main.myPlayer)
                Projectile.netUpdate = true;

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            BounceFx(Projectile.Center, 14, 5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            float scale = Projectile.scale * 0.65f * MathHelper.Clamp(Projectile.localAI[0] / 8f, 0.3f, 1f);
            float fade = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);
            Color add = new Color(Violet.R, Violet.G, Violet.B, 0);
            int len = Projectile.oldPos.Length;

            for (int k = 1; k < len; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;

                float f = 1f - k / (float)len;
                Main.EntitySpriteDraw(tex, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null, add * (f * 0.35f * fade),
                    Projectile.oldRot[k], origin, scale * (0.7f + 0.3f * f), SpriteEffects.None, 0);
            }

            Vector2 pos = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < 8; i++)
            {
                Vector2 off = new Vector2(2f, 0f).RotatedBy(MathHelper.PiOver4 * i);
                Main.EntitySpriteDraw(tex, pos + off, null, add * (0.35f * fade), Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, pos, null, new Color(Blood.R, Blood.G, Blood.B, 0) * (0.2f * fade), -Projectile.rotation * 0.6f, origin, scale * 1.15f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, pos, null, Color.Lerp(lightColor, Color.White, 0.7f) * fade, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
}