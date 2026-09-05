//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Synergia.Content.Projectiles.Aura;
//using static Terraria.ModLoader.ModContent;
//using Synergia.Common.Rarities;

//namespace Synergia.Content.Items.Weapons.Cogworm
//{
//    [ExtendsFromMod("ValhallaMod")]
//    public class HellgateAuraScythe : ValhallaMod.Items.AI.ValhallaAuraItem
//    {
//        public override void SetStaticDefaults()
//        {
//            Item.staff[Item.type] = true;
//            ValhallaMod.Values.AuraScythe[Item.type] = true;
//        }

//        public override void SetDefaults()
//        {
//            Item.DamageType = DamageClass.Summon;
//            Item.width = 38;
//            Item.height = 38;
//            Item.useTime = 30;
//            Item.useAnimation = 30;
//            Item.UseSound = SoundID.Item82;
//            Item.noMelee = true;
//            Item.useTurn = true;
//            Item.useStyle = ItemUseStyleID.Shoot;
//            Item.value = Item.sellPrice(0, 1, 0, 0);
//            Item.rare = ModContent.RarityType<LavaGradientRarity>();

//            Item.mana = 40;
//            Item.damage = 69;
//            Item.shoot = ProjectileType<HellgateAura>();
//            Item.shootSpeed = 1f;

//            typeScythe = ProjectileType<HellgateAuraScytheCut>();
//            scytheDamageScale = 1.75f;
//        }
//    }
//}