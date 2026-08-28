using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Waybound.Common.ModSystems;
using Waybound.Common.Utils;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Armor.Melee.Coreburned
{
    [AutoloadEquip(EquipType.Head)]
    public class CoreburnedHelmet : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

        public override void SetDefaults()
        {
            int width = 26; int height = 20;
            Item.Size = new Vector2(width, height);
            Item.rare = ItemRarityID.Yellow;
            Item.defense = 16;
            Item.value = Item.sellPrice(0, 4, 8, 50);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Melee) += 0.15f;
            player.endurance += 0.10f;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.fireWalk = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<CoreburnedBreastplate>()
                && legs.type == ModContent.ItemType<CoreburnedLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = LocUtil.ItemTooltip(LocUtil.ARM, "CoreburnedSetBonus");
            player.GetDamage(DamageClass.Melee) += 0.10f;
            player.GetModPlayer<CoreburnedPlayer>().coreburnedSet = true;
        }
    }

    public class CoreburnedPlayer : ModPlayer
    {
        public bool coreburnedSet;
        public bool stoneMode;
        public float fadeProgress;
        public bool isFadingIn;
        public bool isFadingOut;
        private int stoneTimer;

        private int cooldown;

        public override void ResetEffects()
        {
            coreburnedSet = false;

        }

        public override void PreUpdate()
        {
            if (cooldown > 0)
                cooldown--;

            if (coreburnedSet &&
                VanillaKeybinds.ArmorSetBonusActivation.JustPressed &&
                !stoneMode &&
                cooldown <= 0)
            {
                stoneMode = true;
                stoneTimer = 60 * 10;
                cooldown = 60 * 20;
                fadeProgress = 0f;
                isFadingIn = true;
                isFadingOut = false;
            }

            if (!stoneMode)
                return;

            Player.immune = true;
            Player.immuneTime = 2;

            Player.controlJump = false;
            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlUp = false;
            Player.controlDown = false;
            Player.controlUseItem = false;
            Player.controlUseTile = false;

            Player.velocity = Vector2.Zero;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && npc.CanBeChasedBy())
                {
                    npc.target = Player.whoAmI;
                }
            }

            stoneTimer--;

            if (stoneTimer <= 0)
            {
                isFadingOut = true;
                isFadingIn = false;

                if (fadeProgress <= 0f)
                {
                    stoneMode = false;
                    isFadingOut = false;
                    fadeProgress = 0f;
                }
            }

            if (isFadingIn && fadeProgress < 1f)
            {
                fadeProgress += 0.05f;
                if (fadeProgress > 1f) fadeProgress = 1f;
            }

            if (isFadingOut && fadeProgress > 0f)
            {
                fadeProgress -= 0.05f;
                if (fadeProgress < 0f) fadeProgress = 0f;
            }
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (!stoneMode)
                return;

            npc.SimpleStrikeNPC(hurtInfo.Damage * 10, 0, false, 0f);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (!stoneMode)
                return;

            if (proj.owner >= 0 &&
                proj.owner < Main.maxNPCs &&
                Main.npc[proj.owner].active)
            {
                Main.npc[proj.owner].SimpleStrikeNPC(hurtInfo.Damage * 10, 0, false, 0f);
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            return stoneMode;
        }

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (stoneMode || isFadingIn || isFadingOut)
                drawInfo.hideEntirePlayer = true;
        }
    }

    public class CoreburnedStoneLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FrontAccFront);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            CoreburnedPlayer modPlayer = drawInfo.drawPlayer.GetModPlayer<CoreburnedPlayer>();
            return modPlayer.stoneMode || modPlayer.isFadingIn || modPlayer.isFadingOut;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            CoreburnedPlayer modPlayer = drawPlayer.GetModPlayer<CoreburnedPlayer>();

            Texture2D texture = ModContent.Request<Texture2D>(
                "Waybound/Content/Items/Armor/Melee/Coreburned/CoreburnedCopy").Value;

            Vector2 pos = drawInfo.Center - Main.screenPosition;

            SpriteEffects effects = SpriteEffects.None;
            if (drawPlayer.direction == -1)
                effects = SpriteEffects.FlipHorizontally;

            float alpha = modPlayer.fadeProgress;
            Color mainColor = Color.White * alpha;
            Color glowColor = Color.Black * (0.5f * alpha);

            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = new Vector2(
                    MathHelper.Lerp(-4, 4, i % 3 / 2f),
                    MathHelper.Lerp(-4, 4, i / 3f)
                );

                DrawData glowData = new DrawData(
                    texture,
                    pos + offset,
                    null,
                    glowColor,
                    0f,
                    texture.Size() / 2,
                    1f,
                    effects,
                    0);

                drawInfo.DrawDataCache.Add(glowData);
            }

            DrawData mainData = new DrawData(
                texture,
                pos,
                null,
                mainColor,
                0f,
                texture.Size() / 2,
                1f,
                effects,
                0);

            drawInfo.DrawDataCache.Add(mainData);
        }
    }
}