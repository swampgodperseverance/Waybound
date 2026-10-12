using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Weapons.Ranged.Guns.PreHM;

namespace Waybound.Content.Items.Accessories.Hardmode
{//Simple accessory for the war guns
    public class ExtendedMagazine : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            WPlayer rp = player.GetModPlayer<WPlayer>();
            rp.MagMult += 0.5f;
            rp.SweepMult *= 0.85f;
            rp.LastRounds = true;
        }
    }
}