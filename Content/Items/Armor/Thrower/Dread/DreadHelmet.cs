using System;
using Waybound.Common.ModSystems;
using Waybound.Common.Rarities;
using Waybound.Common.WUtils;
using Waybound.Content.Dusts;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Renderers;
using Terraria.ID;

namespace Waybound.Content.Items.Armor.Thrower.Dread
{
    [AutoloadEquip(EquipType.Head)]
    public class DreadHelmet : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 3, 20, 0);
            Item.rare = ModContent.RarityType<CoreburnedRarity>();
            Item.defense = 10;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Throwing) += 0.18f;
            player.moveSpeed += 0.08f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<DreadArmor>() && legs.type == ItemType<DreadLeggings>() && head.type == Type;
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = LocUtil.ItemTooltip(LocUtil.ARM, "DreadSetBonus");
            player.endurance += 0.12f;
            DreadDashPlayer dashPlayer = player.GetModPlayer<DreadDashPlayer>();
            dashPlayer.dashAvailable = true;
            if (dashPlayer.dashCooldown > 0)
                dashPlayer.dashCooldown--;
        }
    }

    public class DreadDashPlayer : ModPlayer
    {
        public bool dashAvailable = false;
        public int dashCooldown = 0;
        public bool isDashing = false;
        public int dashTimer = 0;
        public Vector2 dashDirection = Vector2.Zero;

        private const int DashDuration = 20;
        private const int DashSpeed = 18;
        private const int CooldownFrames = 120;

        public override void ResetEffects()
        {
            if (!dashAvailable)
            {
                isDashing = false;
                dashTimer = 0;
                dashCooldown = 0;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (!dashAvailable || isDashing || dashCooldown > 0 || Player.dead)
                return;

            if (VanillaKeybinds.ArmorSetBonusActivation.JustPressed)
            {
                Vector2 dashDir = Vector2.Zero;
                if (Player.controlLeft) dashDir.X = -1;
                if (Player.controlRight) dashDir.X = 1;
                if (Player.controlUp) dashDir.Y = -1;
                if (Player.controlDown) dashDir.Y = 1;

                if (dashDir == Vector2.Zero)
                    dashDir = new Vector2(Player.direction, 0);

                dashDir.Normalize();
                StartDash(dashDir);
            }
        }

        public void StartDash(Vector2 direction)
        {
            isDashing = true;
            dashTimer = DashDuration;
            dashDirection = direction;
            dashCooldown = CooldownFrames;
            Player.immune = true;
            Player.immuneTime = DashDuration + 5;
            Player.velocity = direction * DashSpeed;
            for (int i = 0; i < 14; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Player.width * 0.6f, Player.height * 0.6f);
                Vector2 vel = -direction * Main.rand.NextFloat(3f, 7f) + Main.rand.NextVector2Circular(1.5f, 1.5f);

                Dust.NewDustPerfect(
                    Player.Center + offset,
                    DustID.Torch,
                    vel,
                    100,
                    new Color(255, 180, 80),
                    Main.rand.NextFloat(0.9f, 1.4f)
                ).noGravity = true;
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Player.width * 0.7f, Player.height * 0.7f);
                Vector2 dustVel = -direction * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1f, 1f);

                Dust.NewDustPerfect(
                    Player.Center + offset,
                    ModContent.DustType<Fog>(),
                    dustVel,
                    0,
                    default,
                    Main.rand.NextFloat(0.9f, 1.2f)
                );
            }
        }

        public override void PostUpdate()
        {
            if (!dashAvailable) return;

            if (isDashing)
            {
                dashTimer--;
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, 2);
                Player.noKnockback = true;
                Player.velocity = dashDirection * DashSpeed;

                if (dashTimer % 2 == 0)
                {
                    float progress = (float)dashTimer / DashDuration;
                    int spawnCount = (int)(4f * progress) + 1;

                    for (int i = 0; i < spawnCount; i++)
                    {
                        Vector2 offset = new Vector2(
                            Main.rand.NextFloat(-Player.width * 0.65f, Player.width * 0.65f),
                            Main.rand.NextFloat(-Player.height * 0.65f, Player.height * 0.65f)
                        );

                        Vector2 dustVel = -dashDirection * Main.rand.NextFloat(1.2f, 3.5f) +
                                          Main.rand.NextVector2Circular(0.7f, 0.7f);

                        Dust dust = Dust.NewDustPerfect(
                            Player.Center + offset,
                            ModContent.DustType<Fog>(),
                            dustVel,
                            0,
                            default,
                            Main.rand.NextFloat(0.9f, 1.25f)
                        );
                        dust.noGravity = true;
                    }
                }

                if (dashTimer <= 0)
                {
                    isDashing = false;
                    Player.velocity *= 0.3f;

                    for (int i = 0; i < 18; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                        Dust.NewDustPerfect(
                            Player.Center,
                            ModContent.DustType<Fog>(),
                            dustVel,
                            0,
                            default,
                            Main.rand.NextFloat(0.8f, 1.15f)
                        );
                    }
                }
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (isDashing)
                return true;
            return false;
        }
    }
}