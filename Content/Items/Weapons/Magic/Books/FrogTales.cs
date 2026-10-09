using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Magic.Books
{
    public class FrogTales : ModItem
    {
        private const int MaxFlies = 12;
        private int casts;
        // i think i grab the idea from spirit mod ive seen something like this
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.damage = 11;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 7;
            Item.knockBack = 1.5f;
            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FrogTalesProj>();
            Item.shootSpeed = 7f;
            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item8 with { Volume = 0.6f, Pitch = 0.45f };
        }

        public override void HoldItem(Player player)
        {
            if (Main.netMode == NetmodeID.Server) return;

            Lighting.AddLight(player.Center, new Vector3(0.1f, 0.25f, 0.08f));

            if (Main.rand.NextBool(45))
            {
                Dust dust = Dust.NewDustPerfect(
                    player.Center + new Vector2(player.direction * 14f, -4f) + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.GreenTorch, new Vector2(0f, -0.6f), 100, default, 0.8f);
                dust.noGravity = true;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
            Vector2 muzzle = position + dir * 20f;
            if (!Collision.CanHit(player.Center, 1, 1, muzzle, 1, 1))
                muzzle = player.Center;

            TrimFlies(player.whoAmI, type, 3);

            casts++;
            bool fat = casts % 3 == 0;
            float speed = velocity.Length();

            for (int i = 0; i < 2; i++)
            {
                float off = (i - 0.5f) * 0.55f;
                Vector2 v = dir.RotatedBy(off).RotatedByRandom(0.1f) * speed * Main.rand.NextFloat(0.85f, 1.1f);
                Projectile.NewProjectile(source, muzzle, v, type, damage, knockback, player.whoAmI, 0f, 0f);
            }

            if (fat)
            {
                Projectile.NewProjectile(source, muzzle, dir * speed * 0.8f, type, (int)(damage * 1.8f), knockback * 1.5f, player.whoAmI, 0f, 1f);
                SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.7f, Pitch = -0.2f }, muzzle);
            }

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(muzzle, i % 2 == 0 ? DustID.GreenTorch : DustID.Grass,
                    dir.RotatedByRandom(0.7f) * Main.rand.NextFloat(1.5f, 4.5f), 100, default, Main.rand.NextFloat(0.9f, 1.3f));
                dust.noGravity = true;
            }

            return false;
        }

        private static void TrimFlies(int owner, int type, int incoming)
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner && p.type == type) count++;
            }

            while (count + incoming > MaxFlies)
            {
                Projectile oldest = null;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active || p.owner != owner || p.type != type) continue;
                    if (oldest == null || p.timeLeft < oldest.timeLeft) oldest = p;
                }

                if (oldest == null) break;
                oldest.Kill();
                count--;
            }
        }
    }

    public class FrogTalesProj : ModProjectile
    {
        private static readonly Color Venom = new Color(120, 255, 120);

        private bool Fat => Projectile.ai[1] == 1f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 360;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            bool fat = Fat;

            if (Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[1] = 1f;
                Projectile.localAI[0] = Main.rand.NextFloat(MathHelper.TwoPi);
                Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;

                if (fat)
                {
                    Projectile.Resize(26, 26);
                    Projectile.penetrate = 6;
                }
            }

            Projectile.scale = fat ? 1.5f : 1f;
            Projectile.ai[0]++;
            float age = Projectile.ai[0];
            float phase = Projectile.localAI[0];

            Projectile.Opacity = MathHelper.Clamp(Math.Min(age / 6f, Projectile.timeLeft / 20f), 0f, 1f);

            if (++Projectile.frameCounter >= 2)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 3;
            }

            if (Projectile.owner == Main.myPlayer)
            {
                if (Projectile.ai[2] > 0f)
                {
                    Projectile.ai[2]--;
                    Projectile.velocity *= 0.96f;
                }
                else
                {
                    Steer(owner, fat, age, phase);
                }

                if (age % 5 == 0)
                    Projectile.netUpdate = true;
            }

            if (Math.Abs(Projectile.velocity.X) > 0.4f)
                Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;

            float tilt = Projectile.spriteDirection == 1
                ? Projectile.velocity.ToRotation()
                : MathHelper.WrapAngle(Projectile.velocity.ToRotation() + MathHelper.Pi);
            tilt = MathHelper.Clamp(tilt, -0.9f, 0.9f);
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, tilt, 0.2f);

            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(fat ? 4 : 10))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                        DustID.GreenTorch, Main.rand.NextVector2Circular(0.4f, 0.4f), 100, default, fat ? 1f : 0.7f);
                    dust.noGravity = true;
                }

                Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.3f, 0.08f) * (fat ? 1.6f : 1f));
            }
        }

        private void Steer(Player owner, bool fat, float age, float phase)
        {
            float maxSpeed = fat ? 8f : 10.5f;
            NPC target = FindTarget(fat ? 520f : 420f);

            if (target != null)
            {
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Vector2 side = new Vector2(-dir.Y, dir.X) * MathF.Sin(age * 0.4f + phase) * 3.2f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * maxSpeed + side, 0.1f);
                return;
            }

            Vector2 delta = Main.MouseWorld - owner.Center;
            if (delta.Length() > 800f)
                delta = delta.SafeNormalize(Vector2.UnitX) * 800f;

            Vector2 anchor = owner.Center + delta + new Vector2(
                MathF.Cos(age * 0.07f + phase) * 50f,
                MathF.Sin(age * 0.11f + phase * 1.7f) * 30f);

            Vector2 toAnchor = anchor - Projectile.Center;
            Vector2 desired = toAnchor.SafeNormalize(Vector2.Zero) * Math.Min(toAnchor.Length() * 0.12f, maxSpeed);
            desired += Main.rand.NextVector2Circular(1.2f, 1.2f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.1f);
        }

        private NPC FindTarget(float range)
        {
            NPC best = null;
            float bestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.CanBeChasedBy(Projectile)) continue;

                float d = Vector2.Distance(Projectile.Center, n.Center);
                if (d >= bestDist) continue;
                if (!Collision.CanHit(Projectile.Center, 1, 1, n.position, n.width, n.height)) continue;

                bestDist = d;
                best = n;
            }

            return best;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, Fat ? 300 : 180);

            Projectile.ai[2] = 12f;
            Projectile.velocity = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitY) * 6f + Main.rand.NextVector2Circular(2f, 2f);
            Projectile.netUpdate = true;

            if (Main.netMode == NetmodeID.Server) return;

            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 2 == 0 ? DustID.GreenTorch : DustID.Grass,
                    Main.rand.NextVector2Circular(3f, 3f), 100, default, Main.rand.NextFloat(0.9f, 1.3f));
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;

            for (int i = 0; i < (Fat ? 12 : 6); i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 100, default, Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, 3, 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;
            SpriteEffects fx = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Color light = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16) * Projectile.Opacity;
            Color trail = Venom with { A = 0 };

            for (int i = Projectile.oldPos.Length - 1; i >= 1; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float k = i / (float)Projectile.oldPos.Length;
                Main.EntitySpriteDraw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, frame,
                    trail * ((1f - k) * 0.25f * Projectile.Opacity), Projectile.oldRot[i], origin, Projectile.scale, fx, 0);
            }

            if (Fat)
            {
                float time = Main.GlobalTimeWrappedHourly;
                float pulse = 0.7f + 0.3f * MathF.Sin(time * 8f);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 off = new Vector2(2.5f, 0f).RotatedBy(MathHelper.PiOver2 * i + time * 3f);
                    Main.EntitySpriteDraw(tex, pos + off, frame, trail * (0.45f * pulse * Projectile.Opacity), Projectile.rotation, origin, Projectile.scale * 1.05f, fx, 0);
                }
            }

            Main.EntitySpriteDraw(tex, pos, frame, light, Projectile.rotation, origin, Projectile.scale, fx, 0);

            return false;
        }
    }
}