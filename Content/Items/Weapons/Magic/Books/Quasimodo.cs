using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Magic.Books;

namespace Waybound.Content.Items.Weapons.Magic.Books
{
    public class Quasimodo : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.holdStyle = 0;
            Item.noUseGraphic = true;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.autoReuse = false;
            Item.DamageType = DamageClass.Magic;
            Item.damage = 44;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(0, 10);
            Item.rare = ItemRarityID.Pink;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<QuasimodoProj>();
            Item.shootSpeed = 1f;
            Item.mana = 6;
        }

        public override void HoldItem(Player player)
        {
            var modPlayer = player.GetModPlayer<QuasimodoPlayer>();

            Vector2 bookWorldPos = player.Center + new Vector2(player.direction * 36f, -18f);
            Vector2 shoulder = player.Center + new Vector2(player.direction * 8f, -4f);
            float armAngle = (bookWorldPos - shoulder).ToRotation();

            float sway = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.6f) * 0.045f;
            float shootSwing = MathHelper.SmoothStep(0f, 0.18f, modPlayer.armShootSwing);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armAngle - MathHelper.PiOver2 + sway + shootSwing);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armAngle - MathHelper.PiOver2 - sway * 0.7f - shootSwing * 0.55f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer != player.whoAmI)
                return false;

            Vector2 spawnPos = player.Center + new Vector2(player.direction * 36f, -18f);
            Vector2 baseDir = Vector2.Normalize(Main.MouseWorld - spawnPos);

            int count = 5;
            float totalSpread = MathHelper.ToRadians(48f);

            for (int i = 0; i < count; i++)
            {
                float offset = MathHelper.Lerp(-totalSpread / 2f, totalSpread / 2f, i / (float)(count - 1));
                Vector2 shootVel = baseDir.RotatedBy(offset) * 4.5f;

                int proj = Projectile.NewProjectile(source, spawnPos, shootVel, type, damage, knockback, player.whoAmI, -i * 2f);
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
            }

            var mp = player.GetModPlayer<QuasimodoPlayer>();
            mp.armShootSwing = 1f;
            mp.glowPulse = 1f;

            return false;
        }
    }

    public class QuasimodoPlayer : ModPlayer
    {
        public float glowPulse;
        public float bookFade;
        public float bookRotation;
        public float armShootSwing;

        public override void PostUpdate()
        {
            if (armShootSwing > 0f)
                armShootSwing = MathHelper.Clamp(armShootSwing - 0.055f, 0f, 1f);

            if (glowPulse > 0f)
                glowPulse = MathHelper.Clamp(glowPulse - 0.04f, 0f, 1f);

            bool isHolding = Player.HeldItem.type == ModContent.ItemType<Quasimodo>();

            if (isHolding)
                bookFade = MathHelper.Clamp(bookFade + 0.07f, 0f, 1f);
            else
                bookFade = MathHelper.Clamp(bookFade - 0.11f, 0f, 1f);
        }
    }

    public class QuasimodoBookLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.HeldItem.type == ModContent.ItemType<Quasimodo>()
                && !drawInfo.drawPlayer.dead
                && !drawInfo.drawPlayer.frozen;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<QuasimodoPlayer>();

            if (modPlayer.bookFade <= 0.01f)
                return;

            Texture2D texture = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Magic/Books/Quasimodo", AssetRequestMode.ImmediateLoad).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Magic/Books/Quasimodo_Glow", AssetRequestMode.ImmediateLoad).Value;

            Vector2 bookWorld = player.Center + new Vector2(player.direction * 36f, -18f);
            Vector2 bookPos = bookWorld - Main.screenPosition;
            bookPos = bookPos.Floor();

            float time = Main.GlobalTimeWrappedHourly;
            float breathe = (float)Math.Sin(time * 2.6f) * 0.5f + 0.5f;
            float scale = 1f + breathe * 0.05f;
            bookPos.Y += (float)Math.Sin(time * 2.6f) * 1.6f;

            float targetRot = 0f;
            SpriteEffects effects = SpriteEffects.None;

            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 toMouse = Main.MouseWorld - bookWorld;
                float mouseAngle = toMouse.ToRotation();

                if (player.direction == 1)
                {
                    float relative = MathHelper.WrapAngle(mouseAngle);
                    relative = MathHelper.Clamp(relative, MathHelper.ToRadians(-50f), MathHelper.ToRadians(50f));
                    targetRot = relative;
                    effects = SpriteEffects.None;
                }
                else
                {
                    float relative = MathHelper.WrapAngle(mouseAngle - MathHelper.Pi);
                    relative = MathHelper.Clamp(relative, MathHelper.ToRadians(-50f), MathHelper.ToRadians(50f));
                    targetRot = relative;                
                    effects = SpriteEffects.FlipHorizontally;
                }
            }
            else
            {
                targetRot = 0f;
                effects = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            modPlayer.bookRotation = MathHelper.Lerp(modPlayer.bookRotation, targetRot, 0.12f);

            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Size() * 0.5f;
            float fade = modPlayer.bookFade;

            float outlinePulse = 0.6f + (float)Math.Sin(time * 3.2f) * 0.2f;
            Color outlineColor = new Color(170, 90, 255) * (0.18f * outlinePulse * fade);

            for (int i = 0; i < 3; i++)
            {
                float outlineScale = scale * (1.05f + i * 0.04f);
                float outlineAlpha = 1f - i * 0.3f;

                drawInfo.DrawDataCache.Add(new DrawData(
                    texture,
                    bookPos,
                    frame,
                    outlineColor * outlineAlpha,
                    modPlayer.bookRotation,
                    origin,
                    outlineScale,
                    effects,
                    0
                ));
            }

            drawInfo.DrawDataCache.Add(new DrawData(
                texture,
                bookPos,
                frame,
                Color.White * fade,
                modPlayer.bookRotation,
                origin,
                scale,
                effects,
                0
            ));
                
            float pulse = modPlayer.glowPulse;
            float glowStrength = (0.32f + pulse * 0.8f) * fade;
            Color glowColor = new Color(255, 160, 210) * glowStrength;

            for (int i = 0; i < 3; i++)
            {
                float layerScale = scale * (1.07f + i * 0.055f + pulse * 0.1f);
                float layerAlpha = glowStrength * (1f - i * 0.28f);

                drawInfo.DrawDataCache.Add(new DrawData(
                    glowTex,
                    bookPos,
                    frame,
                    glowColor * layerAlpha,
                    modPlayer.bookRotation,
                    origin,
                    layerScale,
                    effects,
                    0
                ));
            }
        }   
    }
}