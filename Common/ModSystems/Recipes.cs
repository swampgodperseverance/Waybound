using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.UIs;

namespace Waybound.Common.ModSystems
{
    public class Recipes : ModSystem
    {
        public override void AddRecipes()
        {
            //Recipe leather = Recipe.Create(ItemID.Leather, 1);
            //leather.AddIngredient(1330, 5);
            //leather.AddTile(TileID.WorkBenches);
            //leather.Register();
            //Recipe spectreboots = Recipe.Create(405);
            //spectreboots.AddIngredient(ModContent.ItemType<Content.Items.Accessories.Boots.deepBoots>());
            //spectreboots.AddIngredient(128);
            //spectreboots.AddTile(TileID.WorkBenches);
            //spectreboots.Register();
            //Recipe avengeremblem = Recipe.Create(935);
            //avengeremblem.AddIngredient(ModContent.ItemType<Content.Items.Accessories.Emblems.throwerEmblem>());
            //avengeremblem.AddIngredient(548, 5);
            //avengeremblem.AddIngredient(549, 5);
            //avengeremblem.AddIngredient(547, 5);
            //avengeremblem.AddTile(114);
            //avengeremblem.Register();
        }

        public static RecipeGroup DesertHunterGroup;

        public override void AddRecipeGroups()
        {
            //.AddRecipeGroup(RecipeGroupID.IronBar, x)

            DesertHunterGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ModContent.ItemType<DesertHunterBlockI>())}", ModContent.ItemType<DesertHunterBlockI>(), ModContent.ItemType<DesertHunterTileI>());
            RecipeGroup.RegisterGroup("Waybound:DesertHunterBlockI", DesertHunterGroup);
            //.AddRecipeGroup("VictimaMod2:desertHunterBlockI", x)

            if (RecipeGroup.recipeGroupIDs.ContainsKey("Wood"))
            {
                //int index = RecipeGroup.recipeGroupIDs["Wood"];
                //RecipeGroup group = RecipeGroup.recipeGroups[index];
                //group.ValidItems.Add(ModContent.ItemType<Content.Items.Placeable.deepTreeItem>());
            }
            //.AddRecipeGroup(RecipeGroupID.Wood, x)
        }
    }
}
