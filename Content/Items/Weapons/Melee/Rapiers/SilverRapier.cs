using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class SilverRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<SilverRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 9);
            Item.damage = 13;
            Item.knockBack = 4.5f;
            Item.crit = 6;
        }
    }

    public class SilverRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/SilverRapier";
        public override bool UseSpecial => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<SilverRapier>();

        protected override Color GetLungeParticleColor() => new Color(180, 190, 205, 255);
        protected override Color GetTipParticleColor() => new Color(165, 175, 190, 255);
        protected override Color GetTrailParticleColor() => new Color(170, 180, 195, 200);
        protected override Color GetGlowColor() => new Color(200, 210, 220, 180);
    }
}