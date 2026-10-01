using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Hostile;

namespace Waybound.Content.Items.Accessories.PreHardmode
{
	[AutoloadEquip(EquipType.Neck)]
	public class CrystalHeartNecklace : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("");
			// DisplayName.SetDefault("Crystal Heart Necklace");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = 2;
			Item.value = Item.sellPrice(0, 0, 60, 0);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<CrystalHeartPlayer>().crystalHeartNecklace = 1;
		}
	}
    public class CrystalHeartPlayer : ModPlayer
    {
        public int crystalHeartNecklace;

        public float dam;

        public override void ResetEffects()
        {
            crystalHeartNecklace = 0;

        }

        public override void PreUpdate()
        {
            if (Main.masterMode)
            {
                dam = 1.666f;
            }
            else if (Main.expertMode && !Main.masterMode)
            {
                dam = 1.333f;
            }
            else
            {
                dam = 1f;
            }

            base.PreUpdate();
        }

        public override void PostUpdate()
        {

        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (crystalHeartNecklace == 1 && Main.rand.NextBool(2))
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vector = new Vector2(Main.rand.Next(-7, -5), Main.rand.Next(-7, -5)).RotatedByRandom(MathHelper.ToRadians(360));
                    int proj = Projectile.NewProjectile(Player.GetSource_Misc("CrystalHeartNecklace"), Player.Center, vector, ModContent.ProjectileType<DeepCrystalProj>(), (int)(15 * dam), 2, Player.whoAmI);
                    Main.projectile[proj].timeLeft += Main.rand.Next(-5, 5);
                }
            }
        }
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)/* tModPorter If you don't need the Item, consider using OnHitNPC instead */
        {

        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)/* tModPorter If you don't need the Projectile, consider using OnHitNPC instead */
        {

            
        }
    }
}