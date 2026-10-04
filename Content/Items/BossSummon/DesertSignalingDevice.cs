using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.NPCs.Bosses.Themis;

namespace Waybound.Content.Items.BossSummon
{
	public class DesertSignalingDevice : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Desert Signaling Device");
			// Tooltip.SetDefault("Allows you to find you Themis robots");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 20;
			Item.value = Item.sellPrice(0, 0, 75, 0);
			Item.rare = 3;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = true;
		}

		public override bool CanUseItem(Player player)
		{
			return base.CanUseItem(player) && player.ZoneDesert && player.ZoneOverworldHeight && !NPC.AnyNPCs(ModContent.NPCType<Themis>());
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer && player.ZoneDesert && player.ZoneOverworldHeight)
			{
				int type = ModContent.NPCType<Themis>();
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.SpawnOnPlayer(player.whoAmI, type);
				}
				else
				{
					NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
				}
			}

			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<DesertCore>(1)
				.AddIngredient<DesertWreckage>(5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}