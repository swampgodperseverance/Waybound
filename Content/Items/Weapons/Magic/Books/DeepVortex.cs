using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using SysVec2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Magic.Books
{
    public class DeepVortex : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 30;
            Item.height = 30;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(0, 1, 20, 0);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void HoldItem(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
                return;

            int type = ModContent.ProjectileType<DeepVortexHeld>();
            if (player.ownedProjectileCounts[type] == 0)
            {
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter,
                    Vector2.Zero, type, 0, 0f, player.whoAmI);
            }
        }

        public class DeepVortexHeld : ModProjectile
        {
            public override string Texture => "Waybound/Content/Items/Weapons/Magic/Books/DeepVortexHeld";
            private const string SpikeTexturePath = "Waybound/Content/Items/Weapons/Magic/Books/DeepVortexSpike";

            private const int ShardCount = 5;
            private const float IdleLength = 16f;
            private const float FullLength = 36f;
            private const float ShardSpeed = 15f;

            private const float SpikeForward = MathHelper.PiOver4;
            private const float SpikeOriginFraction = 0.1f;

            private static readonly float[] AngleOffsets = { -0.6f, -0.3f, 0f, 0.3f, 0.6f };
            private static readonly float[] LengthMul = { 0.8f, 0.95f, 1.1f, 0.95f, 0.8f };
            private static readonly float[] LaunchAt = { 0.40f, 0.48f, 0.56f, 0.64f, 0.72f };

            private bool init;
            private int age;
            private int launchedMask;
            private float lastProgress;
            private float progress;
            private bool casting;
            private float displayAngle;

            public override void SetDefaults()
            {
                Projectile.width = 30;
                Projectile.height = 30;
                Projectile.friendly = false;
                Projectile.hostile = false;
                Projectile.penetrate = -1;
                Projectile.tileCollide = false;
                Projectile.ignoreWater = true;
                Projectile.aiStyle = -1;
                Projectile.timeLeft = 2;
            }

            public override bool? CanDamage() => false;
            public override bool ShouldUpdatePosition() => false;

            private static float IdleAngle(Player player) => new Vector2(player.direction, -1.1f).ToRotation();

            public override void AI()
            {
                Player player = Main.player[Projectile.owner];

                if (!player.active || player.dead || player.noItems || player.CCed
                    || player.HeldItem.type != ModContent.ItemType<DeepVortex>())
                {
                    Projectile.Kill();
                    return;
                }

                if (!init)
                {
                    init = true;

                    foreach (Projectile other in Main.ActiveProjectiles)
                    {
                        if (other.whoAmI < Projectile.whoAmI && other.owner == Projectile.owner && other.type == Type)
                        {
                            Projectile.Kill();
                            return;
                        }
                    }

                    displayAngle = IdleAngle(player);
                    Projectile.Center = player.MountedCenter + displayAngle.ToRotationVector2() * 16f;
                }

                Projectile.timeLeft = 2;
                age = Math.Min(age + 1, 60);

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 aim = (Main.MouseWorld - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0f));
                    if (Vector2.DistanceSquared(aim, Projectile.velocity) > 0.0004f)
                    {
                        Projectile.velocity = aim;
                        Projectile.netUpdate = true;
                    }
                }

                casting = player.itemAnimation > 0 && player.itemAnimationMax > 0;
                progress = casting ? 1f - player.itemAnimation / (float)player.itemAnimationMax : 0f;
                if (progress < lastProgress)
                    launchedMask = 0;
                lastProgress = progress;

                float targetAngle = casting && Projectile.velocity != Vector2.Zero
                    ? Projectile.velocity.ToRotation()
                    : IdleAngle(player);
                displayAngle = displayAngle.AngleLerp(targetAngle, 0.3f);
                Vector2 dir = displayAngle.ToRotationVector2();

                float bob = MathF.Sin(Main.GlobalTimeWrappedHourly * 3f) * 2f;
                Vector2 bookTarget = player.MountedCenter + dir * 16f + new Vector2(0f, bob + player.gfxOffY);
                Projectile.Center = Vector2.Lerp(Projectile.Center, bookTarget, 0.5f);
                Projectile.spriteDirection = player.direction;
                Projectile.rotation = player.direction * (0.12f + 0.15f * MathF.Sin(MathF.PI * progress))
                    + MathF.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.04f;

                player.heldProj = Projectile.whoAmI;
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, displayAngle - MathHelper.PiOver2);
                player.itemRotation = MathF.Atan2(dir.Y * player.direction, dir.X * player.direction);

                if (casting)
                {
                    for (int i = 0; i < ShardCount; i++)
                    {
                        int bit = 1 << i;
                        if ((launchedMask & bit) != 0 || progress < LaunchAt[i])
                            continue;

                        launchedMask |= bit;
                        LaunchFx(i);
                        if (Main.myPlayer == Projectile.owner)
                            Fire(player, i);
                    }
                }

                SpawnIdleAndChargeFx();

                float charge = casting ? DeepVortexFx.Smooth(progress / LaunchAt[ShardCount - 1]) : 0f;
                Lighting.AddLight(Projectile.Center, 0.2f + 0.35f * charge, 0.3f + 0.4f * charge, 0.45f + 0.45f * charge);
            }

            private float ShardLength(int i)
            {
                float appear = DeepVortexFx.Smooth(age / 18f);
                float wobble = 1f + 0.07f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.6f + i * 1.7f);
                float idle = IdleLength * LengthMul[i] * wobble;

                if (!casting)
                    return idle * appear;

                float full = FullLength * LengthMul[i];
                float launch = LaunchAt[i];

                float len = progress < launch
                    ? MathHelper.Lerp(idle, full, DeepVortexFx.Smooth(progress / launch))
                    : idle * DeepVortexFx.Smooth((progress - launch) / (1f - launch));

                return len * appear;
            }

            private Vector2 ShardDirection(int i) => (displayAngle + AngleOffsets[i]).ToRotationVector2();

            private void Fire(Player player, int i)
            {
                Vector2 dir = ShardDirection(i);
                Vector2 pos = Projectile.Center + dir * (FullLength * LengthMul[i]);
                Item item = player.HeldItem;

                Projectile.NewProjectile(
                    player.GetSource_ItemUse(item),
                    pos,
                    dir * ShardSpeed,
                    ModContent.ProjectileType<DeepVortexProj>(),
                    player.GetWeaponDamage(item),
                    player.GetWeaponKnockback(item, item.knockBack),
                    player.whoAmI);
            }

            private void LaunchFx(int i)
            {
                if (Main.dedServ)
                    return;

                Vector2 pos = Projectile.Center + ShardDirection(i) * (FullLength * LengthMul[i]);
                DeepVortexFx.Burst(pos, 7, 1.5f, 4.5f, 8f, 16f);

                SoundEngine.PlaySound(SoundID.Item27 with
                {
                    Volume = 0.35f,
                    Pitch = -0.2f + i * 0.1f,
                    PitchVariance = 0.05f
                }, pos);
            }

            private void SpawnIdleAndChargeFx()
            {
                if (Main.dedServ)
                    return;

                float charge = casting ? DeepVortexFx.Smooth(progress / LaunchAt[ShardCount - 1]) : 0f;
                float chance = casting ? 0.35f + 0.65f * charge : 0.07f;

                if (Main.rand.NextFloat() >= chance)
                    return;

                int i = Main.rand.Next(ShardCount);
                if ((launchedMask & (1 << i)) != 0)
                    return;

                Vector2 dir = ShardDirection(i);
                Vector2 pos = Projectile.Center + dir * ShardLength(i) * Main.rand.NextFloat(0.6f, 1f);
                float size = Main.rand.NextFloat(6f, 12f) * (casting ? 1.2f : 0.8f);

                DeepVortexFx.Shard(pos,
                    dir * Main.rand.NextFloat(0.2f, 1.2f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    size, Main.rand.Next(18, 30));
            }

            public override bool PreDraw(ref Color lightColor)
            {
                if (Main.dedServ)
                    return false;

                Texture2D book = TextureAssets.Projectile[Type].Value;
                Texture2D spike = ModContent.Request<Texture2D>(SpikeTexturePath).Value;

                Vector2 basePos = Projectile.Center - Main.screenPosition;
                float diag = MathF.Sqrt(spike.Width * spike.Width + spike.Height * spike.Height);
                Vector2 spikeOrigin = new Vector2(spike.Width, spike.Height) * SpikeOriginFraction;
                float charge = casting ? DeepVortexFx.Smooth(progress / LaunchAt[ShardCount - 1]) : 0f;

                Color body = Color.Lerp(Color.Lerp(lightColor, Color.White, 0.55f), DeepVortexFx.Glow with { A = 255 }, 0.25f);

                for (int i = 0; i < ShardCount; i++)
                {
                    if (casting && progress >= LaunchAt[i] && progress < LaunchAt[i] + 0.02f)
                        continue;

                    float len = ShardLength(i);
                    if (len < 2f)
                        continue;

                    float rot = displayAngle + AngleOffsets[i] - SpikeForward;
                    float scale = len / diag;
                    float glow = 0.25f + 0.55f * charge;

                    Main.EntitySpriteDraw(spike, basePos, null, DeepVortexFx.Glow * glow, rot, spikeOrigin,
                        scale * 1.12f, SpriteEffects.None, 0f);
                    Main.EntitySpriteDraw(spike, basePos, null, body, rot, spikeOrigin,
                        scale, SpriteEffects.None, 0f);
                }

                SpriteEffects fx = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                Vector2 bookOrigin = book.Size() * 0.5f;

                if (casting)
                {
                    Color aura = DeepVortexFx.Glow * (0.5f * MathF.Sin(MathF.PI * progress));
                    for (int k = 1; k <= 2; k++)
                    {
                        Main.EntitySpriteDraw(book, basePos, null, aura * (1f - k / 3f), Projectile.rotation, bookOrigin,
                            Projectile.scale * (1f + k * 0.06f), fx, 0f);
                    }
                }

                Main.EntitySpriteDraw(book, basePos, null, Color.Lerp(lightColor, Color.White, 0.25f),
                    Projectile.rotation, bookOrigin, Projectile.scale, fx, 0f);

                return false;
            }

            public override void DrawBehind(int index, System.Collections.Generic.List<int> behindNPCsAndTiles,
                System.Collections.Generic.List<int> behindNPCs, System.Collections.Generic.List<int> behindProjectiles,
                System.Collections.Generic.List<int> overPlayers, System.Collections.Generic.List<int> overWiresUI)
            {
                overPlayers.Add(index);
            }
        }

        public class DeepVortexProj : ModProjectile
        {
            public override string Texture => "Waybound/Content/Items/Weapons/Magic/Books/DeepVortexSpike";

            private const float SpriteForward = MathHelper.PiOver4;

            private const int HomingDelay = 14;
            private const float HomingRange = 380f;
            private const float HomingTurn = 0.045f;
            private const float MaxSpeed = 19f;

            public override void SetStaticDefaults()
            {
                ProjectileID.Sets.TrailCacheLength[Type] = 10;
                ProjectileID.Sets.TrailingMode[Type] = 2;
            }

            public override void SetDefaults()
            {
                Projectile.width = 14;
                Projectile.height = 14;
                Projectile.friendly = true;
                Projectile.hostile = false;
                Projectile.DamageType = DamageClass.Magic;
                Projectile.penetrate = 2;
                Projectile.tileCollide = true;
                Projectile.ignoreWater = true;
                Projectile.aiStyle = -1;
                Projectile.timeLeft = 120;
                Projectile.usesLocalNPCImmunity = true;
                Projectile.localNPCHitCooldown = -1;
            }

            public override void AI()
            {
                Projectile.localAI[0]++;
                float age = Projectile.localAI[0];

                if (age == 1f && !Main.dedServ)
                    DeepVortexFx.Burst(Projectile.Center, 4, 0.5f, 2f, 6f, 10f, 14, 22);

                float speed = Projectile.velocity.Length();
                if (speed < MaxSpeed)
                    Projectile.velocity *= 1.012f;

                if (HomingTurn > 0f && age > HomingDelay)
                {
                    NPC target = FindTarget();
                    if (target != null)
                    {
                        float want = (target.Center - Projectile.Center).ToRotation();
                        float cur = Projectile.velocity.ToRotation();
                        Projectile.velocity = cur.AngleTowards(want, HomingTurn).ToRotationVector2() * Projectile.velocity.Length();
                    }
                }

                Projectile.rotation = Projectile.velocity.ToRotation() - SpriteForward;

                SpawnTrailFx();
                Lighting.AddLight(Projectile.Center, 0.3f, 0.45f, 0.65f);
            }

            private NPC FindTarget()
            {
                NPC best = null;
                float bestDist = HomingRange;

                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.CanBeChasedBy(this))
                        continue;

                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < bestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.position, npc.width, npc.height))
                    {
                        best = npc;
                        bestDist = dist;
                    }
                }

                return best;
            }

            private void SpawnTrailFx()
            {
                if (Main.dedServ || !Main.rand.NextBool(2))
                    return;

                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 perp = dir.RotatedBy(MathHelper.PiOver2);

                DeepVortexFx.Shard(
                    Projectile.Center - dir * Main.rand.NextFloat(0f, 10f) + perp * Main.rand.NextFloat(-3f, 3f),
                    -dir * Main.rand.NextFloat(0.3f, 1.5f) + perp * Main.rand.NextFloat(-0.8f, 0.8f),
                    Main.rand.NextFloat(6f, 12f),
                    Main.rand.Next(16, 26));
            }

            public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
            {
                if (!Main.dedServ)
                    DeepVortexFx.Burst(Projectile.Center, 6, 1.5f, 4f, 7f, 14f);
            }

            public override void OnKill(int timeLeft)
            {
                if (Main.dedServ)
                    return;

                DeepVortexFx.Burst(Projectile.Center, 14, 2f, 6f, 8f, 18f);
                SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, PitchVariance = 0.2f }, Projectile.Center);
            }

            public override bool PreDraw(ref Color lightColor)
            {
                if (Main.dedServ)
                    return false;

                Texture2D tex = TextureAssets.Projectile[Type].Value;
                Vector2 origin = new Vector2(tex.Width, tex.Height) * 0.1f;

                int len = Projectile.oldPos.Length;
                for (int k = len - 1; k >= 1; k--)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero)
                        continue;

                    float t = 1f - k / (float)len;
                    Main.EntitySpriteDraw(tex, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null,
                        DeepVortexFx.Glow * (t * t * 0.5f), Projectile.oldRot[k], origin,
                        Projectile.scale * MathHelper.Lerp(0.5f, 1f, t), SpriteEffects.None, 0f);
                }

                Vector2 pos = Projectile.Center - Main.screenPosition;

                Main.EntitySpriteDraw(tex, pos, null, DeepVortexFx.Glow * 0.45f, Projectile.rotation, origin,
                    Projectile.scale * 1.15f, SpriteEffects.None, 0f);
                Main.EntitySpriteDraw(tex, pos, null, Color.Lerp(lightColor, Color.White, 0.6f), Projectile.rotation, origin,
                    Projectile.scale, SpriteEffects.None, 0f);

                return false;
            }
        }
    }

    internal static class DeepVortexFx
    {
        public static readonly Color Crystal = new Color(255, 168, 135, 0);
        public static readonly Color Glow = new Color(190, 225, 255, 0);
                    // again fx
        public static float Smooth(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        public static void Shard(Vector2 pos, Vector2 vel, float size, int life, float alpha = 1f)
        {
            if (Main.dedServ || ParticleSystem.CrystalBuffer == null)
                return;

            ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVec2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                Crystal * Main.rand.NextFloat(0.85f, 1.15f) * alpha,
                life
            ));
        }

        public static void Burst(Vector2 center, int count, float minSpeed, float maxSpeed,
            float minSize, float maxSize, int minLife = 20, int maxLife = 36)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                Shard(center + dir * Main.rand.NextFloat(0f, 6f),
                    dir * Main.rand.NextFloat(minSpeed, maxSpeed),
                    Main.rand.NextFloat(minSize, maxSize),
                    Main.rand.Next(minLife, maxLife));
            }
        }
    }
}