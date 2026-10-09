using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class IronRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<IronRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 4);
            Item.damage = 11;
            Item.knockBack = 4f;
            Item.crit = 5;
        }
    }

    public class IronRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/IronRapier";
        public override bool UseSpecial => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<IronRapier>();

        protected override Color GetLungeParticleColor() => new Color(160, 160, 165, 255);
        protected override Color GetTipParticleColor() => new Color(145, 145, 150, 255);
        protected override Color GetTrailParticleColor() => new Color(150, 150, 155, 200);
        protected override Color GetGlowColor() => new Color(180, 180, 185, 180);
    }
}