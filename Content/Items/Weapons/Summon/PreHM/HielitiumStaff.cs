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
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Summon.PreHM
{
    public class HielitiumStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.DamageType = DamageClass.Summon;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Orange;
            Item.mana = 10;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.buffType = ModContent.BuffType<HielitiumIcicleBuff>();
            Item.shoot = ModContent.ProjectileType<HielitiumIcicle>();
            Item.shootSpeed = 0f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile proj = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI);
            proj.originalDamage = Item.damage;

            for (int i = 0; i < 14; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                float size = Main.rand.NextFloat(14f, 24f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    player.MountedCenter.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                    new Color(100, 210, 255, 200),
                    Main.rand.Next(16, 28)
                ));
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<HielitiumBar>(9)
                .AddIngredient(ItemID.IceBlock, 25)
                .AddIngredient(ItemID.Book, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class HielitiumIcicle : ModProjectile
    {
        private const int StateIdle = 0;
        private const int StateLaunch = 1;
        private const int StateReturn = 2;
        private const int StateSwarm = 3;
        private const int StateWindup = 4;
        private const int StateDash = 5;

        private const float SpikeLength = 30f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.minionSlots = 0.5f;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.scale = 0.8f;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => Projectile.ai[0] == StateLaunch || Projectile.ai[0] == StateDash;

        private bool CheckActive(Player player)
        {
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<HielitiumIcicleBuff>());
                return false;
            }

            if (player.HasBuff(ModContent.BuffType<HielitiumIcicleBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private void GetSlot(Player player, out Vector2 pos, out float angle, out int index, out int total)
        {
            total = 0;
            index = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == Projectile.type)
                {
                    if (i < Projectile.whoAmI)
                        index++;
                    total++;
                }
            }

            float t = total <= 1 ? 0.5f : index / (float)(total - 1);
            float spread = Math.Min(0.15f * total, 0.9f);
            float center = new Vector2(-player.direction, -0.55f).ToRotation();
            angle = center + MathHelper.Lerp(-spread, spread, t);

            float bob = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.2f + index * 0.9f) * 2f;
            pos = player.MountedCenter + new Vector2(-player.direction * 9f, 0f) + angle.ToRotationVector2() * (22f + bob);
        }

        private NPC FindTarget(Player player)
        {
            if (player.HasMinionAttackTargetNPC)
            {
                NPC forced = Main.npc[player.MinionAttackTargetNPC];
                if (forced.CanBeChasedBy(this) && Vector2.Distance(forced.Center, player.Center) < 900f)
                    return forced;
            }

            NPC best = null;
            float bestDist = 650f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                    continue;

                float dist = Vector2.Distance(npc.Center, player.Center);
                if (dist >= bestDist)
                    continue;

                if (!Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1) &&
                    !Collision.CanHitLine(player.Center, 1, 1, npc.Center, 1, 1))
                    continue;

                bestDist = dist;
                best = npc;
            }
            return best;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player))
            {
                Projectile.Kill();
                return;
            }

            GetSlot(player, out Vector2 slotPos, out float slotAngle, out int index, out int total);

            if (Main.myPlayer == Projectile.owner)
            {
                bool command = Main.mouseRight && !player.mouseInterface && player.HeldItem.type == ModContent.ItemType<HielitiumStaff>();
                int current = (int)Projectile.ai[0];

                if (command && current < StateSwarm)
                {
                    Projectile.ai[0] = StateSwarm;
                    Projectile.ai[1] = 0f;
                    Projectile.localAI[0] = 6 + index * 8;
                    Projectile.localAI[1] = 0f;
                    Projectile.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.4f, Pitch = 0.1f }, Projectile.Center);
                }
                else if (!command && current >= StateSwarm)
                {
                    Projectile.ai[0] = StateReturn;
                    Projectile.ai[1] = 0f;
                    Projectile.localAI[1] = 0f;
                    Projectile.netUpdate = true;
                }
            }

            int state = (int)Projectile.ai[0];

            if (state == StateIdle)
            {
                Projectile.friendly = false;
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = Vector2.Lerp(Projectile.Center, slotPos, 0.25f);
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, slotAngle, 0.25f);

                if (Projectile.localAI[0] > 0f)
                    Projectile.localAI[0]--;

                if (Main.rand.NextBool(40))
                {
                    Vector2 p = Projectile.Center + Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(-SpikeLength * 0.4f, SpikeLength * 0.4f);
                    float size = Main.rand.NextFloat(8f, 14f);
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        p.ToNumerics(),
                        Main.rand.NextVector2Circular(0.3f, 0.3f).ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                        new Color(120, 215, 255, 180),
                        Main.rand.Next(14, 22)
                    ));
                }

                if (Main.myPlayer == Projectile.owner && Projectile.localAI[0] <= 0f)
                {
                    NPC target = FindTarget(player);
                    if (target != null)
                    {
                        Projectile.ai[0] = StateLaunch;
                        Projectile.ai[1] = target.whoAmI;
                        Projectile.localAI[1] = 0f;
                        Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 15f;
                        Projectile.netUpdate = true;
                        SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.6f, Pitch = 0.3f }, Projectile.Center);
                    }
                }
            }
            else if (state == StateLaunch)
            {
                Projectile.friendly = true;
                Projectile.localAI[1]++;

                int targetId = (int)Projectile.ai[1];
                NPC target = targetId >= 0 && targetId < Main.maxNPCs ? Main.npc[targetId] : null;

                if (target == null || !target.active || !target.CanBeChasedBy(this) || Projectile.localAI[1] > 50f)
                {
                    BeginReturn();
                }
                else
                {
                    Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 19f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.14f);
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation(), 0.5f);

                    if (Main.rand.NextBool(2))
                    {
                        float size = Main.rand.NextFloat(12f, 20f);
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            Projectile.Center.ToNumerics(),
                            (Projectile.velocity * -0.06f + Main.rand.NextVector2Circular(0.5f, 0.5f)).ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                            new Color(100, 210, 255, 200),
                            Main.rand.Next(14, 24)
                        ));
                    }
                }
            }
            else if (state == StateReturn)
            {
                Projectile.friendly = false;
                Vector2 to = slotPos - Projectile.Center;
                float len = to.Length();

                if (len < 22f)
                {
                    Projectile.ai[0] = StateIdle;
                    Projectile.localAI[0] = 18 + index * 9;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                }
                else
                {
                    Vector2 desired = to.SafeNormalize(Vector2.UnitX) * Math.Min(22f, 9f + len * 0.2f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.22f);
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation(), 0.3f);
                }
            }

            else if (state >= StateSwarm)
            {
                if (Main.myPlayer != Projectile.owner)
                {
                    Projectile.friendly = state == StateDash;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation(), 0.3f);
                }
                else
                {
                    Vector2 cursor = Main.MouseWorld;
                    Vector2 toCursor = cursor - Projectile.Center;
                    Vector2 dir = toCursor.SafeNormalize(Vector2.UnitX);
                    Projectile.friendly = state == StateDash;

                    if (state == StateSwarm)
                    {
                        float aim = (cursor - player.MountedCenter).ToRotation();
                        float t = total <= 1 ? 0.5f : index / (float)(total - 1);
                        float fan = MathHelper.Lerp(-1.25f, 1.25f, t) * Math.Min(1f, 0.35f + total * 0.12f);
                        float time = Main.GlobalTimeWrappedHourly;
                        float ang = aim + fan + (float)Math.Sin(time * 1.7f + index * 1.3f) * 0.09f;
                        float radius = 95f + (float)Math.Sin(time * 2.3f + index) * 8f;
                        Vector2 hover = player.MountedCenter + ang.ToRotationVector2() * radius;

                        Vector2 desired = (hover - Projectile.Center) * 0.25f;
                        if (desired.Length() > 28f)
                            desired = desired.SafeNormalize(Vector2.Zero) * 28f;

                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.4f);
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, dir.ToRotation(), 0.3f);

                        Projectile.localAI[0]--;
                        if (Projectile.localAI[0] <= 0f)
                        {
                            Projectile.ai[0] = StateWindup;
                            Projectile.localAI[1] = 0f;
                            Projectile.netUpdate = true;
                        }
                    }
                    else if (state == StateWindup)
                    {
                        Projectile.localAI[1]++;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, -dir * 5f, 0.3f);
                        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, dir.ToRotation(), 0.5f);

                        if (Main.rand.NextBool(2))
                        {
                            float size = Main.rand.NextFloat(8f, 14f);
                            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                                (Projectile.Center - dir * 12f).ToNumerics(),
                                Main.rand.NextVector2Circular(0.5f, 0.5f).ToNumerics(),
                                Main.rand.NextFloat(MathHelper.TwoPi),
                                new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                                new Color(140, 225, 255, 200),
                                Main.rand.Next(10, 16)
                            ));
                        }

                        if (Projectile.localAI[1] >= 12f)
                        {
                            Projectile.ai[0] = StateDash;
                            Projectile.ai[1] = MathHelper.Clamp(toCursor.Length() / 30f + 4f, 8f, 24f);
                            Projectile.localAI[1] = 0f;
                            Projectile.velocity = dir * 30f;
                            Projectile.netUpdate = true;
                            SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);

                            for (int i = 0; i < 6; i++)
                            {
                                float size = Main.rand.NextFloat(14f, 22f);
                                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                                    Projectile.Center.ToNumerics(),
                                    (-dir * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(1.5f, 1.5f)).ToNumerics(),
                                    Main.rand.NextFloat(MathHelper.TwoPi),
                                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                                    new Color(110, 215, 255, 210),
                                    Main.rand.Next(14, 22)
                                ));
                            }
                        }
                    }
                    else
                    {
                        Projectile.localAI[1]++;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 30f, 0.06f);
                        Projectile.rotation = Projectile.velocity.ToRotation();

                        if (Main.rand.NextBool(2))
                        {
                            float size = Main.rand.NextFloat(12f, 20f);
                            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                                Projectile.Center.ToNumerics(),
                                (Projectile.velocity * -0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f)).ToNumerics(),
                                Main.rand.NextFloat(MathHelper.TwoPi),
                                new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                                new Color(100, 210, 255, 200),
                                Main.rand.Next(14, 24)
                            ));
                        }

                        if (Projectile.localAI[1] >= Projectile.ai[1])
                        {
                            Projectile.ai[0] = StateSwarm;
                            Projectile.ai[1] = 0f;
                            Projectile.localAI[0] = 14 + total * 7;
                            Projectile.netUpdate = true;
                        }
                    }
                }
            }

            Lighting.AddLight(Projectile.Center, 0.25f, 0.5f, 0.7f);
        }

        private void BeginReturn()
        {
            Projectile.ai[0] = StateReturn;
            Projectile.localAI[1] = 0f;
            Projectile.netUpdate = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 150, false);

            for (int i = 0; i < 8; i++)
            {
                float size = Main.rand.NextFloat(14f, 24f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    Main.rand.NextVector2Circular(3f, 3f).ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                    new Color(100, 210, 255, 210),
                    Main.rand.Next(16, 28)
                ));
            }
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.55f, Pitch = 0.2f }, Projectile.Center);

            if (Projectile.ai[0] == StateLaunch)
            {
                Projectile.ai[0] = StateReturn;
                Projectile.localAI[1] = 0f;
                Projectile.velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitX) * 10f;
                Projectile.netUpdate = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(160, 230, 255, 180);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float drawRot = Projectile.rotation + MathHelper.PiOver2;
            float pulse = 0.75f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + Projectile.whoAmI);

            if (Projectile.ai[0] != StateIdle)
            {
                int len = Projectile.oldPos.Length;
                for (int k = 1; k < len; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero)
                        continue;

                    float fade = 1f - k / (float)len;
                    Vector2 pos = Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition;
                    Main.EntitySpriteDraw(texture, pos, null, new Color(90, 190, 255, 0) * fade * 0.4f,
                        Projectile.oldRot[k] + MathHelper.PiOver2, origin, Projectile.scale * (1f - k * 0.04f), SpriteEffects.None, 0);
                }
            }

            Color glow = new Color(100, 205, 255, 0) * 0.5f * pulse;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 2f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, drawRot, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, Color.Lerp(lightColor, Color.White, 0.6f), drawRot, origin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, new Color(140, 220, 255, 0) * 0.35f * pulse, drawRot, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}