using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Items.Accessories.Shields
{
	[AutoloadEquip(EquipType.Shield)]
	public class DeepStoneShield : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("Allows the player to dash\nDouble tap a direction");
			// DisplayName.SetDefault("Deep Stone Shield");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.rare = 2;
			Item.defense = 2;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<DeepShieldUpdate>().deepShield = true;
		}
	}

	public class DeepShieldUpdate : ModPlayer
	{
		public const int DashRight = 2;
		public const int DashLeft = 3;
		public const int DashCooldown = 60;
		public const int DashDuration = 35;
		public int DashDir = -1;
		public int DashDelay = 0;
		public int DashTimer = 0;
		public const float DashVelocity = 10f;
		public bool deepShield;

		public override void ResetEffects()
		{
			deepShield = false;
			if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15)
			{
				DashDir = DashRight;
			}
			else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15)
			{
				DashDir = DashLeft;
			}
			else
			{
				DashDir = -1;
			}
		}

		public override void PreUpdateMovement()
		{
			if (CanUseDash() && DashDir != -1 && DashDelay == 0)
			{
				SoundEngine.PlaySound(SoundID.Item43, Player.position);

				Vector2 newVelocity = Player.velocity;

				switch (DashDir)
				{
					case DashLeft when Player.velocity.X > -DashVelocity:
					case DashRight when Player.velocity.X < DashVelocity:
						{
							float dashDirection = DashDir == DashRight ? 1 : -1;
							newVelocity.X = dashDirection * DashVelocity;
							break;
						}
					default:
						return;
				}

				DashDelay = DashCooldown;
				DashTimer = DashDuration;
				Player.velocity = newVelocity;

				for (int i = 0; i < 75; i++)
				{
					Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
					Dust dust = Dust.NewDustPerfect(Player.Center + (Player.velocity.SafeNormalize(Vector2.UnitX) * 25), ModContent.DustType<DeepMagicDust>(), speed * 7.5f, Scale: 1.5f);
					dust.noGravity = true;
				}
			}

			if (DashDelay > 0)
				DashDelay--;

			if (DashTimer > 0)
			{
				Player.eocDash = DashTimer;
				Player.armorEffectDrawShadowEOCShield = true;

				DashTimer--;
			}
		}

		private bool CanUseDash()
		{
			return deepShield && Player.dashType == 0 && !Player.setSolar && !Player.mount.Active;
		}
	}
}