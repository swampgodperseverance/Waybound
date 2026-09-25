using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Bars;

namespace Waybound.Content.Items.Tools.FishingPoles
{
	public class HielitiumFishingPole : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Hielitium Fishing Pole");
			// Tooltip.SetDefault("");
			ItemID.Sets.CanFishInLava[Item.type] = false;
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.useStyle = 1;
			Item.useAnimation = 8;
			Item.useTime = 8;
			Item.width = 42;
			Item.height = 34;
			Item.rare = 3;
			Item.value = Item.sellPrice(0, 2, 40, 0);
			Item.UseSound = SoundID.Item1;
			Item.fishingPole = 30;
			Item.shootSpeed = 16f;
			Item.shoot = ModContent.ProjectileType<HielitiumFishingPoleP>();
		}

		public override void HoldItem(Player player)
		{
			player.accFishingLine = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<HielitiumBar>(6)
				.AddIngredient(2503, 20)
				.AddIngredient(5070, 2) //flinx fur
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class HielitiumFishingPoleP : ModProjectile
	{
		public static readonly Color[] PossibleLineColors = new Color[]
		{
			new Color(241, 237, 210)
		};
		private Color FishingLineColor => PossibleLineColors[fishingLineColorIndex];
		private bool initialized;
		private int fishingLineColorIndex;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Hielitium Fishing Pole");
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 28;
			Projectile.aiStyle = 61;
			Projectile.bobber = true;
			Projectile.penetrate = -1;
			Projectile.CloneDefaults(ProjectileID.BobberWooden);
			DrawOriginOffsetY = -8;
		}

		public override void AI()
		{
			if (!initialized)
			{
				initialized = true;
				fishingLineColorIndex = (byte)Main.rand.Next(PossibleLineColors.Length);
				Projectile.netUpdate = true;
			}
		}

		public override void ModifyFishingLine(ref Vector2 lineOriginOffset, ref Color lineColor)/* tModPorter Note: Removed. Use ModItem.ModifyFishingLine */
		{
			lineOriginOffset = new Vector2(36, -30);
			lineColor = FishingLineColor;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write((byte)fishingLineColorIndex);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			fishingLineColorIndex = reader.ReadByte();
		}
	}
}