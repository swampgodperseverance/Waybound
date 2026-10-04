using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Waybound.Content.Items.Accessories.PreHardmode;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Items.Tools.PreHM;
using Waybound.Content.Items.Weapons.Magic.Staffs.PreHM;
using Waybound.Content.Tiles.Furniture;

namespace Waybound.Content.Items.Placeable.Furniture
{
	public class DeepCrateI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Crate");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<DeepCrate>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 2;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.consumable = true;
		}

		public override bool CanRightClick() => true;

		public override void RightClick(Player player)
		{
			var source = player.GetSource_OpenItem(Type);
			int choice1 = Main.rand.Next(3);
			switch (choice1)
			{
				case 0:
					player.QuickSpawnItem(source, ModContent.ItemType<DeepCrystalShardItem>(), Main.rand.Next(1, 5));
					break;
				case 1:
					player.QuickSpawnItem(source, ModContent.ItemType<DeepTreeItem>(), Main.rand.Next(5, 10));
					break;
				case 2:
					player.QuickSpawnItem(source, ModContent.ItemType<DeepTreeItem>(), Main.rand.Next(5, 10));
					break;
			}

			if (Main.rand.NextBool(2))
			{
				player.QuickSpawnItem(source, 2674, Main.rand.Next(4, 8));
			}
			else if (Main.rand.NextBool(3))
			{
				player.QuickSpawnItem(source, 2675, Main.rand.Next(2, 6));
			}
			else if (Main.rand.NextBool(4))
			{
				player.QuickSpawnItem(source, 2675, Main.rand.Next(2, 4));
			}

			if (Main.rand.NextBool(6))
			{
				int choice2 = Main.rand.Next(10);
				switch (choice2)
				{
					case 0:
						player.QuickSpawnItem(source, 2346, 1);
						break;
					case 1:
						player.QuickSpawnItem(source, 2354, 1);
						break;
					case 2:
						player.QuickSpawnItem(source, 292, 1);
						break;
					case 3:
						player.QuickSpawnItem(source, 2345, 1);
						break;
					case 4:
						player.QuickSpawnItem(source, 288, 1);
						break;
					case 5:
						player.QuickSpawnItem(source, 289, 1);
						break;
					case 6:
						player.QuickSpawnItem(source, 2355, 1);
						break;
					case 7:
						player.QuickSpawnItem(source, 296, 1);
						break;
					case 8:
						player.QuickSpawnItem(source, 290, 1);
						break;
					case 9:
						player.QuickSpawnItem(source, 288, 1);
						break;
				}
			}

			if (Main.rand.NextBool(10))
			{
				int choice4 = Main.rand.Next(4);
				switch (choice4)
				{
					case 0:
						player.QuickSpawnItem(source, ModContent.ItemType<DeepStoneAxe>(), 1);
						break;
					case 1:
						player.QuickSpawnItem(source, ModContent.ItemType<DeepStonePickaxe>(), 1);
						break;
					case 2:
						player.QuickSpawnItem(source, ModContent.ItemType<CrystalHeartNecklace>(), 1);
						break;
                    case 3:
                        player.QuickSpawnItem(source, ModContent.ItemType<Deepslate>(), 1);
                        break;
                }
			}
		}
	}
}