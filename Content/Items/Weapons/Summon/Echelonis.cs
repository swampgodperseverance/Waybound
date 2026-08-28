using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Waybound.Common.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Minions;


namespace Waybound.Content.Items.Weapons.Summon
{
    public class Echelonis : ModItem
    {
        private static readonly int[] PositiveAccessories = new int[]
        {
            ItemID.BandofRegeneration,
            ItemID.CrossNecklace,
        };

        private static readonly int[] NegativeAccessories = new int[]
        {
            ItemID.DestroyerEmblem,
            ItemID.PanicNecklace
        };

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 41;
            Item.knockBack = 2f;
            Item.mana = 10;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 1;
            Item.noUseGraphic = false;
            Item.staff[Item.type] = true;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<ProdigyBuff>();
            Item.shoot = ModContent.ProjectileType<Projectiles.Summon.ProdigalSeraph>();
            Item.shootSpeed = 0f;

            Item.rare = ModContent.RarityType<CoreburnedRarity>();
            Item.value = Item.buyPrice(gold: 1, silver: 50);
        }

        private bool HasConflict(Player player)
        {
            bool hasPositive = false;
            bool hasNegative = false;

            for (int i = 3; i < 10; i++)
            {
                Item acc = player.armor[i];
                if (acc.IsAir) continue;

                if (Array.Exists(PositiveAccessories, type => type == acc.type))
                    hasPositive = true;

                if (Array.Exists(NegativeAccessories, type => type == acc.type))
                    hasNegative = true;
            }

            return hasPositive && hasNegative;
        }

        private int GetPositiveCount(Player player)
        {
            int count = 0;
            for (int i = 3; i < 10; i++)
            {
                Item acc = player.armor[i];
                if (!acc.IsAir && Array.Exists(PositiveAccessories, type => type == acc.type))
                    count++;
            }
            return count;
        }

        private int GetNegativeCount(Player player)
        {
            int count = 0;
            for (int i = 3; i < 10; i++)
            {
                Item acc = player.armor[i];
                if (!acc.IsAir && Array.Exists(NegativeAccessories, type => type == acc.type))
                    count++;
            }
            return count;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (HasConflict(player))
            {
                damage *= 0f;
                return;
            }

            int posCount = GetPositiveCount(player);
            int negCount = GetNegativeCount(player);
            if (posCount > 0)
            {
                float bonus = posCount switch
                {
                    1 => 0.10f,
                    2 => 0.14f,
                    3 => 0.18f,
                    4 => 0.22f,
                    _ => 0.25f
                };
                damage *= 1f + bonus;
            }
            if (negCount > 0)
            {
                float penalty = Math.Min(negCount * 0.10f, 0.50f); 
                damage *= 1f - penalty;
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Player player = Main.LocalPlayer;
            if (HasConflict(player))
            {
                float time = Main.GameUpdateCount * 1.2f;
                float shakeX = (float)Math.Sin(time * 1.1f) * 0.7f + (float)Math.Sin(time * 4.3f) * 0.4f;
                float shakeY = (float)Math.Sin(time * 1.7f) * 0.8f + (float)Math.Sin(time * 3.1f) * 0.35f;
                Vector2 offset = new Vector2(shakeX, shakeY);

                Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
                spriteBatch.Draw(texture, position + offset, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (HasConflict(player))
            {
                player.Hurt(PlayerDeathReason.ByCustomReason("Echelonis Curse..."), 50, 0, false, false, -1);
                return false;
            }

            player.AddBuff(Item.buffType, 3600, quiet: false);

            var spawnPos = Main.MouseWorld;
            if (Main.myPlayer == player.whoAmI)
            {
                Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }

    public class EchelonisAccessoryGlow : GlobalItem
    {
        private static readonly int[] PositiveAccessories =
        {
            ItemID.BandofRegeneration,
            ItemID.CrossNecklace,
        };

        private static readonly int[] NegativeAccessories =
        {
            ItemID.DestroyerEmblem,
            ItemID.PanicNecklace
        };

        public override bool InstancePerEntity => false;

        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Player player = Main.LocalPlayer;
            if (player.HeldItem.type != ModContent.ItemType<Echelonis>())
                return;

            bool isPositive = Array.Exists(PositiveAccessories, t => t == item.type);
            bool isNegative = Array.Exists(NegativeAccessories, t => t == item.type);

            if (!isPositive && !isNegative)
                return;

            Color outlineColor = isPositive
                ? new Color(255, 255, 255, 130)
                : new Color(100, 100, 100, 160);

            Texture2D texture = TextureAssets.Item[item.type].Value;

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.0f, 0f).RotatedBy(i * MathHelper.PiOver2) * 0.8f;
                spriteBatch.Draw(texture, position + offset, frame, outlineColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }
    }
}