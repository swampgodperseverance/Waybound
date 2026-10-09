using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Waybound.Content.Items.Accessories.Hardmode;
using Waybound.Content.Items.Weapons.Ranged.LaserGuns.GemLaserGuns;

namespace Waybound.Content.Items.Accessories.PreHardmode
{
    public class MegaBullet : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<MBPlayer>().equipped = true;
        }

        public override void PostUpdate()
        {
            float pulse = 0.75f + 0.25f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + Item.whoAmI);
            Lighting.AddLight(Item.Center, new Vector3(0.5f, 0.38f, 0.1f) * 0.35f * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.7f + 0.3f * MathF.Sin(time * 3f + whoAmI);
            Color glow = new Color(255, 205, 100, 0);

            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(2f, 0f).RotatedBy(time * 1.2f + MathHelper.PiOver2 * i);
                spriteBatch.Draw(texture, position + off, null, glow * (0.22f * pulse), rotation, origin, scale * 1.03f, SpriteEffects.None, 0f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.IronBar, 8)
                .AddIngredient(ItemID.MusketBall, 100)
                .AddIngredient(ItemID.Lens, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class MBPlayer : ModPlayer
    {
        public bool equipped;
        public bool deathbringer; //new accessory
        public int shotCount;
        public int chain;
        public bool forceNextMega;
        public int frenzy;

        private uint lastShotTick = uint.MaxValue;
        private uint lastMarkChargeTick = uint.MaxValue;
        private bool lastShotMega;
        private bool wasReady;

        public int ShotsNeeded => deathbringer ? 4 : 5;
        public int MaxChain => deathbringer ? 6 : 3;
        public bool Ready => equipped && (forceNextMega || shotCount >= ShotsNeeded - 1);

        public override void ResetEffects()
        {
            equipped = false;
            deathbringer = false;
        }

        public override void PostUpdateEquips()
        {
            if (deathbringer && frenzy > 0)
                Player.GetAttackSpeed(DamageClass.Ranged) += 0.2f;
        }

        public override void PostUpdate()
        {
            if (!equipped)
            {
                shotCount = 0;
                chain = 0;
                frenzy = 0;
                forceNextMega = false;
                wasReady = false;
                return;
            }

            if (frenzy > 0)
                frenzy--;

            bool ready = Ready;

            if (ready && !wasReady && Player.whoAmI == Main.myPlayer)
                SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.6f, Pitch = deathbringer ? -0.2f : 0.3f }, Player.Center);
            wasReady = ready;

            if (Main.netMode == NetmodeID.Server) return;

            int dustType = deathbringer ? DustID.RedTorch : DustID.GoldFlame;

            if (frenzy > 0 && Main.rand.NextBool(3))
            {
                Dust aura = Dust.NewDustPerfect(
                    Player.Center + Main.rand.NextVector2Circular(Player.width * 0.6f, Player.height * 0.6f),
                    dustType, new Vector2(0f, -Main.rand.NextFloat(1f, 2.5f)), 100, default, Main.rand.NextFloat(1f, 1.5f));
                aura.noGravity = true;
            }

            if (!ready) return;

            if (Main.GameUpdateCount % 4 == 0)
            {
                float time = Main.GlobalTimeWrappedHourly;
                int count = deathbringer ? 4 : 3;
                float radius = deathbringer ? 36f : 30f;
                for (int k = 0; k < count; k++)
                {
                    Vector2 pos = Player.Center + Vector2.UnitX.RotatedBy(time * 6f + MathHelper.TwoPi * k / count) * radius;
                    Dust dust = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 100, default, 0.9f);
                    dust.noGravity = true;
                }
            }

            Vector3 light = deathbringer ? new Vector3(0.7f, 0.12f, 0.2f) : new Vector3(0.5f, 0.38f, 0.1f);
            Lighting.AddLight(Player.Center, light * 0.6f);
        }

        public bool RegisterShot()
        {
            if (lastShotTick == Main.GameUpdateCount)
                return lastShotMega;

            lastShotTick = Main.GameUpdateCount;
            bool mega;

            if (forceNextMega)
            {
                forceNextMega = false;
                mega = true;
            }
            else
            {
                shotCount++;
                if (shotCount >= ShotsNeeded)
                {
                    shotCount = 0;
                    chain = 0;
                    mega = true;
                }
                else
                {
                    mega = false;
                }
            }

            if (mega && deathbringer)
                frenzy = 150;

            lastShotMega = mega;
            return mega;
        }

        public bool TryRefund()
        {
            if (chain >= MaxChain) return false;

            chain++;
            forceNextMega = true;
            return true;
        }

        public bool TryMarkCharge()
        {
            if (!deathbringer || lastMarkChargeTick == Main.GameUpdateCount) return false;
            lastMarkChargeTick = Main.GameUpdateCount;

            if (forceNextMega || shotCount >= ShotsNeeded - 1) return false;

            shotCount++;
            return true;
        }
    }

    public class MBGP : GlobalProjectile
    {
        private static readonly Color Gold = new Color(255, 205, 100);
        private static readonly Color Blood = new Color(235, 45, 70);

        public bool mega;
        public bool deathbringer;
        private bool fxDone;

        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation
                && entity.friendly
                && !entity.hostile
                && !entity.minion
                && !entity.sentry
                && entity.CountsAsClass(DamageClass.Ranged);
        }

        private static bool IsShotSource(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_ItemUse_WithAmmo)
                return true;

            if (source is EntitySource_Parent parent && parent.Entity is Projectile parentProj)
            {
                Player owner = Main.player[projectile.owner];
                return parentProj.owner == projectile.owner && owner.heldProj == parentProj.whoAmI;
            }

            return false;
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (projectile.owner != Main.myPlayer) return;
            if (projectile.type == ModContent.ProjectileType<MBBlast>()) return;
            if (projectile.type == ModContent.ProjectileType<DeathbringerShard>()) return;
            if (projectile.ModProjectile is RangedLaser) return;
            if (!IsShotSource(projectile, source)) return;

            Player player = Main.player[projectile.owner];
            MBPlayer mp = player.GetModPlayer<MBPlayer>();
            if (!mp.equipped) return;

            if (!mp.RegisterShot()) return;

            mega = true;
            deathbringer = mp.deathbringer;

            if (projectile.damage > 0)
                projectile.damage = (int)(projectile.damage * 1.5f);

            if (projectile.penetrate > 0)
                projectile.penetrate += deathbringer ? 1 : 0;

            projectile.CritChance += 25;
            projectile.netUpdate = true;
        }

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            bitWriter.WriteBit(mega);
            bitWriter.WriteBit(deathbringer);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            mega = bitReader.ReadBit();
            deathbringer = bitReader.ReadBit();
        }

        public override void PostAI(Projectile projectile)
        {
            if (!mega || Main.netMode == NetmodeID.Server) return;

            int dustType = deathbringer ? DustID.RedTorch : DustID.GoldFlame;

            if (!fxDone)
            {
                fxDone = true;
                SoundEngine.PlaySound(SoundID.Item38 with { Volume = 0.8f, Pitch = deathbringer ? -0.5f : -0.25f }, projectile.Center);

                Vector2 dir = projectile.velocity.SafeNormalize(Vector2.UnitX);
                for (int i = 0; i < (deathbringer ? 16 : 10); i++)
                {
                    Dust spark = Dust.NewDustPerfect(projectile.Center, dustType,
                        dir.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 9f), 100, default, Main.rand.NextFloat(1.1f, 1.6f));
                    spark.noGravity = true;
                }
            }

            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustPerfect(
                    projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    deathbringer ? (Main.rand.NextBool(3) ? DustID.Smoke : DustID.RedTorch) : (Main.rand.NextBool() ? DustID.GoldFlame : DustID.Sand),
                    -projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    100, default, Main.rand.NextFloat(0.9f, 1.3f));
                trail.noGravity = true;
            }

            Vector3 light = deathbringer ? new Vector3(0.75f, 0.12f, 0.2f) : new Vector3(0.6f, 0.45f, 0.12f);
            Lighting.AddLight(projectile.Center, light);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (!mega) return true;

            Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() / 2f;
            Vector2 pos = projectile.Center - Main.screenPosition;
            SpriteEffects fx = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.8f + 0.2f * MathF.Sin(time * 14f);
            Color glow = (deathbringer ? Blood : Gold) with { A = 0 };

            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(deathbringer ? 3.2f : 2.5f, 0f).RotatedBy(MathHelper.PiOver2 * i + time * 5f);
                Main.EntitySpriteDraw(tex, pos + off, frame, glow * (0.5f * pulse), projectile.rotation, origin, projectile.scale * 1.08f, fx, 0);
            }

            Main.EntitySpriteDraw(tex, pos, frame, glow * 0.35f, projectile.rotation, origin, projectile.scale * 1.3f, fx, 0);

            if (deathbringer)
                Main.EntitySpriteDraw(tex, pos, frame, (Color.White with { A = 0 }) * (0.25f * pulse), projectile.rotation, origin, projectile.scale * 1.05f, fx, 0);

            return true;
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!mega || !deathbringer) return;

            modifiers.CritDamage += 0.1f;

            if (target.life < target.lifeMax * 0.3f)
                modifiers.FinalDamage *= 1.08f;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.owner != Main.myPlayer) return;

            MBPlayer mp = Main.player[projectile.owner].GetModPlayer<MBPlayer>();

            if (!mega)
            {
                if (mp.deathbringer && target.GetGlobalNPC<DeathbringerNPC>().markTimer > 0)
                    mp.TryMarkCharge();
                return;
            }

            if (deathbringer)
                target.GetGlobalNPC<DeathbringerNPC>().Mark(projectile.owner);

            int blastDamage = Math.Max(1, (int)(projectile.damage * (deathbringer ? 0.8f : 0.6f)));
            Projectile.NewProjectile(projectile.GetSource_OnHit(target), target.Center, Vector2.Zero,
                ModContent.ProjectileType<MBBlast>(), blastDamage, 0f, projectile.owner, target.whoAmI, deathbringer ? 1f : 0f);

            if (target.life > 0 || target.lifeMax <= 5 || target.friendly) return;

            if (deathbringer)
            {
                int shardDamage = Math.Max(1, (int)(projectile.damage * 0.35f));
                for (int i = 0; i < 3; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(7f, 11f);
                    Projectile.NewProjectile(projectile.GetSource_OnHit(target), target.Center, vel,
                        ModContent.ProjectileType<DeathbringerShard>(), shardDamage, 0f, projectile.owner);
                }
            }

            if (!mp.equipped || !mp.TryRefund()) return;

            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.7f, Pitch = deathbringer ? 0f : 0.4f }, target.Center);
            for (int i = 0; i < 14; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center, deathbringer ? DustID.RedTorch : DustID.GoldFlame,
                    Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 7f), 80, default, Main.rand.NextFloat(1.1f, 1.6f));
                dust.noGravity = true;
            }
        }
    }

    public class MBBlast : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 6;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[1] == 1f)
                Projectile.Resize(240, 240);
        }

        public override bool? CanHitNPC(NPC target)
        {
            if ((int)Projectile.ai[0] == target.whoAmI)
                return false;

            return null;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] != 0f) return;
            Projectile.localAI[0] = 1f;

            if (Main.netMode == NetmodeID.Server) return;

            bool db = Projectile.ai[1] == 1f;
            int dustType = db ? DustID.RedTorch : DustID.GoldFlame;
            int sparks = db ? 40 : 26;

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = db ? 0.8f : 0.55f, Pitch = db ? -0.1f : 0.35f }, Projectile.Center);

            for (int i = 0; i < sparks; i++)
            {
                Vector2 dir = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / sparks);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, dustType,
                    dir * Main.rand.NextFloat(3f, db ? 12f : 9f), 80, default, Main.rand.NextFloat(1.2f, 1.8f));
                spark.noGravity = true;
            }

            for (int i = 0; i < 12; i++)
            {
                Dust sand = Dust.NewDustPerfect(Projectile.Center, db ? DustID.Torch : DustID.Sand,
                    Main.rand.NextVector2Circular(5f, 5f), 80, default, Main.rand.NextFloat(1f, 1.5f));
                sand.noGravity = true;
            }

            for (int i = 0; i < 6; i++)
            {
                Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), DustID.Smoke,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), db ? 200 : 140, default, Main.rand.NextFloat(1.2f, 1.8f));
                smoke.noGravity = true;
            }

            Vector3 light = db ? new Vector3(1f, 0.2f, 0.3f) : new Vector3(1f, 0.75f, 0.2f);
            Lighting.AddLight(Projectile.Center, light);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}