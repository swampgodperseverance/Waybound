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

namespace Waybound.Content.Items.Weapons.Summon.PreHM
{    //gonna balance later
    public class Fibrillation : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.knockBack = 2f;
            Item.mana = 10;
            Item.width = 30;
            Item.height = 30;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.value = Item.buyPrice(0, 1, 50, 0);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item44;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<FibrillationBuff>();
            Item.shoot = ModContent.ProjectileType<FibrillationProj>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
            velocity = Vector2.Zero;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile p = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            p.originalDamage = Item.damage;

            if (!Main.dedServ)
            {
                for (int i = 0; i < 14; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 6f);
                    FibrillationProj.Spark(position, vel, new Color(235, 45, 90), Main.rand.NextFloat(8f, 14f), Main.rand.Next(16, 26));
                }
            }

            return false;
        }

        public override void PostUpdate()
        {
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 6f + Item.whoAmI);
            Lighting.AddLight(Item.Center, new Vector3(0.85f, 0.15f, 0.4f) * 0.35f * pulse);
        }
    }

    public class FibrillationProj : ModProjectile
    {
        private static readonly Color Blood = new Color(235, 45, 90);
        private static readonly Color Violet = new Color(175, 105, 245);

        public override string Texture => "Waybound/Content/Items/Weapons/Summon/PreHM/Fibrillation";

        private int State
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private int Timer
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public static void Spark(Vector2 pos, Vector2 vel, Color color, float size, int life)
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

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override bool? CanDamage() => State == 2 ? (bool?)null : false;

        public override bool MinionContactDamage() => State == 2;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            int buff = ModContent.BuffType<FibrillationBuff>();

            if (!owner.active || owner.dead)
                owner.ClearBuff(buff);

            if (owner.HasBuff(buff))
                Projectile.timeLeft = 2;

            int index = 0;
            int total = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != Projectile.owner || p.type != Projectile.type)
                    continue;

                if (p.whoAmI < Projectile.whoAmI)
                    index++;
                total++;
            }

            if (Vector2.Distance(owner.Center, Projectile.Center) > 1600f)
            {
                Projectile.Center = owner.Center;
                Projectile.velocity = Vector2.Zero;
                State = 0;
                Timer = 0;
                Projectile.netUpdate = true;
            }

            NPC target = FindTarget(owner);
            Timer++;

            if (target == null)
            {
                if (State != 0)
                {
                    State = 0;
                    Timer = 0;
                    Projectile.netUpdate = true;
                }

                float bob = MathF.Sin(Main.GameUpdateCount * 0.07f + index) * 8f;
                Vector2 home = owner.Center + new Vector2(-owner.direction * (36f + index * 28f), -56f + bob);
                Move(home, 14f, 0.12f);
                RotateTo(Projectile.velocity.X * 0.06f);
            }
            else if (State == 0)
            {
                float ang = MathHelper.TwoPi * index / Math.Max(total, 1) + Main.GameUpdateCount * 0.02f;
                Vector2 goal = target.Center + new Vector2(0f, -110f).RotatedBy(ang);
                Move(goal, 10f, 0.1f);
                RotateTo(Projectile.velocity.X * 0.06f);

                if (Timer >= 34 + index * 9)
                {
                    State = 1;
                    Timer = 0;
                    Projectile.netUpdate = true;
                }
            }
            else if (State == 1)
            {
                Projectile.velocity *= 0.82f;
                Vector2 toTarget = target.Center - Projectile.Center;
                RotateTo(toTarget.ToRotation() + MathHelper.PiOver2);

                Vector2 axis = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
                if (Main.rand.NextBool(2))
                {
                    float sign = Main.rand.NextBool() ? 1f : -1f;
                    Spark(Projectile.Center + axis * 18f * sign, axis * sign * 1.5f, Violet, Main.rand.NextFloat(6f, 10f), 12);
                }

                if (Timer >= 14)
                {
                    Vector2 dir = toTarget.SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = dir * 15f;
                    State = 2;
                    Timer = 0;
                    Projectile.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item17 with { Volume = 0.6f, Pitch = 0.3f }, Projectile.Center);
                }
            }
            else
            {
                Vector2 want = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 15f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, want, 0.05f);
                RotateTo(Projectile.velocity.ToRotation() + MathHelper.PiOver2);

                if (Main.rand.NextBool(2))
                    Spark(Projectile.Center, -Projectile.velocity * 0.1f, Blood, Main.rand.NextFloat(8f, 12f), 14);

                if (Timer >= 18)
                {
                    State = 0;
                    Timer = 0;
                    Projectile.velocity *= 0.3f;
                    Projectile.netUpdate = true;
                }
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.85f, 0.15f, 0.4f) * (State == 2 ? 0.7f : 0.4f));
        }

        private void RotateTo(float rot)
        {
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, rot, 0.25f);
        }

        private void Move(Vector2 goal, float speed, float inertia)
        {
            Vector2 diff = goal - Projectile.Center;
            float len = diff.Length();
            Vector2 want = len > 1f ? diff / len * Math.Min(len * 0.14f, speed) : Vector2.Zero;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, want, inertia);
        }

        private NPC FindTarget(Player owner)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC forced = Main.npc[owner.MinionAttackTargetNPC];
                if (forced.CanBeChasedBy(this) && Vector2.Distance(forced.Center, owner.Center) < 1800f)
                    return forced;
            }

            NPC found = null;
            float best = 1000f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.CanBeChasedBy(this))
                    continue;

                float d = Vector2.Distance(n.Center, owner.Center);
                if (d < best)
                {
                    best = d;
                    found = n;
                }
            }
            return found;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Bleeding, 150);

            Vector2 axis = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
            for (int i = 0; i < 10; i++)
                Spark(Projectile.Center + axis * Main.rand.NextFloat(-18f, 18f), Main.rand.NextVector2Circular(4f, 4f), Blood, Main.rand.NextFloat(8f, 13f), Main.rand.Next(14, 22));

            for (int i = 0; i < 4; i++)
                Dust.NewDustPerfect(target.Center, DustID.Blood, Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.2f);

            if (State == 2)
            {
                Projectile.velocity = -Projectile.velocity * 0.4f;
                State = 0;
                Timer = 0;
                Projectile.netUpdate = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
                Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), Blood, Main.rand.NextFloat(8f, 12f), 18);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Texture2D star = TextureAssets.Extra[98].Value;
            Vector2 origin = tex.Size() / 2f;
            float pulse = 0.5f + 0.5f * MathF.Sin(Main.GameUpdateCount * 0.35f + Projectile.whoAmI);
            float scale = Projectile.scale * (1f + 0.06f * pulse);
            Color glow = new Color(Blood.R, Blood.G, Blood.B, 0);

            Vector2 shake = Main.rand.NextVector2Circular(0.8f, 0.8f);
            if (State == 1)
                shake = Main.rand.NextVector2Circular(2.4f, 2.4f);

            if (State == 2)
            {
                int len = Projectile.oldPos.Length;
                for (int k = 1; k < len; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero)
                        continue;

                    float f = 1f - k / (float)len;
                    Main.EntitySpriteDraw(tex, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null, glow * (f * 0.5f),
                        Projectile.oldRot[k], origin, scale * (0.6f + 0.4f * f), SpriteEffects.None, 0);
                }
            }

            Vector2 pos = Projectile.Center - Main.screenPosition + shake;

            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(1.8f, 0f).RotatedBy(MathHelper.PiOver2 * i + Main.GlobalTimeWrappedHourly);
                Main.EntitySpriteDraw(tex, pos + off, null, glow * (0.35f + 0.25f * pulse), Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(star, pos, null, glow * (0.25f + 0.35f * pulse), 0f, star.Size() / 2f, new Vector2(0.3f + 0.15f * pulse), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, pos, null, Color.Lerp(lightColor, Color.White, 0.4f), Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
}