
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Waybound.Content.Buffs.Minions;
using Waybound.Content.Projectiles.Summon;

namespace Waybound.Content.Items.Weapons.Summon
{
    public class GoldBearer : ModItem
    {
        public override void SetDefaults()
        {
            base.Item.SetWeaponValues(78, 2f, 0);
            base.Item.useTime = (base.Item.useAnimation = 24);
            base.Item.maxStack = 1;
            base.Item.useTurn = false;
            base.Item.noMelee = true;
            base.Item.noUseGraphic = false;
            base.Item.autoReuse = false;
            base.Item.shoot = ModContent.ProjectileType<GoldBearerProj>();
            base.Item.buffType = ModContent.BuffType<GoldBearerBuff>();
            base.Item.UseSound = SoundID.Item44;
            base.Item.SetShopValues(ItemRarityColor.Yellow8, Item.sellPrice(0, 8, 5, 0));
            base.Item.DamageType = DamageClass.Summon;
            base.Item.useStyle = 1;
        }

        // Token: 0x060008BB RID: 2235 RVA: 0x000418EC File Offset: 0x0003FAEC
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, Main.myPlayer, 0f, 0f, 0f).originalDamage = base.Item.damage;
            return false;
        }
    }
}