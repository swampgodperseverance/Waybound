using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Minions;
using Waybound.Content.Items.Materials.Bars;
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Summon.PreHM
{
    public class HielitiumStaff2 : ModItem
    {
        public const float SlotsPerCast = 1f;
        public const float MaxSlots = 3f;
        public const float StaffTilt = 0f;

        public static readonly Color Ice = new Color(110, 215, 255);
        public static readonly Color IceSoft = new Color(180, 235, 255);
        public static readonly Color IceDeep = new Color(80, 190, 255);

        public static void Spark(Vector2 pos, Vector2 vel, float size, Color color, int life)
        {
            if (Main.dedServ || ParticleSystem.MegasparkBuffer == null) return;
            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(), vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                color, life));
        }

        public static void Snow(Vector2 pos, Vector2 vel, float size, Color color, int life)
        {
            if (Main.dedServ || ParticleSystem.SnowFlakeBuffer == null) return;
            ParticleSystem.SnowFlakeBuffer.Create(new ParticleInfo(
                pos.ToNumerics(), vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size), color, life));
        }

        public static void Star(Vector2 world, float rot, Vector2 scale, Color color)
        {
            Texture2D t = TextureAssets.Extra[98].Value;
            Main.EntitySpriteDraw(t, world - Main.screenPosition, null, color, rot, t.Size() / 2f, scale, SpriteEffects.None, 0);
        }

        public static void Ring(Vector2 center, int count, float speed, float size, Color color)
        {
            float off = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = (off + MathHelper.TwoPi * i / count).ToRotationVector2();
                Spark(center + dir * 6f, dir * speed * Main.rand.NextFloat(0.85f, 1.15f),
                    size * Main.rand.NextFloat(0.8f, 1.2f), color, Main.rand.Next(20, 32));
            }
        }

        public static void Snowflake(Vector2 center, float radius, float alpha, float rotation, int branches = 2)
        {
            const int arms = 6;
            for (int a = 0; a < arms; a++)
            {
                float ang = rotation + MathHelper.TwoPi * a / arms;
                Vector2 dir = ang.ToRotationVector2();

                for (int s = 1; s <= 4; s++)
                {
                    float t = s / 4f;
                    Vector2 p = center + dir * radius * t;
                    Star(p, ang + MathHelper.PiOver2, new Vector2(0.10f, 0.28f * (1f - t * 0.4f)),
                        Ice * (alpha * (1f - t * 0.5f)));
                }

                for (int b = 1; b <= branches; b++)
                {
                    float t = b / (float)(branches + 1);
                    Vector2 baseP = center + dir * radius * t;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float bAng = ang + side * 0.9f;
                        Vector2 bDir = bAng.ToRotationVector2();
                        float bLen = radius * 0.22f * (1f - t * 0.5f);
                        for (int s = 1; s <= 2; s++)
                        {
                            Vector2 p = baseP + bDir * bLen * (s / 2f);
                            Star(p, bAng + MathHelper.PiOver2, new Vector2(0.07f, 0.18f),
                                IceSoft * (alpha * 0.8f));
                        }
                    }
                }

                Star(center + dir * radius, ang, new Vector2(0.28f, 0.28f), IceSoft * alpha);
            }
        }

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 3));
        }

        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.width = 38;
            Item.height = 40;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = null;
            Item.autoReuse = false;
            Item.buffType = ModContent.BuffType<HielitiumStaff2Buff>();
            Item.shoot = ModContent.ProjectileType<HielitiumSnowflake>();
            Item.shootSpeed = 0f;
        }

        public override void HoldItem(Player player)
        {
            int held = ModContent.ProjectileType<HielitiumStaffHeld>();
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[held] < 1)
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.UnitX, held, 0, 0f, player.whoAmI);
        }

        public override bool CanUseItem(Player player)
        {
            HielitiumPlayer hp = player.GetModPlayer<HielitiumPlayer>();
            if (hp.Flake != null && hp.Flake.ai[0] >= MaxSlots)
                return false;

            return player.maxMinions - player.slotsMinions >= SlotsPerCast;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            HielitiumPlayer hp = player.GetModPlayer<HielitiumPlayer>();
            hp.CastGlow = 1f;

            Projectile flake = hp.Flake;
            if (flake != null)
            {
                flake.ai[0] = Math.Min(MaxSlots, flake.ai[0] + SlotsPerCast);
                flake.netUpdate = true;
            }
            else
            {
                Vector2 spawn = player.Center + new Vector2(player.direction * 20f, -50f);
                Projectile.NewProjectile(source, spawn, Vector2.Zero, type, damage, knockback, player.whoAmI, SlotsPerCast);
            }

            SoundEngine.PlaySound(SoundID.Item43 with { Volume = 0.8f, Pitch = 0.1f }, player.Center);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<HielitiumBar>(9)
                .AddIngredient(ItemID.SnowBlock, 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class HielitiumPlayer : ModPlayer
    {
        public float CastGlow;
        public Projectile Flake { get; private set; }
        public int Stage { get; private set; }

        public override void PostUpdate()
        {
            CastGlow = Math.Max(0f, CastGlow - 0.04f);

            Flake = null;
            Stage = 0;

            int type = ModContent.ProjectileType<HielitiumSnowflake>();
            if (Player.ownedProjectileCounts[type] <= 0) return;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.type == type)
                {
                    Flake = p;
                    Stage = HielitiumSnowflake.StageOf(p.ai[0]);
                    break;
                }
            }
        }
    }

    public class HielitiumStaffHeld : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Summon/PreHM/HielitiumStaff2";

        private bool burst;
        private Vector2 tip;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<HielitiumStaff2>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Type])
                    Projectile.frame = 0;
            }

            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;

            HielitiumPlayer hp = player.GetModPlayer<HielitiumPlayer>();

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 now = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                if (Vector2.Dot(now, Projectile.velocity.SafeNormalize(Vector2.UnitX)) < 0.9995f)
                {
                    Projectile.velocity = now;
                    Projectile.netUpdate = true;
                }
            }

            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            player.ChangeDir(direction.X > 0f ? 1 : -1);
            Projectile.direction = player.direction;
            Projectile.spriteDirection = player.direction;

            float cast = hp.CastGlow;
            float pulse = 0.5f + 0.5f * (float)Math.Sin(Main.GameUpdateCount * 0.08f);
            float bob = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 1.2f;
            float sway = (float)Math.Sin(Main.GameUpdateCount * 0.035f) * 0.02f;
            float lift = cast * 2f * pulse;

            Projectile.Center = player.MountedCenter + new Vector2(player.direction * 6f, -30f + bob - lift);
            Projectile.rotation = -player.direction * HielitiumStaff2.StaffTilt + player.direction * sway;

            float armFront = -MathHelper.Pi + player.direction * (0.12f + sway);
            float armBack = -MathHelper.Pi + player.direction * (0.30f + sway);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armFront);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armBack);

            tip = Projectile.Center + new Vector2(0f, -17f).RotatedBy(player.direction * sway);

            if (cast > 0.7f && pulse > 0.9f && !burst)
            {
                burst = true;
                HielitiumStaff2.Ring(tip, 14 + hp.Stage * 4, 4f, 16f, new Color(100, 210, 255, 210));
            }
            else if (cast < 0.4f)
                burst = false;

            if (Main.rand.NextBool(Math.Max(2, 6 - hp.Stage * 2)))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(0.3f, 1f));
                HielitiumStaff2.Spark(tip + Main.rand.NextVector2Circular(6f, 6f), vel,
                    Main.rand.NextFloat(10f, 16f) * (1f + hp.Stage * 0.2f),
                    new Color(100, 205, 255, 200), Main.rand.Next(16, 26));
            }

            Lighting.AddLight(tip, 0.2f + cast * 0.3f, 0.45f + cast * 0.35f, 0.65f + cast * 0.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            HielitiumPlayer hp = player.GetModPlayer<HielitiumPlayer>();

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float pulse = 0.55f + 0.45f * (0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.2f));
            float glowStrength = (0.35f + hp.CastGlow * 0.55f + hp.Stage * 0.1f) * pulse;
            Color glowColor = new Color(90, 190, 255, 50) * glowStrength;

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.4f + hp.CastGlow * 1.2f, 0f)
                    .RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 1.5f);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, frame,
                    glowColor, Projectile.rotation, origin, Projectile.scale * (1f + hp.CastGlow * 0.05f), effects, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame,
                lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            if (hp.CastGlow > 0.05f)
                HielitiumStaff2.Star(tip, Main.GlobalTimeWrappedHourly * 2f,
                    new Vector2(0.5f + hp.CastGlow * 0.8f),
                    new Color(130, 220, 255, 0) * hp.CastGlow);

            return false;
        }
    }

    public class HielitiumSnowflake : ModProjectile
    {
        private static readonly float[] StageScale = { 1f, 1.25f, 1.55f };
        private static readonly float[] StageDamage = { 1f, 1.3f, 1.7f };

        private float lastSlots = -1f;
        private float fromScale = 0.2f;
        private int growTimer;
        private int attackTimer;
        private int volley;
        private int auraTimer;

        public static int StageOf(float slots) => (int)MathHelper.Clamp((int)slots - 1, 0, 2);

        private int Stage => StageOf(Projectile.ai[0]);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = HielitiumStaff2.SlotsPerCast;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ContinuouslyUpdateDamageStats = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;
        public override bool? CanDamage() => false;

        private bool CheckActive(Player player)
        {
            int buff = ModContent.BuffType<HielitiumStaff2Buff>();
            if (player.dead || !player.active)
            {
                player.ClearBuff(buff);
                return false;
            }
            if (player.HasBuff(buff))
                Projectile.timeLeft = 2;
            return true;
        }

        private NPC FindTarget(Player player, float range)
        {
            if (player.HasMinionAttackTargetNPC)
            {
                NPC n = Main.npc[player.MinionAttackTargetNPC];
                if (n.CanBeChasedBy(Projectile) && Vector2.Distance(n.Center, Projectile.Center) < range * 1.6f)
                    return n;
            }

            NPC best = null;
            float bestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.CanBeChasedBy(Projectile)) continue;
                float d = Vector2.Distance(n.Center, Projectile.Center);
                if (d < bestDist && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1))
                {
                    best = n;
                    bestDist = d;
                }
            }
            return best;
        }

        private void OnGrow(int stage)
        {
            fromScale = Projectile.scale;
            growTimer = 30;

            HielitiumStaff2.Ring(Projectile.Center, 14 + stage * 6, 5f + stage, 18f, new Color(110, 215, 255, 215));
            HielitiumStaff2.Ring(Projectile.Center, 8 + stage * 4, 2.5f, 20f, new Color(220, 245, 255, 220));
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.8f, Pitch = 0.3f + stage * 0.15f }, Projectile.Center);
        }

        private void Fire(NPC target, int stage)
        {
            volley++;
            Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            int dmg = (int)(Projectile.damage * StageDamage[stage]);
            int shard = ModContent.ProjectileType<HielitiumShard>();

            if (stage == 0)
            {
                Shoot(shard, dir, 11f, dmg, stage);
            }
            else if (stage == 1)
            {
                for (int i = -1; i <= 1; i++)
                    Shoot(shard, dir.RotatedBy(i * 0.2f), 11.5f, dmg, stage);
            }
            else
            {
                for (int i = -2; i <= 2; i++)
                    Shoot(shard, dir.RotatedBy(i * 0.16f), 12f, dmg, stage);

                if (volley % 3 == 0)
                {
                    float off = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < 12; i++)
                        Shoot(shard, (off + MathHelper.TwoPi * i / 12f).ToRotationVector2(), 6.5f, dmg, stage);

                    HielitiumStaff2.Ring(Projectile.Center, 20, 6f, 20f, new Color(160, 230, 255, 220));
                    SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
                }
            }

            for (int i = 0; i < 4 + stage * 2; i++)
                HielitiumStaff2.Spark(Projectile.Center, dir.RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 5f),
                    Main.rand.NextFloat(10f, 16f), new Color(110, 215, 255, 210), Main.rand.Next(14, 24));

            SoundEngine.PlaySound(SoundID.Item43 with { Volume = 0.5f, Pitch = 0.3f + stage * 0.1f }, Projectile.Center);
        }

        private void Shoot(int type, Vector2 dir, float speed, int dmg, int stage)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir * speed,
                type, dmg, Projectile.knockBack, Projectile.owner, stage);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player))
                return;

            if (Projectile.ai[0] < HielitiumStaff2.SlotsPerCast)
                Projectile.ai[0] = HielitiumStaff2.SlotsPerCast;

            float slots = Math.Min(Projectile.ai[0], HielitiumStaff2.MaxSlots);
            Projectile.minionSlots = slots;
            int stage = StageOf(slots);
            Projectile.frame = stage;

            if (lastSlots < 0f || slots > lastSlots)
            {
                lastSlots = slots;
                OnGrow(stage);
            }

            if (growTimer > 0)
                growTimer--;

            float t = 1f - growTimer / 30f;
            Projectile.scale = MathHelper.Lerp(fromScale, StageScale[stage], EaseFunctions.EaseOutBack(t));

            float range = 560f + 90f * stage;
            NPC target = FindTarget(player, range);

            float bob = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 8f;
            Vector2 goal = player.Center + new Vector2(-player.direction * 40f, -70f - stage * 10f + bob);

            if (target != null)
            {
                Vector2 toTarget = target.Center - player.Center;
                float len = toTarget.Length();
                goal = player.Center + toTarget.SafeNormalize(Vector2.UnitY) * Math.Min(len * 0.5f, 140f)
                    + new Vector2(0f, -60f - stage * 8f + bob);
            }

            Vector2 diff = goal - Projectile.Center;
            Vector2 desired = diff.SafeNormalize(Vector2.Zero) * Math.Min(diff.Length() * 0.12f, 16f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.12f);

            if (Vector2.Distance(player.Center, Projectile.Center) > 1800f)
            {
                Projectile.Center = player.Center + new Vector2(0f, -60f);
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }

            Projectile.rotation += (0.025f + 0.012f * stage) * (target != null ? 2.2f : 1f);

            int cooldown = stage == 0 ? 45 : stage == 1 ? 38 : 32;
            if (target != null)
            {
                attackTimer++;
                if (attackTimer >= cooldown)
                {
                    attackTimer = 0;
                    if (Projectile.owner == Main.myPlayer)
                        Fire(target, stage);
                }
            }
            else
                attackTimer = Math.Min(attackTimer, cooldown - 12);

            if (stage == 2)
            {
                auraTimer++;
                if (auraTimer >= 30)
                {
                    auraTimer = 0;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC n = Main.npc[i];
                            if (n.CanBeChasedBy(Projectile) &&
                                Vector2.DistanceSquared(n.Center, Projectile.Center) < 150f * 150f)
                                n.AddBuff(BuffID.Chilled, 90);
                        }
                    }
                }

                if (Main.rand.NextBool(4))
                {
                    Vector2 p = Projectile.Center + Main.rand.NextVector2Circular(150f, 150f);
                    HielitiumStaff2.Snow(p, new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(0.2f, 0.8f)),
                        Main.rand.NextFloat(10f, 16f), new Color(200, 235, 255, 200), Main.rand.Next(30, 50));
                }
            }

            if (Main.rand.NextBool(Math.Max(2, 5 - stage)))
            {
                float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + ang.ToRotationVector2() * 18f * Projectile.scale;
                HielitiumStaff2.Spark(pos, ang.ToRotationVector2() * 0.6f, Main.rand.NextFloat(10f, 16f),
                    new Color(110, 215, 255, 200), Main.rand.Next(14, 24));
            }

            Lighting.AddLight(Projectile.Center, 0.3f + stage * 0.1f, 0.55f + stage * 0.1f, 0.8f + stage * 0.1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int stage = Stage;
            Rectangle frame = tex.Frame(1, 3, 0, stage);
            Vector2 origin = frame.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.6f + 0.4f * (0.5f + 0.5f * (float)Math.Sin(time * 3.2f));

            if (stage == 2)
            {
                float radius = 150f * (0.95f + 0.05f * EaseFunctions.EaseInOutSine(pulse));
                HielitiumStaff2.Snowflake(Projectile.Center, radius, 0.35f * pulse, time * 0.4f, branches: 2);
            }

            for (int k = 1; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero) continue;
                float fade = 1f - k / (float)Projectile.oldPos.Length;
                Main.EntitySpriteDraw(tex, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, frame,
                    new Color(100, 210, 255, 0) * (fade * 0.3f), Projectile.oldRot[k], origin,
                    Projectile.scale * (0.6f + 0.4f * fade), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, pos, frame, new Color(90, 190, 255, 0) * (0.3f * pulse),
                Projectile.rotation, origin, Projectile.scale * 1.3f, SpriteEffects.None, 0);

            Color outline = new Color(100, 210, 255, 0) * (0.55f * (0.7f + 0.3f * pulse));
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.6f + stage * 0.4f, 0f)
                    .RotatedBy(MathHelper.TwoPi * i / 4f + time);
                Main.EntitySpriteDraw(tex, pos + offset, frame, outline, Projectile.rotation, origin,
                    Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, pos, frame, Color.Lerp(lightColor, Color.White, 0.6f),
                Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class HielitiumShard : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 100;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.penetrate = 1 + (int)Projectile.ai[0];
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 120);
            for (int i = 0; i < 3; i++)
                HielitiumStaff2.Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    Main.rand.NextFloat(10f, 16f), new Color(110, 215, 255, 210), Main.rand.Next(14, 24));
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(3))
                HielitiumStaff2.Spark(Projectile.Center, Projectile.velocity * -0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Main.rand.NextFloat(8f, 12f), new Color(110, 215, 255, 190), Main.rand.Next(12, 20));

            Lighting.AddLight(Projectile.Center, 0.25f, 0.5f, 0.7f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
                HielitiumStaff2.Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    Main.rand.NextFloat(12f, 18f), new Color(110, 215, 255, 210), Main.rand.Next(14, 24));

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D streak = TextureAssets.Extra[98].Value;

            Vector2 tail = Projectile.Center;
            for (int k = Projectile.oldPos.Length - 1; k >= 1; k--)
            {
                if (Projectile.oldPos[k] != Vector2.Zero)
                {
                    tail = Projectile.oldPos[k] + Projectile.Size / 2f;
                    break;
                }
            }

            float len = Vector2.Distance(Projectile.Center, tail);
            if (len > 2f)
            {
                float rot = (Projectile.Center - tail).ToRotation() + MathHelper.PiOver2;
                Rectangle src = new Rectangle(0, streak.Height / 2, streak.Width, streak.Height / 2);
                Vector2 org = new Vector2(streak.Width * 0.5f, 0f);
                float scaleY = len / (streak.Height * 0.5f) * 1.2f;
                Color c = new Color(80, 190, 255, 0);

                Main.EntitySpriteDraw(streak, Projectile.Center - Main.screenPosition, src, c * 0.7f, rot, org,
                    new Vector2(0.7f, scaleY), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(streak, Projectile.Center - Main.screenPosition, src, c * 0.3f, rot, org,
                    new Vector2(1.3f, scaleY * 0.8f), SpriteEffects.None, 0);
            }

            HielitiumStaff2.Star(Projectile.Center, Projectile.rotation, new Vector2(0.9f, 0.3f), new Color(180, 235, 255, 0));
            HielitiumStaff2.Star(Projectile.Center, Projectile.rotation + MathHelper.PiOver2, new Vector2(0.35f, 0.18f),
                new Color(110, 215, 255, 0));
            return false;
        }
    }
}