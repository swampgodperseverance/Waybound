using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class TinRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<TinRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 4);
            Item.damage = 10;
            Item.knockBack = 3.8f;
            Item.crit = 4;
        }
    }

    public class TinRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/TinRapier";
        public override bool UseSpecial => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<TinRapier>();

        protected override Color GetLungeParticleColor() => new Color(175, 170, 155, 255);
        protected override Color GetTipParticleColor() => new Color(160, 155, 140, 255);
        protected override Color GetTrailParticleColor() => new Color(165, 160, 145, 200);
        protected override Color GetGlowColor() => new Color(190, 185, 170, 160);
    }
}