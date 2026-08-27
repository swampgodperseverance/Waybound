
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Accessories.Hardmode
{
    public class MortalStones : ModItem
    {
        public const int ArmorPenetration = 8; //That was the worst thing that ive ever do lol

        public override void SetStaticDefaults()
        {
            
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(gold: 4);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.noKnockback = true;
            player.GetDamage(DamageClass.Melee) += 0.10f;
            player.GetArmorPenetration(DamageClass.Generic) += ArmorPenetration;
        }
    }
}