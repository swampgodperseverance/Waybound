using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Bars;
using Waybound.Particles;
using static Terraria.ModLoader.ModContent;

namespace Waybound.Content.Items.Armor.Magic.HielitiumArmor
{
    [AutoloadEquip(EquipType.Head)]
    public class HielitiumHelmet : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 0, 90, 0);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 7;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Magic) += 0.09f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<HielitiumBreastplate>() && legs.type == ItemType<HielitiumLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Two mid-air jumps (second is weaker)\nExtra jumps restore mana\nIncreased mana regeneration";
            player.GetModPlayer<HielitiumArmorPlayer>().setBonus = true;
            player.manaRegenBonus += 8;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<HielitiumBar>(4)
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(5070, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class HielitiumArmorPlayer : ModPlayer
    {
        public bool setBonus;

        public override void ResetEffects()
        {
            setBonus = false;
        }

        public override void PostUpdateEquips()
        {
            if (!setBonus)
                return;

            Player.GetJumpState<HielitiumExtraJump1>().Enable();
            Player.GetJumpState<HielitiumExtraJump2>().Enable();
        }
    }

    public class HielitiumExtraJump1 : ExtraJump
    {
        public override Position GetDefaultPosition() => new Before(CloudInABottle);

        public override float GetDurationMultiplier(Player player) => 1.15f;

        public override void OnStarted(Player player, ref bool playSound)
        {
            playSound = false;
            RestoreMana(player, 12);
            SpawnJumpParticles(player, 24); 
            SpawnRingParticles(player, 16); 
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.45f, Pitch = 0.2f }, player.Center);
        }

        public override void ShowVisuals(Player player)
        {
            SpawnJumpParticles(player, 3);
            if (Main.rand.NextBool(2))
                SpawnRingParticles(player, 2);
        }

        internal static void RestoreMana(Player player, int amount)
        {
            player.statMana = System.Math.Min(player.statMana + amount, player.statManaMax2);
            player.ManaEffect(amount);
        }

        internal static void SpawnJumpParticles(Player player, int count = 8)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Vector2 basePos = player.Bottom;
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = basePos + Main.rand.NextVector2Circular(18f, 8f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2.0f, 2.0f), Main.rand.NextFloat(-3.6f, -0.9f));
                float size = Main.rand.NextFloat(14f, 28f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.1f)),
                    new Color(100, 200, 255, 210) * Main.rand.NextFloat(0.9f, 1.3f),
                    Main.rand.Next(18, 34)
                ));
            }
        }

        internal static void SpawnRingParticles(Player player, int count = 12)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Vector2 center = player.Bottom;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 dir = angle.ToRotationVector2();
                Vector2 pos = center + dir * Main.rand.NextFloat(4f, 14f);
                Vector2 vel = dir * Main.rand.NextFloat(2.5f, 4.5f) + new Vector2(0f, -1.5f);
                float size = Main.rand.NextFloat(10f, 20f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.6f, 1.2f)),
                    new Color(140, 220, 255, 230) * Main.rand.NextFloat(1.0f, 1.4f),
                    Main.rand.Next(20, 36)
                ));
            }
        }
    }

    public class HielitiumExtraJump2 : ExtraJump
    {
        public override Position GetDefaultPosition() => new After(FartInAJar);

        public override float GetDurationMultiplier(Player player) => 0.7f;

        public override void OnStarted(Player player, ref bool playSound)
        {
            playSound = false;
            HielitiumExtraJump1.RestoreMana(player, 12);
            HielitiumExtraJump1.SpawnJumpParticles(player, 18); 
            HielitiumExtraJump1.SpawnRingParticles(player, 12);
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.35f, Pitch = 0.4f }, player.Center);
        }

        public override void ShowVisuals(Player player)
        {
            HielitiumExtraJump1.SpawnJumpParticles(player, 2);
            if (Main.rand.NextBool(2))
                HielitiumExtraJump1.SpawnRingParticles(player, 1);
        }
    }
}