using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Weapons.Melee.Swords.Waybound.Content.Items.Weapons.Melee.Swords;

namespace Waybound.Content.Items.Weapons.Melee.Swords
{//faster than red 
    public sealed class SlimeSword1 : CustomSwingItem
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<SlimeSword1Swing>();

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.width = 44;  
            Item.height = 44;
            Item.damage = 16;
            Item.knockBack = 4.5f;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Gel, 25)
                .AddRecipeGroup(RecipeGroupID.IronBar, 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public sealed class SlimeSword1Swing : CustomSwing
    {

        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Swords/SlimeSword1";

        public override float SwingArc => 3.4f;

        public override Color TrailColor => new Color(70, 130, 255);
        public override Color TrailCoreColor => new Color(190, 225, 255);
        public override Color ParticleColor => new Color(80, 150, 255);
        public override Color ParticleCoreColor => new Color(200, 235, 255);
        public override int DustType => DustID.t_Slime;
        public override Color DustColor => new Color(0, 80, 255, 80);

        public override void OnSwingHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slimed, IsFinisher ? 300 : 150);
        }
    }
}