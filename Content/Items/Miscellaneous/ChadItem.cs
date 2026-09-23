using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.NPCs.Critters;

namespace Waybound.Content.Items.Miscellaneous
{
    public class ChadItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(copper: 10);
            Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.makeNPC = (short)ModContent.NPCType<Chad>();
        }
    }
}