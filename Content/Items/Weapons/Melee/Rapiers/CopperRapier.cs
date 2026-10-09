using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class CopperRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<CopperRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 40);
            Item.damage = 9;
            Item.knockBack = 3.5f;
            Item.crit = 4;
        }
    }

    public class CopperRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/CopperRapier";
        public override bool UseSpecial => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<CopperRapier>();

        protected override Color GetLungeParticleColor() => new Color(190, 110, 60, 255);
        protected override Color GetTipParticleColor() => new Color(175, 95, 50, 255);
        protected override Color GetTrailParticleColor() => new Color(180, 100, 55, 200);
        protected override Color GetGlowColor() => new Color(210, 130, 70, 180);
    }
}