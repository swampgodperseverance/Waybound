using System.Linq;
using Terraria;
using Terraria.ID;
using Waybound.Common.Rarities;
using Waybound.Common.Utils;
using Waybound.Content.Projectiles.Armor;

namespace Waybound.Content.Items.Armor.Summon.Guts
{
    [AutoloadEquip(EquipType.Head)]
    public sealed class Gutshelmet : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 20;
            Item.rare = ModContent.RarityType<CoreburnedRarity>();
            Item.defense = 7;
            Item.value = Item.sellPrice(0, 5, 2, 10);
        }

        public override void UpdateEquip(Player player)
        {
            player.maxMinions += 2;
            player.maxTurrets += 1;
            player.lifeRegen += 2;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
            => body.type == ModContent.ItemType<Gutsplate>()
            && legs.type == ModContent.ItemType<Gutslegs>()
            && head.type == Type;


        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Loc.GetTips("Armor.GutsSetBonus");
            player.GetDamage(DamageClass.Summon) += 0.18f;

            var modPlayer = player.GetModPlayer<GutsArmorPlayer>();
            if (player.dead) return;

            modPlayer.GutsJewTimer++;
            if (modPlayer.GutsJewTimer >= 240)
            {
                modPlayer.GutsJewTimer = 0;

                int count = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<GutsJew>())
                    {
                        count++;
                    }
                }

                if (count < 5)
                {
                    Vector2 pos = player.Center + new Vector2(Main.rand.Next(-48, 49), Main.rand.Next(-48, 49));

                    int jewIdx = Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item),
                        pos,
                        Vector2.Zero,
                        ModContent.ProjectileType<GutsJew>(),
                        0,
                        0f,
                        player.whoAmI,
                        ai0: count  
                    );
                    if (jewIdx >= 0 && Main.projectile[jewIdx].active)
                    {
                        Projectile.NewProjectile(
                            player.GetSource_ItemUse(Item),
                            player.Center,
                            Vector2.Zero,
                            ModContent.ProjectileType<Gut>(),
                            0,
                            0f,
                            player.whoAmI,
                            ai0: jewIdx   
                        );
                    }

                    if (Main.netMode != NetmodeID.SinglePlayer && jewIdx < Main.maxProjectiles)
                    {
                        NetMessage.SendData(MessageID.SyncProjectile, number: jewIdx);
                    }
                }
            }

            modPlayer.UpdateGutsJewProjectiles();
        }
    }
        public class GutsArmorPlayer : ModPlayer
    {
        public int GutsJewTimer { get; set; } = 0;

        public override void ResetEffects()
        {
            if (!Player.armor.Any(a => a.type == ModContent.ItemType<Gutshelmet>()))
            {
                GutsJewTimer = 0;
            }
        }

        public override void OnEnterWorld()
        {
            GutsJewTimer = 0;
        }

        public override void OnRespawn()
        {
            GutsJewTimer = 0;
        }

        public void UpdateGutsJewProjectiles()
        {
        }

        public override void PostUpdateEquips()
        {
            if (!Player.active || Player.dead || Player.ghost)
            {
                GutsJewTimer = 0;
            }
        }
    }
}