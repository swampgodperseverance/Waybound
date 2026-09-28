using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Waybound.Content.Items.Accessories.Shields;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Items.Vanity.BossMasks;

namespace Waybound.Content.Items.Bags
{
	public class ThemisBag : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("Right click to open");
			// DisplayName.SetDefault("Treasure Bag");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.maxStack = 999;
			Item.consumable = true;
			Item.width = 24;
			Item.height = 24;
			Item.rare = -12;
			Item.expert = true;
		}
		public override bool CanRightClick()
		{
			return true;
		}

		public override void RightClick(Player player)
		{
			var source = player.GetSource_OpenItem(Type);

			player.QuickSpawnItem(source, ModContent.ItemType<DesertCore>(), Main.rand.Next(5, 9));
			player.QuickSpawnItem(source, ModContent.ItemType<DesertWreckage>(), Main.rand.Next(35, 45));

			if (Main.rand.Next(7) == 0)
			{
				player.QuickSpawnItem(source, ModContent.ItemType<ThemisMask>());
			}

			player.QuickSpawnItem(source, ModContent.ItemType<ShieldOfDesertHunter>());

		}
		// public override int BossBagNPC => ModContent.NPCType<NPCs.Bosses.Themis.themis>();
	}
}