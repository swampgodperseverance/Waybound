using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Waybound.Content.Dusts;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Accessories.Hardmode
{
    public class Jeweler : ModItem
    {
        public const int ProjectileLifeTime = 30;
        public const float DamageReduction = 0.2f;
        public const float MagicDamageBonus = 1.07f;
        public const float SummonDamageBonus = 1.07f;
        public const int SpawnCooldown = 30;
        public const float SpawnRadius = 80f;

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += MagicDamageBonus - 0.10f;
            player.GetDamage(DamageClass.Summon) += SummonDamageBonus - 0.10f;

            var jevelerPlayer = player.GetModPlayer<JewelerPlayer>();
            jevelerPlayer.equippedJeveler = true;

            if (jevelerPlayer.spawnTimer <= 0)
            {
                SpawnProtectiveProjectile(player);
                jevelerPlayer.spawnTimer = SpawnCooldown;
            }
            else
            {
                jevelerPlayer.spawnTimer--;
            }
        }

        private void SpawnProtectiveProjectile(Player player)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * SpawnRadius;
            Vector2 position = player.Center + offset;

            Projectile.NewProjectile(player.GetSource_Accessory(Item),
                position,
                Vector2.Zero,
                ModContent.ProjectileType<JewelerProj>(),
                0,
                0f,
                player.whoAmI);
        }
    }

    public class JewelerPlayer : ModPlayer
    {
        public bool equippedJeveler;
        public int spawnTimer;

        public override void ResetEffects()
        {
            equippedJeveler = false;
        }
    }

    public class JewelerProj : ModProjectile
    {
        private int lifeTimer = 0;
        private const int MaxLifeTime = 30;
        private float rotationAngle = 0f;
        private const float RotationSpeed = 0.05f;
        private Vector2 followOffset;
        private bool offsetInitialized = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.timeLeft = MaxLifeTime + 1;
            Projectile.penetrate = -1;
            Projectile.netImportant = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            lifeTimer++;

            int frameSpeed = 5;
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= frameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            rotationAngle += RotationSpeed;
            Projectile.rotation = rotationAngle;

            float alphaValue;

            if (lifeTimer <= MaxLifeTime / 2)
            {
                float progress = (float)lifeTimer / (MaxLifeTime / 2);
                alphaValue = MathHelper.Lerp(0f, 1f, progress);
            }
            else
            {
                float progress = (float)(lifeTimer - MaxLifeTime / 2) / (MaxLifeTime / 2);
                alphaValue = MathHelper.Lerp(1f, 0f, progress);
            }

            Projectile.alpha = (int)MathHelper.Lerp(255, 0, alphaValue);

            if (lifeTimer == 3 && alphaValue > 0.1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                        ModContent.DustType<CruorDust>(), 0, 0, 100, default, 0.6f);
                }
            }

            if (lifeTimer == MaxLifeTime - 3 && alphaValue < 0.9f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                        ModContent.DustType<CruorDust>(), 0, 0, 100, default, 0.6f);
                    dust.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 1f;
                }
            }

            Player owner = Main.player[Projectile.owner];
            if (owner.active && !owner.dead)
            {
                if (!offsetInitialized)
                {
                    followOffset = Projectile.Center - owner.Center;
                    offsetInitialized = true;
                }

                Vector2 targetCenter = owner.Center + followOffset;
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetCenter, 0.3f);
            }

            if (lifeTimer >= MaxLifeTime)
            {
                Projectile.Kill();
            }
        }

        public override void PostDraw(Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
            Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            Color outlineColor = new Color((byte)139, (byte)0, (byte)0, (byte)(Projectile.alpha / 2));

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.5f, 0).RotatedBy(i * MathHelper.PiOver2);
                spriteBatch.Draw(texture, position + offset, frame, outlineColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(texture, position, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        }
    }

    public class JewelerGlobalProjectile : GlobalProjectile
    {
        public override bool CanHitPlayer(Projectile projectile, Player target)
        {
            if (projectile.hostile && !projectile.friendly)
            {
                var jevelerPlayer = target.GetModPlayer<JewelerPlayer>();

                if (jevelerPlayer.equippedJeveler)
                {
                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.active && proj.type == ModContent.ProjectileType<JewelerProj>() &&
                            proj.owner == target.whoAmI && projectile.Hitbox.Intersects(proj.Hitbox))
                        {
                            return true;
                        }
                    }
                }
            }

            return base.CanHitPlayer(projectile, target);
        }

        public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo hurtInfo)
        {
            var jevelerPlayer = target.GetModPlayer<JewelerPlayer>();

            if (jevelerPlayer.equippedJeveler)
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<JewelerProj>() &&
                        proj.owner == target.whoAmI && projectile.Hitbox.Intersects(proj.Hitbox))
                    {
                        hurtInfo.Damage = (int)(hurtInfo.Damage * (1f - Jeweler.DamageReduction));
                        break;
                    }
                }
            }
        }
    }
}