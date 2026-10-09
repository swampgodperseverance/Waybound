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
using Waybound.Content.Buffs.Minions;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Summon.PreHM
{

    public abstract class KSitem : ModItem
    {
        protected abstract int MinionType { get; }
        protected abstract int MinionBuff { get; }
        protected abstract int BaseDamage { get; }
        protected abstract int SellValue { get; }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = BaseDamage;
            Item.knockBack = 3f;
            Item.mana = 10;
            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;

            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item44;

            Item.rare = ItemRarityID.Blue;
            Item.value = SellValue;

            Item.buffType = MinionBuff;
            Item.shoot = MinionType;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.Center + new Vector2(player.direction * 36f, -12f);
            velocity = Vector2.Zero;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            proj.originalDamage = Item.damage;
            return false;
        }
    }

    public class KingsGoldSword : KSitem
    {
        protected override int MinionType => ModContent.ProjectileType<KingsGoldKnight>();
        protected override int MinionBuff => ModContent.BuffType<GoldKnightBuff>();
        protected override int BaseDamage => 13;
        protected override int SellValue => Item.sellPrice(gold: 1, silver: 20);

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 14)
                .AddIngredient(ItemID.Topaz, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class KingsPlatinumSword : KSitem
    {
        protected override int MinionType => ModContent.ProjectileType<KingsPlatinumKnight>();
        protected override int MinionBuff => ModContent.BuffType<PlatinumKnightBuff>();
        protected override int BaseDamage => 14;
        protected override int SellValue => Item.sellPrice(gold: 1, silver: 50);

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.PlatinumBar, 14)
                .AddIngredient(ItemID.Sapphire, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    /// <summary>
    /// Are we fucking deadass? I really dont like the way i did that but it is what it is
    /// </summary>
    public class KingsFxAssets : ModSystem
    {
        public static Texture2D Bubble; // Its so silly
        public static Texture2D Soft;  

        public override void Load()
        {
            if (Main.dedServ)
                return;

            Main.QueueMainThreadAction(() =>
            {
                Bubble = Make(128, r =>
                    SmoothStep(0.70f, 0.97f, r) * (1f - SmoothStep(0.97f, 1f, r)) * 0.95f + r * r * 0.10f);
                Soft = Make(64, r => (1f - r) * (1f - r));
            });
        }

        public override void Unload()
        {
            Main.QueueMainThreadAction(() =>
            {
                Bubble?.Dispose();
                Soft?.Dispose();
                Bubble = null;
                Soft = null;
            });
        }

        private static float SmoothStep(float e0, float e1, float x)
        {
            float t = MathHelper.Clamp((x - e0) / (e1 - e0), 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        private static Texture2D Make(int size, Func<float, float> profile)
        {
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    float a = r >= 1f ? 0f : MathHelper.Clamp(profile(r), 0f, 1f);
                    byte v = (byte)(a * 255f);
                    data[y * size + x] = new Color(v, v, v, v); //bruh
                }
            }

            tex.SetData(data);
            return tex;
        }
    }


        //0-3   walk
        //4-9   attack
        //10-14 fly

    public abstract class KSproj : ModProjectile
    {
        protected abstract int BuffType { get; }
        protected abstract Color PrimaryColor { get; }
        protected abstract Color SecondaryColor { get; }

        private const int WalkFrameCount = 4;
        private const int AttackFirstFrame = 4;
        private const int AttackFrameCount = 6;
        private const int LevitateFirstFrame = 10;
        private const int LevitateFrameCount = 5;
        private const int AttackTicksPerFrame = 6;
        private const int AttackDuration = AttackFrameCount * AttackTicksPerFrame; 
        private const int HitStart = 12;  
        private const int HitEnd = 24;
        private const float SlashReach = 64f;

        private bool spawned;
        private bool wasAirborne;
        private bool grounded;
        private int attackTimer;
        private int attackCooldown;
        private int blockedTimer;
        private int stuckTimer;
        private int airTime;
        private int levitateAnim;
        private int walkFrame;
        private float lastX;
        private float shield;
        private float flash;  

        private bool Airborne => Projectile.ai[0] == 1f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 15;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.timeLeft = 18000;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = AttackDuration;
            Projectile.spriteDirection = 1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false; 
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false; 



        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner))
                return;

            if (!spawned)
            {
                spawned = true;
                lastX = Projectile.position.X;
                flash = 1f;
                Burst(Projectile.Center, 14, 2f, 5f);
            }

            if (Projectile.owner == Main.myPlayer)
            {
                NPC found = FindTarget(owner);
                int idx = found != null ? found.whoAmI + 1 : 0;
                if ((int)Projectile.ai[1] != idx)
                {
                    Projectile.ai[1] = idx;
                    Projectile.netUpdate = true;
                }

                if (Vector2.Distance(Projectile.Center, owner.Center) > 1800f)
                {
                    Projectile.Center = owner.Center;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                }

                DecideAirborne(owner, GetTarget());
                if (Main.GameUpdateCount % 30 == 0)
                    Projectile.netUpdate = true;
            }

            NPC target = GetTarget();



            if (Airborne != wasAirborne)
            {
                wasAirborne = Airborne;
                flash = 1f;
                if (Airborne)
                {
                    Projectile.velocity.Y = Math.Min(Projectile.velocity.Y, -4f); 
                    Burst(Projectile.Center, 14, 2f, 5f);
                    SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
                }
                else
                {
                    Burst(Projectile.Center, 20, 3f, 7f); 
                    SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);
                }
            }

            if (Airborne)
                AirMovement(owner, target);
            else
                GroundMovement(owner, target);

            AttackLogic(target);
            UpdateFrame();
            AmbientEffects();

            shield = MathHelper.Clamp(shield + (Airborne ? 0.07f : -0.07f), 0f, 1f);
            flash = Math.Max(0f, flash - 0.07f);

            float ease = shield * shield * (3f - 2f * shield);
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * (0.25f + 0.35f * ease + 0.3f * flash));
        }

        private bool CheckActive(Player owner)
        {
            if (!owner.active || owner.dead)
            {
                owner.ClearBuff(BuffType);
                return false;
            }

            if (owner.HasBuff(BuffType))
                Projectile.timeLeft = 2;

            return true;
        }


        private NPC GetTarget()
        {
            int idx = (int)Projectile.ai[1] - 1;
            if (idx < 0 || idx >= Main.maxNPCs)
                return null;

            NPC npc = Main.npc[idx];
            return npc.CanBeChasedBy(this) ? npc : null;
        }

        private NPC FindTarget(Player owner)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC forced = Main.npc[owner.MinionAttackTargetNPC];
                if (forced.CanBeChasedBy(this) && Vector2.Distance(forced.Center, owner.Center) < 1400f)
                    return forced;
            }

            NPC best = null;
            float bestDist = 750f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                    continue;

                if (Vector2.Distance(npc.Center, owner.Center) > 1000f)
                    continue;

                float d = Vector2.Distance(npc.Center, Projectile.Center);
                if (d < bestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                {
                    bestDist = d;
                    best = npc;
                }
            }

            return best;
        }

        private void DecideAirborne(Player owner, NPC target)
        {
            float dist = Vector2.Distance(Projectile.Center, owner.Center);
            bool air = Airborne;
            bool want = air;

            if (!air)
            {
                bool targetHigh = target != null
                    && target.Center.Y < Projectile.Top.Y - 90f
                    && Math.Abs(target.Center.X - Projectile.Center.X) < 420f;
                bool farFromPlayer = dist > 700f;
                bool playerAbove = target == null && owner.Center.Y < Projectile.Center.Y - 240f;

                if (targetHigh || farFromPlayer || playerAbove || stuckTimer > 50)
                    want = true;
            }
            else if (airTime > 40)
            {
                bool groundBelow = Collision.SolidCollision(Projectile.position + new Vector2(0f, Projectile.height), Projectile.width, 48, true);
                bool insideTiles = Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height);
                bool nearGoal = target != null
                    ? target.Center.Y > Projectile.Top.Y - 40f
                    : dist < 160f && owner.Center.Y > Projectile.Center.Y - 120f;

                if (groundBelow && !insideTiles && nearGoal)
                    want = false;
            }

            if (want != air)
            {
                Projectile.ai[0] = want ? 1f : 0f;
                Projectile.netUpdate = true;
            }
        }

        private void GroundMovement(Player owner, NPC target)
        {
            Projectile.tileCollide = true;
            airTime = 0;
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 0f, 0.25f);

            grounded = Projectile.velocity.Y >= 0f &&
                Collision.SolidCollision(Projectile.position + new Vector2(0f, Projectile.height), Projectile.width, 2, true);

            float goalX = target != null
                ? target.Center.X
                : owner.Center.X + -owner.direction * (40f + Projectile.minionPos * 32f);
            float dx = goalX - Projectile.Center.X;

            bool attacking = attackTimer > 0;
            float maxSpeed = target != null ? 5.4f : (Math.Abs(dx) > 220f ? 7.5f : 4.2f);
            bool wantMove = !attacking && Math.Abs(dx) > (target != null ? 34f : 18f);

            if (wantMove)
                Projectile.velocity.X += Math.Sign(dx) * 0.38f;
            else
                Projectile.velocity.X *= attacking ? 0.92f : 0.82f;

            Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -maxSpeed, maxSpeed);
            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.4f, 12f);

            float moved = Math.Abs(Projectile.position.X - lastX);
            if (wantMove && grounded && moved < 0.4f)
            {
                blockedTimer++;
                stuckTimer++;
            }
            else
            {
                blockedTimer = 0;
                if (moved > 0.8f || !wantMove)
                    stuckTimer = 0;
            }

            if (blockedTimer >= 5)
            {
                Projectile.velocity.Y = -7.5f;
                blockedTimer = 0;
            }

            if (grounded && !attacking && target != null
                && target.Center.Y < Projectile.Center.Y - 50f && Math.Abs(dx) < 90f)
            {
                Projectile.velocity.Y = -8f;
            }

            lastX = Projectile.position.X;

            if (!attacking)
            {
                if (target != null)
                    Projectile.spriteDirection = dx >= 0f ? 1 : -1;
                else if (Math.Abs(Projectile.velocity.X) > 0.3f)
                    Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;
            }
        }

        private void AirMovement(Player owner, NPC target)
        {
            Projectile.tileCollide = false;
            grounded = false;
            airTime++;
            blockedTimer = 0;
            stuckTimer = 0;
            lastX = Projectile.position.X;

            Vector2 goal;
            if (target != null)
            {
                float side = Projectile.Center.X < target.Center.X ? -1f : 1f;
                goal = target.Center + new Vector2(side * 58f, 4f);
            }
            else
            {
                float off = (30f + Projectile.minionPos * 34f) * -owner.direction;
                goal = owner.Center + new Vector2(off, -64f);
            }

            Vector2 to = goal - Projectile.Center;
            float dist = to.Length();
            float speed = Math.Min(target != null ? 11f : 14f, dist * 0.15f);
            Vector2 desired = dist > 0.01f ? to / dist * speed : Vector2.Zero;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.1f);
            Projectile.rotation = MathHelper.Clamp(Projectile.velocity.X * 0.04f, -0.35f, 0.35f);

            if (attackTimer <= 0)
            {
                if (target != null)
                    Projectile.spriteDirection = target.Center.X >= Projectile.Center.X ? 1 : -1;
                else if (Math.Abs(Projectile.velocity.X) > 0.4f)
                    Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;
            }
        }



        private Rectangle SlashBox(int dir)
        {
            float left = dir > 0 ? Projectile.Center.X - 4f : Projectile.Center.X - SlashReach + 4f;
            return new Rectangle((int)left, (int)(Projectile.Center.Y - 32f), (int)SlashReach, 64);
        }

        private void AttackLogic(NPC target)
        {
            if (attackCooldown > 0)
                attackCooldown--;

            if (attackTimer > 0)
            {
                attackTimer++;
                int dir = Projectile.spriteDirection == -1 ? -1 : 1;

                if (attackTimer == HitStart)
                {
                    Projectile.velocity.X += dir * (Airborne ? 4f : 3.2f);
                    SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
                }

                if (!Main.dedServ && attackTimer >= HitStart && attackTimer <= HitEnd && attackTimer % 2 == 0)
                {
                    float p = (attackTimer - HitStart) / (float)(HitEnd - HitStart);
                    float a = MathHelper.Lerp(-1.25f, 1.0f, p);
                    Vector2 d = new Vector2(dir * (float)Math.Cos(a), (float)Math.Sin(a));
                    Spark(Projectile.Center + d * 44f, d * Main.rand.NextFloat(1f, 2.5f), Main.rand.NextFloat(0.5f, 0.8f));
                    GlowDust(Projectile.Center + d * 44f, d * 1.5f, Main.rand.NextFloat(0.9f, 1.3f));
                }

                if (attackTimer > AttackDuration)
                {
                    attackTimer = 0;
                    attackCooldown = 14;
                }
                return;
            }

            if (target == null || attackCooldown > 0)
                return;

            int facing = target.Center.X >= Projectile.Center.X ? 1 : -1;
            if (SlashBox(facing).Intersects(target.Hitbox))
            {
                Projectile.spriteDirection = facing;
                attackTimer = 1;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (attackTimer < HitStart || attackTimer > HitEnd)
                return false;

            return projHitbox.Intersects(targetHitbox)
                || SlashBox(Projectile.spriteDirection == -1 ? -1 : 1).Intersects(targetHitbox);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = target.Center.X >= Projectile.Center.X ? 1 : -1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 pos = Vector2.Clamp(Projectile.Center, target.Hitbox.TopLeft(), target.Hitbox.BottomRight());
            for (int i = 0; i < 8; i++)
                Spark(pos, Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextFloat(0.5f, 0.9f));
        }

        private void UpdateFrame()
        {
            if (attackTimer > 0)
            {
                Projectile.frame = AttackFirstFrame + Math.Min(AttackFrameCount - 1, (attackTimer - 1) / AttackTicksPerFrame);
            }
            else if (Airborne)
            {
                levitateAnim++;
                Projectile.frame = LevitateFirstFrame + (levitateAnim / 7) % LevitateFrameCount;
            }
            else
            {
                if (!grounded)
                {
                    Projectile.frame = 1; 
                }
                else if (Math.Abs(Projectile.velocity.X) > 0.35f)
                {
                    Projectile.frameCounter++;
                    int delay = Math.Abs(Projectile.velocity.X) > 5.5f ? 4 : 6;
                    if (Projectile.frameCounter >= delay)
                    {
                        Projectile.frameCounter = 0;
                        walkFrame = (walkFrame + 1) % WalkFrameCount;
                    }
                    Projectile.frame = walkFrame;
                }
                else
                {
                    Projectile.frameCounter = 0;
                    walkFrame = 0;
                    Projectile.frame = 0;
                }
            }
        }

        private void AmbientEffects()
        {
            if (Main.dedServ)
                return;

            Vector2 feet = new Vector2(Projectile.Center.X, Projectile.Bottom.Y);

            if (Airborne && shield > 0.2f)
            {
                if (Main.GameUpdateCount % 5 == 0)
                {
                    Spark(feet + new Vector2(Main.rand.NextFloat(-14f, 14f), 4f),
                        new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-2.2f, -0.8f)),
                        Main.rand.NextFloat(0.35f, 0.6f));
                }

                if (Main.rand.NextBool(14))
                {
                    Vector2 edge = Projectile.Center + Main.rand.NextVector2CircularEdge(32f, 32f);
                    GlowDust(edge, Vector2.Zero, Main.rand.NextFloat(0.8f, 1.1f));
                }
            }
            else if (grounded && Math.Abs(Projectile.velocity.X) > 3.5f && Main.rand.NextBool(6))
            {
                GlowDust(feet + new Vector2(0f, -2f), new Vector2(-Math.Sign(Projectile.velocity.X) * 0.8f, -0.4f), 0.8f);
            }
        }

        private void Burst(Vector2 center, int count, float minSpeed, float maxSpeed)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.15f, 0.15f));
                Spark(center + dir * 18f, dir * Main.rand.NextFloat(minSpeed, maxSpeed), Main.rand.NextFloat(0.5f, 0.9f));
            }

            for (int i = 0; i < 6; i++)
                GlowDust(center + Main.rand.NextVector2Circular(14f, 14f), Main.rand.NextVector2Circular(3f, 3f), Main.rand.NextFloat(1f, 1.4f));
        }

        private void Spark(Vector2 pos, Vector2 vel, float scale)
        {
            if (Main.dedServ)
                return;

            Color c = Main.rand.NextBool(3) ? SecondaryColor : PrimaryColor;
            c.A = 200;

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(scale),
                c,
                Main.rand.Next(14, 24)
            ));
        }

        private void GlowDust(Vector2 pos, Vector2 vel, float scale)
        {
            Dust d = Dust.NewDustPerfect(pos, DustID.TintableDustLighted, vel, 0, PrimaryColor, scale);
            d.noGravity = true;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;

            float t = Main.GlobalTimeWrappedHourly;
            float ease = shield * shield * (3f - 2f * shield);

            float bob = (float)Math.Sin(t * 3.2f + Projectile.whoAmI) * 3.5f * ease - 3f * ease;
            Vector2 spritePos = new Vector2(Projectile.Center.X, Projectile.Bottom.Y - frame.Height / 2f + Projectile.gfxOffY)
                + new Vector2(0f, bob);

            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float radius = Math.Max(30f, Math.Max(frame.Width, frame.Height) * 0.62f);


            Color c = Color.Lerp(lightColor, Color.White, 0.15f * ease);
            Main.EntitySpriteDraw(tex, spritePos - Main.screenPosition, frame, c, Projectile.rotation, origin, 1f, fx, 0);

            if (ease > 0.01f)
            {
                Color rim = PrimaryColor;
                rim.A = 0;
                Main.EntitySpriteDraw(tex, spritePos - Main.screenPosition, frame, rim * (0.28f * ease),
                    Projectile.rotation, origin, 1.04f, fx, 0);
            }

            DrawSlash(spritePos);
            DrawFlash(spritePos);

            return false;
        }

        private void DrawSoft(Texture2D soft, Vector2 worldPos, Color color, float rotation, Vector2 sizePx)
        {
            Main.EntitySpriteDraw(soft, worldPos - Main.screenPosition, null, color, rotation,
                soft.Size() / 2f, new Vector2(sizePx.X / soft.Width, sizePx.Y / soft.Height), SpriteEffects.None, 0);
        }

        

        private void DrawSlash(Vector2 center) // im proud of this one
        {
            Texture2D soft = KingsFxAssets.Soft;
            if (soft == null || attackTimer < HitStart || attackTimer > HitEnd + 8)
                return;

            int f = Projectile.spriteDirection == -1 ? -1 : 1;
            float p = MathHelper.Clamp((attackTimer - HitStart) / (float)(HitEnd - HitStart), 0f, 1f);
            float fade = attackTimer > HitEnd ? 1f - (attackTimer - HitEnd) / 8f : 1f;
            float aNow = MathHelper.Lerp(-1.25f, 1.0f, p);

            for (int k = 0; k < 10; k++)
            {
                float a = aNow - k * 0.17f;
                if (a < -1.25f)
                    break;

                float w = 1f - k / 10f;
                Vector2 dir = new Vector2(f * (float)Math.Cos(a), (float)Math.Sin(a));
                Color c = Color.Lerp(PrimaryColor, SecondaryColor, w);
                c.A = 0;

                DrawSoft(soft, center + dir * 44f, c * (0.8f * w * fade), dir.ToRotation(),
                    new Vector2(8f * w + 3f, 24f * w + 6f));
            }
        }

        private void DrawFlash(Vector2 center)
        {
            if (flash <= 0.01f)
                return;

            Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            float size = 60f + 80f * flash;
            float sc = size / glow.Width;

            Color s = SecondaryColor; s.A = 0;
            Color p = PrimaryColor; p.A = 0;

            Main.EntitySpriteDraw(glow, center - Main.screenPosition, null, p * (flash * 0.8f), 0f,
                glow.Size() / 2f, sc * 1.4f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, center - Main.screenPosition, null, s * (flash * 0.9f), MathHelper.PiOver4,
                glow.Size() / 2f, sc, SpriteEffects.None, 0);
        }
    }

    public class KingsGoldKnight : KSproj
    {
        protected override int BuffType => ModContent.BuffType<GoldKnightBuff>();
        protected override Color PrimaryColor => new Color(255, 205, 90);
        protected override Color SecondaryColor => new Color(255, 245, 190);
    }

    public class KingsPlatinumKnight : KSproj
    {
        protected override int BuffType => ModContent.BuffType<PlatinumKnightBuff>();
        protected override Color PrimaryColor => new Color(150, 215, 255);
        protected override Color SecondaryColor => new Color(235, 248, 255);
    }
}