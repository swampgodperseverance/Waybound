using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Weapons.Melee.Swords.Waybound.Content.Items.Weapons.Melee.Swords;

namespace Waybound.Content.Items.Weapons.Melee.Swords
{
    //red
    public sealed class SlimeSword2 : CustomSwingItem
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<SlimeSword2Swing>();

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.width = 48;   
            Item.height = 48;
            Item.damage = 28;
            Item.knockBack = 6f;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 20, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SlimeSword1>()
                .AddIngredient(ItemID.Gel, 30)
                .AddIngredient(ItemID.HellstoneBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public sealed class SlimeSword2Swing : CustomSwing
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Swords/SlimeSword2";
        public override float SwingArc => 3.9f;
        public override float FinisherScaleMultiplier => 1.3f;
        public override float ParticleSizeMultiplier => 1.15f;

        public override Color TrailColor => new Color(255, 70, 70);
        public override Color TrailCoreColor => new Color(255, 205, 190);
        public override Color ParticleColor => new Color(255, 90, 80);
        public override Color ParticleCoreColor => new Color(255, 215, 170);

        public override int DustType => DustID.t_Slime;
        public override Color DustColor => new Color(255, 50, 40, 90);

        public override void OnSwingHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, IsFinisher ? 300 : 180);
        }
    }
}