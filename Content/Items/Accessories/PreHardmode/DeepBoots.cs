using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Projectiles.Hostile;

namespace Waybound.Content.Items.Accessories.PreHardmode
{
    [AutoloadEquip(EquipType.Shoes)]
    public class DeepBoots : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("Creates crystal flames when you run\nThe wearer can run super fast\nGives immunity to ''Deep Fire''");
            // DisplayName.SetDefault("Crystal Gaiters");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = 2;
            Item.value = Item.sellPrice(0, 0, 50, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[ModContent.BuffType<DeepFire>()] = true;
            player.accRunSpeed += 3f;
            player.GetModPlayer<DeepBootsPL>().deepBoots = 1;
        }
    }
    public class DeepBootsPL : ModPlayer
    {
        public int deepBoots;

        public float dam;

        public override void ResetEffects()
        {

            deepBoots = 0;
     
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
            if (deepBoots == 1)
            {
                if (Player.runSoundDelay == 1 && Player.velocity.Y == 0)
                {
                    Vector2 vector = new Vector2(0, -Main.rand.Next(1, 2)).RotatedByRandom(MathHelper.ToRadians(30));
                    int proj = Projectile.NewProjectile(Player.GetSource_Misc("DeepBoots"), Player.Center, vector, ModContent.ProjectileType<DeepBootsP>(), (int)(10 * dam), 0.1f, Main.myPlayer);
                    Main.projectile[proj].timeLeft += Main.rand.Next(0, 40);
                }
            }
        }

   
    }
}