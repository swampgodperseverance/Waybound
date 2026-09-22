using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.WUtils;
using Waybound.Content.Items.Armor.Ranged.Iceborn;
using Waybound.Content.Items.Armor.Melee.Thunder;
using Waybound.Content.Projectiles.Armor;

namespace Waybound.Content.Items.Armor.Ranged.Iceborn
{
    [AutoloadEquip(EquipType.Head)]
    public sealed class IcebornHelmet : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 20;
            Item.rare = ItemRarityID.Blue;
            Item.defense = 3;
            Item.value = Item.sellPrice(0, 1, 4, 50);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Ranged) += 0.07f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) =>
            body.type == ItemType<IcebornBreastplate>() && legs.type == ItemType<IcebornLeggings>() && head.type == Type;

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Loc.GetTips("Armor.IcebornSetBonus");
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.GetModPlayer<IcebornPlayer>().icebornSet = true;
        }
    }

    public class IcebornPlayer : ModPlayer
    {
        public bool icebornSet;
        private int spawnTimer;
        private int[] spikeIds = new int[3] { -1, -1, -1 };

        public override void ResetEffects()
        {
            icebornSet = false;
        }

        public override void PostUpdate()
        {
            if (!icebornSet)
            {
                ClearSpikes();
                spawnTimer = 0;
                return;
            }

            spawnTimer++;
            if (spawnTimer >= Main.rand.Next(120, 181))
            {
                spawnTimer = 0;
                TrySpawnSpike();
            }

            for (int i = 0; i < 3; i++)
            {
                if (spikeIds[i] >= 0)
                {
                    Projectile p = Main.projectile[spikeIds[i]];
                    if (!p.active || p.type != ModContent.ProjectileType<IceSpike>() || p.owner != Player.whoAmI)
                        spikeIds[i] = -1;
                }
            }
        }

        private void TrySpawnSpike()
        {
            int freeSlot = -1;
            for (int i = 0; i < 3; i++)
            {
                if (spikeIds[i] < 0)
                {
                    freeSlot = i;
                    break;
                }
            }
            if (freeSlot < 0)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            Vector2 offset = freeSlot switch
            {
                0 => new Vector2(-28f, -46f),
                1 => new Vector2(0f, -58f),
                _ => new Vector2(28f, -46f)
            };

            int id = Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                Player.Center + offset,
                Vector2.Zero,
                ModContent.ProjectileType<IceSpike>(),
                10,
                1.5f,
                Player.whoAmI,
                freeSlot
            );

            if (id >= 0 && id < Main.maxProjectiles)
            {
                spikeIds[freeSlot] = id;
                Main.projectile[id].netUpdate = true;
            }
        }

        private void ClearSpikes()
        {
            for (int i = 0; i < 3; i++)
            {
                if (spikeIds[i] >= 0)
                {
                    Projectile p = Main.projectile[spikeIds[i]];
                    if (p.active && p.type == ModContent.ProjectileType<IceSpike>() && p.owner == Player.whoAmI)
                        p.Kill();
                    spikeIds[i] = -1;
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (icebornSet)
                CommandSpikes(target.whoAmI);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (icebornSet && proj.type != ModContent.ProjectileType<IceSpike>())
                CommandSpikes(target.whoAmI);
        }

        private void CommandSpikes(int targetWhoAmI)
        {
            for (int i = 0; i < 3; i++)
            {
                if (spikeIds[i] >= 0)
                {
                    Projectile p = Main.projectile[spikeIds[i]];
                    if (p.active && p.type == ModContent.ProjectileType<IceSpike>() && p.owner == Player.whoAmI)
                    {
                        p.ai[1] = targetWhoAmI + 1;
                        p.netUpdate = true;
                    }
                }
            }
        }
    }
}