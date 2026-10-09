using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class LeadRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<LeadRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 4);
            Item.damage = 12;
            Item.knockBack = 4.2f;
            Item.crit = 5;
        }
    }

    public class LeadRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/LeadRapier";
        public override bool UseSpecial => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<LeadRapier>();

        protected override Color GetLungeParticleColor() => new Color(120, 125, 140, 255);
        protected override Color GetTipParticleColor() => new Color(105, 110, 125, 255);
        protected override Color GetTrailParticleColor() => new Color(110, 115, 130, 200);
        protected override Color GetGlowColor() => new Color(140, 145, 160, 180);
    }
}