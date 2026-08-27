using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Accessories.Hardmode
{
    public class SymmetryPlayer : ModPlayer
    {
        public bool OriginOfSymmetry;

        public override void ResetEffects()
        {
            OriginOfSymmetry = false;
        }
    }
    public class TheOriginOfSymmetry : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;

            Item.accessory = true;

            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(gold: 20);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetCritChance(DamageClass.Generic) += 20f;

            if (!hideVisual)
                player.GetModPlayer<SymmetryPlayer>().OriginOfSymmetry = true;
        }
    }

    public class SymmetrySystem : ModSystem
    {
        public override void Load()
        {
            if (Main.dedServ)
                return;

            Filters.Scene["Waybound:OriginOfSymmetry"] =
                new Filter(
                    new ScreenShaderData(
                        new Ref<Effect>(
                            ModContent.Request<Effect>(
                                "Waybound/Assets/Effects/OriginOfSymmetry",
                                ReLogic.Content.AssetRequestMode.ImmediateLoad
                            ).Value
                        ),
                        "Pass1"
                    ),
                    EffectPriority.VeryHigh
                );
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;

            bool active =
                Main.LocalPlayer
                    .GetModPlayer<SymmetryPlayer>()
                    .OriginOfSymmetry;

            if (active)
            {
                Filters.Scene["Waybound:OriginOfSymmetry"].GetShader()
                    .UseProgress((float)Main.GlobalTimeWrappedHourly);

                Filters.Scene.Activate("Waybound:OriginOfSymmetry");
            }
            else
            {
                Filters.Scene.Deactivate("Waybound:OriginOfSymmetry");
            }
        }
    }
}