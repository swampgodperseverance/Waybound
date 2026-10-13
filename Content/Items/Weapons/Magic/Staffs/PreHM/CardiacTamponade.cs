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
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Magic.Staffs.PreHM
{
    //gonna balance later
    public class CardiacTamponade : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 18;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 46;
            Item.height = 46;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(0, 1, 50, 0);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<CardiacTamponadeHoldout>();
            Item.shootSpeed = 1f;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void PostUpdate()
        {
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f + Item.whoAmI);
            Lighting.AddLight(Item.Center, new Vector3(0.85f, 0.15f, 0.35f) * 0.35f * pulse);
        }
    }

    public class CardiacTamponadeHoldout : BaseHoldoutStaffProj
    {
        public const string BeatSound = "Waybound/Assets/Sounds/Bosses/Heartbeat";

        private const int FirstShot = 20;
        private const int BeatGap = 40;
        private const int Cycle = 120;

        private static readonly Color Blood = new Color(235, 45, 90);
        private static readonly Color Violet = new Color(175, 105, 245);
        private static Texture2D softGlow;

        private int timer;
        private int cyc;
        private float kick;

        public override string GlowTexture => "Waybound/Content/Items/Weapons/Magic/Staffs/PreHM/CardiacTamponade";
        public override int MaxCharge => Cycle;
        public override int ProjectileType => ModContent.ProjectileType<TamponadeOrb>();
        public override float ProjectileSpeed => 1f;
        public override float HoldoutDistance => 36f;
        public override float AimResponsiveness => 0.75f;
        public override float SpriteRotationOffset => MathHelper.PiOver4;
        public override float KnockbackForce => 4f;
        public override int ManaCostDivider => 3;
        public override SoundStyle ChargeSound => new SoundStyle(BeatSound) { Volume = 0.5f, Pitch = 0.15f, PitchVariance = 0.04f, MaxInstances = 4 };
        public override SoundStyle ShootSound => SoundID.Item8 with { Pitch = -0.5f };
        public override Color GlowColor => Blood;
        public override Color ChargeColor => new Color(160, 230, 255);

        private Vector2 HeartPos => Projectile.Center + Projectile.velocity * 20f;

        private float ChargeRatio => cyc >= 80 ? MathHelper.Clamp((cyc - 80) / 40f, 0f, 1f) : 0f;
        protected virtual int OrbCountMin => 4;
        protected virtual int OrbCountMax => 7;
        internal static Texture2D SoftGlow
        {
            get
            {
                if (softGlow == null || softGlow.IsDisposed)
                {
                    const int size = 128;
                    softGlow = new Texture2D(Main.instance.GraphicsDevice, size, size);
                    Color[] data = new Color[size * size];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = (x - 63.5f) / 63.5f;
                            float dy = (y - 63.5f) / 63.5f;
                            float d = MathF.Sqrt(dx * dx + dy * dy);
                            float a = MathF.Pow(MathHelper.Clamp(1f - d, 0f, 1f), 2.2f);
                            data[y * size + x] = new Color(a, a, a, a);
                        }
                    }
                    softGlow.SetData(data);
                }
                return softGlow;
            }
        }

        public override void Unload()
        {
            Texture2D a = softGlow;
            softGlow = null;
            Main.QueueMainThreadAction(() => a?.Dispose());
        }

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

        internal static void Flash(Vector2 pos, float size, Color color, int duration)
        {
            if (Main.dedServ)
                return;

            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                position: pos.ToNumerics(),
                velocity: System.Numerics.Vector2.Zero,
                rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                scale: new System.Numerics.Vector2(size),
                color: color,
                duration: duration));
        }

        internal static void Thump(Vector2 pos, float volume, float pitch)
        {
            if (Main.dedServ)
                return;

            SoundEngine.PlaySound(new SoundStyle(BeatSound)
            {
                Volume = volume,
                Pitch = pitch,
                PitchVariance = 0.04f,
                MaxInstances = 4
            }, pos);
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.DamageType = DamageClass.Magic;
        }

        protected override void SpawnBurstParticles()
        {
            kick = 1f;
            Vector2 pos = HeartPos;

            if (Main.dedServ)
                return;

            float off = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 18; i++)
                Spark(pos, (off + MathHelper.TwoPi * i / 18f).ToRotationVector2() * 4.5f, Blood, 12f, 20);

            for (int i = 0; i < 10; i++)
                Spark(pos, Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 10f), Violet, Main.rand.NextFloat(8f, 14f), 18);
        }

        protected override void SpawnChargeParticles()
        {
            float charge = ChargeRatio;
            if (charge <= 0f || Main.rand.NextBool(Math.Max(1, (int)MathHelper.Lerp(4f, 1f, charge))))
            {
                Vector2 off = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(30f, 62f);
                Spark(HeartPos + off, -off * 0.09f, Main.rand.NextBool(3) ? Violet : Blood, Main.rand.NextFloat(6f, 10f), 14);
            }
        }

        protected override void SpawnAmbientParticles()
        {
            if (Main.rand.NextBool(14))
                Spark(HeartPos + Main.rand.NextVector2Circular(8f, 8f), new Vector2(0f, Main.rand.NextFloat(0.5f, 1.5f)), Blood, Main.rand.NextFloat(4f, 7f), 14);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.CCed || player.noItems || !player.channel || player.HeldItem.type != ModContent.ItemType<CardiacTamponade>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 120;

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true, true);

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 aim = Main.MouseWorld - playerCenter;
                if (aim.HasNaNs() || aim == Vector2.Zero)
                    aim = -Vector2.UnitY;
                else
                    aim.Normalize();

                Vector2 v = Vector2.Normalize(Vector2.Lerp(Projectile.velocity, aim, AimResponsiveness));
                if (v.HasNaNs())
                    v = aim;

                if (v != Projectile.velocity)
                    Projectile.netUpdate = true;

                Projectile.velocity = v;
            }

            if (Projectile.velocity.X > 0f)
                Projectile.spriteDirection = 1;
            else if (Projectile.velocity.X < 0f)
                Projectile.spriteDirection = -1;

            player.ChangeDir(Projectile.spriteDirection);
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = (Projectile.velocity * Projectile.spriteDirection).ToRotation();

            kick = Math.Max(0f, kick - 0.08f);
            Projectile.Center = playerCenter + Projectile.velocity * (HoldoutDistance - 7f * kick);
            Projectile.rotation = Projectile.velocity.ToRotation()
                + SpriteRotationOffset * (Projectile.spriteDirection == 1 ? 1 : -1)
                + (Projectile.velocity.X < 0f ? MathHelper.Pi : 0f);

            timer++;
            cyc = (timer + Cycle - FirstShot) % Cycle;
            float charge = ChargeRatio;

            if (timer == 1)
                Thump(Projectile.Center, 0.4f, 0f);

            if (timer >= FirstShot)
            {
                int c = cyc % BeatGap;
                if (c == 0)
                {
                    bool shot = cyc == 0;
                    Thump(Projectile.Center, shot ? 1f : 0.6f, shot ? -0.2f : 0f);

                    if (shot && Projectile.owner == Main.myPlayer)
                    {
                        int manaCost = Math.Max(1, player.HeldItem.mana);
                        if (timer > FirstShot && !player.CheckMana(manaCost, true))
                        {
                            Projectile.Kill();
                            return;
                        }

                        int count = Main.rand.Next(OrbCountMin, OrbCountMax + 1);
                        int volley = Main.rand.Next(1, 100000);
                        float baseAng = Projectile.velocity.ToRotation();
                        float spread = MathHelper.Lerp(1.15f, 1.6f, (7 - count) / 3f);
                        float spawnRadius = 14f;

                        for (int i = 0; i < count; i++)
                        {
                            float t = count == 1 ? 0.5f : i / (float)(count - 1);
                            float ang = baseAng + MathHelper.Lerp(-spread * 0.5f, spread * 0.5f, t);

                            Vector2 spawnPos = HeartPos + ang.ToRotationVector2() * spawnRadius;

                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                spawnPos,
                                ang.ToRotationVector2() * 18f,
                                ProjectileType,
                                Projectile.damage,
                                KnockbackForce,
                                Projectile.owner,
                                volley,
                                i + count * 10,
                                0f);
                        }

                        SpawnBurstParticles();
                    }
                }
                else if (c == 10)
                    Thump(Projectile.Center, 0.5f, 0.15f);
            }

            SpawnChargeParticles();
            SpawnAmbientParticles();

            Lighting.AddLight(HeartPos, Blood.ToVector3() * (0.4f + charge * 0.5f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float charge = ChargeRatio;
            float g = 0.2f + 0.25f * charge;
            Color add = new Color(Blood.R, Blood.G, Blood.B, 0);

            Main.EntitySpriteDraw(tex, pos, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            for (int i = 0; i < 8; i++)
            {
                Vector2 off = new Vector2(2f, 0f).RotatedBy(MathHelper.PiOver4 * i);
                Main.EntitySpriteDraw(tex, pos + off, null, add * (0.2f * g * 2f), Projectile.rotation, origin, Projectile.scale, effects, 0);
            }

            Main.EntitySpriteDraw(tex, pos, null, add * (g * 1.4f), Projectile.rotation, origin, Projectile.scale * 1.05f, effects, 0);

            Vector2 hp = HeartPos - Main.screenPosition;
            Main.EntitySpriteDraw(SoftGlow, hp, null, add * (0.3f + 0.3f * charge), 0f, new Vector2(64f), 56f / 128f, SpriteEffects.None, 0);

            Texture2D star = TextureAssets.Extra[98].Value;
            Main.EntitySpriteDraw(star, hp, null, add * (0.3f + 0.3f * charge), 0f, star.Size() / 2f, new Vector2(0.22f + 0.1f * charge), SpriteEffects.None, 0);

            return false;
        }
    }

    public class TamponadeOrb : ModProjectile
    {
        private const int Burst = 28;
        private const int Hold = 34;
        private const int Stagger = 6;
        private const float BurstSpeed = 17f;
        private const int LinkDelay = 12;
        private const int LinkGrow = 24;
        private const int LinkRetract = 8;

        private static readonly Color Blood = new Color(235, 45, 90);
        private static readonly Color Violet = new Color(175, 105, 245);

        public override string Texture => "Waybound/Content/NPCs/Bosses/Viscula/VisculaBolt";

        private int Age => (int)Projectile.localAI[0];
        private int LaunchAge => (int)Projectile.localAI[2];
        private int Index => (int)Projectile.ai[1] % 10;
        private int Count => Math.Max((int)Projectile.ai[1] / 10, 1);
        private bool Launched => Projectile.ai[2] != 0f;

        private float Grow
        {
            get
            {
                float g = MathHelper.Clamp((Age - Burst * 0.5f - LinkDelay - Index * 4f) / LinkGrow, 0f, 1f);
                return EaseFunctions.EaseInOutCubic(g);
            }
        }

        private float Retract => Launched ? MathHelper.Clamp(LaunchAge / (float)LinkRetract, 0f, 1f) : 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
        }

        public override bool? CanDamage() => Launched && LaunchAge >= 6 ? (bool?)null : false;

        private Projectile Sibling(int index)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == Type && p.owner == Projectile.owner && p.whoAmI != Projectile.whoAmI
                    && (int)p.ai[0] == (int)Projectile.ai[0] && (int)p.ai[1] % 10 == index)
                    return p;
            }
            return null;
        }

        private NPC ResolveTarget()
        {
            int idx = (int)Projectile.ai[2] - 2;
            if (idx >= 0 && idx < Main.maxNPCs && Main.npc[idx].CanBeChasedBy(this))
                return Main.npc[idx];

            NPC best = null;
            float bd = 800f * 800f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.CanBeChasedBy(this))
                    continue;

                float d = Vector2.DistanceSquared(n.Center, Projectile.Center);
                if (d < bd)
                {
                    bd = d;
                    best = n;
                }
            }
            return best;
        }

        private void Launch()
        {
            Vector2 mouse = Main.MouseWorld;
            int best = -1;
            float bd = 700f * 700f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.CanBeChasedBy(this))
                    continue;

                float d = Vector2.DistanceSquared(n.Center, mouse);
                if (d < bd)
                {
                    bd = d;
                    best = i;
                }
            }

            if (best == -1)
            {
                bd = 1100f * 1100f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (!n.CanBeChasedBy(this))
                        continue;

                    float d = Vector2.DistanceSquared(n.Center, Projectile.Center);
                    if (d < bd)
                    {
                        bd = d;
                        best = i;
                    }
                }
            }

            Projectile.ai[2] = best >= 0 ? best + 2f : 1f;
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            int age = Age;
            if (age == 1)
                Projectile.localAI[1] = Projectile.velocity.ToRotation();

            if (!Launched)
            {
                float dir = Projectile.localAI[1];

                if (age <= Burst)
                {
                    float k = age / (float)Burst;
                    Projectile.velocity = dir.ToRotationVector2() * MathHelper.Lerp(BurstSpeed, 0.5f, EaseFunctions.EaseOutCubic(k));

                    if (!Main.dedServ && Main.rand.NextBool(2))
                        CardiacTamponadeHoldout.Spark(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), -Projectile.velocity * 0.1f, Blood, Main.rand.NextFloat(6f, 10f), 14);
                }
                else
                {
                    float bob = MathF.Sin(age * 0.09f + Index * 1.3f) * 0.35f;
                    Projectile.velocity = dir.ToRotationVector2() * 0.4f + (dir + MathHelper.PiOver2).ToRotationVector2() * bob;

                    if (Projectile.owner == Main.myPlayer && age >= Burst + Hold + Index * Stagger)
                        Launch();
                }
            }
            else
                MoveLaunched();

            Projectile.rotation += 0.15f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.85f, 0.15f, 0.35f) * 0.8f);
        }

        private void MoveLaunched()
        {
            Projectile.localAI[2]++;
            int la = LaunchAge;
            NPC target = ResolveTarget();

            if (la == 1)
            {
                Projectile.tileCollide = true;
                Projectile.timeLeft = 160;

                if (target != null)
                    Projectile.localAI[1] = (target.Center - Projectile.Center).ToRotation();

                CardiacTamponadeHoldout.Thump(Projectile.Center, 0.5f, 0.2f);

                if (!Main.dedServ)
                {
                    for (int i = 0; i < 8; i++)
                        CardiacTamponadeHoldout.Spark(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 4f), Main.rand.NextBool(3) ? Violet : Blood, Main.rand.NextFloat(7f, 11f), 16);
                }
            }

            float ang = Projectile.localAI[1];
            float k = MathHelper.Clamp(la / 30f, 0f, 1f);

            if (target != null)
                ang = Utils.AngleTowards(ang, (target.Center - Projectile.Center).ToRotation(), MathHelper.Lerp(0.02f, 0.2f, k));

            Projectile.localAI[1] = ang;

            float speed = la < 9
                ? -MathHelper.Lerp(2.8f, 0f, la / 9f)
                : MathHelper.Lerp(2f, 20f, EaseFunctions.EaseInCubic(MathHelper.Clamp((la - 9) / 22f, 0f, 1f)));

            Projectile.velocity = ang.ToRotationVector2() * speed;

            if (!Main.dedServ && la > 9)
            {
                CardiacTamponadeHoldout.Spark(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), -Projectile.velocity * 0.08f, Blood, Main.rand.NextFloat(6f, 10f), 14);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Bleeding, 180);

            for (int i = 0; i < 8; i++)
                CardiacTamponadeHoldout.Spark(target.Center + Main.rand.NextVector2Circular(8f, 8f), Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextBool(3) ? Violet : Blood, Main.rand.NextFloat(7f, 12f), 16);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => true;

        public override void OnKill(int timeLeft)
        {
            Vector2 c = Projectile.Center;

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.55f, Pitch = -0.3f }, c);
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.35f, Pitch = 0.3f }, c);
            CardiacTamponadeHoldout.Thump(c, 0.6f, -0.2f);

            if (Main.dedServ)
                return;

            CardiacTamponadeHoldout.Flash(c, 120f, new Color(255, 190, 205, 220), 14);
            CardiacTamponadeHoldout.Flash(c, 70f, Color.White, 8);

            float off = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 16; i++)
            {
                Vector2 dir = (off + MathHelper.TwoPi * i / 16f).ToRotationVector2();
                CardiacTamponadeHoldout.Spark(c, dir * 6f, Blood, 12f, 22);
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 dir = (off * 0.5f + MathHelper.TwoPi * i / 10f).ToRotationVector2();
                CardiacTamponadeHoldout.Spark(c, dir * 3f, Violet, 10f, 26);
            }

            for (int i = 0; i < 12; i++)
                CardiacTamponadeHoldout.Spark(c, Main.rand.NextVector2Circular(8f, 8f), Main.rand.NextBool(3) ? Violet : Blood, Main.rand.NextFloat(6f, 12f), Main.rand.Next(16, 28));
        }

        private static void DrawLink(Vector2 a, Vector2 b, float grow, float retract)
        {
            float vis = grow * (1f - retract);
            if (vis < 0.01f)
                return;

            Vector2 d = b - a;
            float len = d.Length();
            if (len < 2f)
                return;

            Texture2D px = TextureAssets.MagicPixel.Value;
            Texture2D star = TextureAssets.Extra[98].Value;
            Rectangle src = new Rectangle(0, 0, 1, 1);
            Vector2 o = new Vector2(0f, 0.5f);
            float rot = d.ToRotation();

            float head = grow;
            float tail = retract;
            float width = EaseFunctions.EaseOutCubic(Math.Min(grow * 1.4f, 1f)) * (1f - retract * 0.6f);
            bool growing = grow < 0.999f;

            const int segs = 18;
            for (int s = 0; s < segs; s++)
            {
                float u0 = tail + (head - tail) * s / segs;
                float u1 = tail + (head - tail) * (s + 1) / segs;
                float g = (u0 + u1) * 0.5f;

                float ends = 0.55f + 0.45f * MathF.Sin(MathHelper.Pi * g);
                float tip = growing ? 0.35f + 0.65f * MathF.Exp(-(head - g) * 4f) : 1f;
                float al = ends * tip * (1f - retract);

                Vector2 sp = a + d * u0 - Main.screenPosition;
                float segLen = len * (u1 - u0) + 1f;

                Main.EntitySpriteDraw(px, sp, src, new Color(Blood.R, Blood.G, Blood.B, 0) * (al * 0.2f), rot, o, new Vector2(segLen, 11f * width), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(px, sp, src, new Color(Violet.R, Violet.G, Violet.B, 0) * (al * 0.3f), rot, o, new Vector2(segLen, 4f * width), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(px, sp, src, new Color(255, 190, 205, 0) * (al * 0.9f), rot, o, new Vector2(segLen, 1.6f * width), SpriteEffects.None, 0);
            }

            if (growing && retract < 0.5f)
            {
                Vector2 hp = a + d * head - Main.screenPosition;
                float f = 1f - grow;
                Main.EntitySpriteDraw(star, hp, null, new Color(255, 190, 205, 0) * (0.9f * f + 0.1f), rot, star.Size() / 2f, new Vector2(0.22f + 0.1f * f), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(CardiacTamponadeHoldout.SoftGlow, hp, null, new Color(Blood.R, Blood.G, Blood.B, 0) * (0.6f * f), 0f, new Vector2(64f), 34f / 128f, SpriteEffects.None, 0);
            }
        }

        private void DrawLinks()
        {
            int count = Count;
            if (count < 3)
                return;

            Projectile next = Sibling((Index + 1) % count);
            if (next == null || !(next.ModProjectile is TamponadeOrb other))
                return;

            float retract = Math.Max(Retract, other.Retract);
            DrawLink(Projectile.Center, next.Center, Grow, retract);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawLinks();

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Texture2D star = TextureAssets.Extra[98].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Color add = new Color(Blood.R, Blood.G, Blood.B, 0);
            Color addV = new Color(Violet.R, Violet.G, Violet.B, 0);
            float scale = 1.25f * Projectile.scale;

            float trailK = MathHelper.Clamp(Projectile.velocity.Length() / 9f, 0f, 1f);
            if (trailK > 0.05f)
            {
                int len = Projectile.oldPos.Length;
                for (int k = 1; k < len; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero)
                        continue;

                    float f = 1f - k / (float)len;
                    Main.EntitySpriteDraw(tex, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null, add * (f * 0.4f * trailK),
                        Projectile.oldRot[k], origin, scale * (0.55f + 0.45f * f), SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(CardiacTamponadeHoldout.SoftGlow, pos, null, add * 0.75f, 0f, new Vector2(64f), 56f * scale / 128f, SpriteEffects.None, 0);

            for (int i = 0; i < 6; i++)
            {
                Vector2 off = new Vector2(1.5f, 0f).RotatedBy(MathHelper.TwoPi * i / 6f + Projectile.rotation);
                Main.EntitySpriteDraw(tex, pos + off, null, addV * 0.4f, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, pos, null, Color.White, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(star, pos, null, add * 0.5f, 0f, star.Size() / 2f, new Vector2(0.3f), SpriteEffects.None, 0);

            return false;
        }
    }
}