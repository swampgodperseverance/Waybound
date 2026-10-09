using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class TungstenRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<TungstenRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 10);
            Item.damage = 14;
            Item.knockBack = 4.7f;
            Item.crit = 6;
        }
    }

    public class TungstenRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/TungstenRapier";
        public override bool UseSpecial => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<TungstenRapier>();

        protected override Color GetLungeParticleColor() => new Color(150, 160, 170, 255);
        protected override Color GetTipParticleColor() => new Color(135, 145, 155, 255);
        protected override Color GetTrailParticleColor() => new Color(140, 150, 160, 200);
        protected override Color GetGlowColor() => new Color(170, 180, 190, 180);
    }
}