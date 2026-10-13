using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Waybound.Common.ModSystems;
using Waybound.Common.Rarities;
using Waybound.Common.WUtils;
using Waybound.Content.Dusts;

namespace Waybound.Content.Items.Armor.Generic.Viscula
{
    [AutoloadEquip(EquipType.Head)]
    public class HypertrophyHelm : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 3, 20, 0);
            Item.rare = ItemRarityID.Purple;
            Item.defense = 10;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Throwing) += 0.18f;
            player.moveSpeed += 0.08f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<HypertrophyBreastplate>() && legs.type == ItemType<AorticBreeches>() && head.type == Type;
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Loc.GetTips("Armor.AorticSetBonus");
            player.GetModPlayer<AorticPlayer>().bloodkeeper = true;
        }
    }
}