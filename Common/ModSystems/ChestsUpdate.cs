using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using Waybound.Content.Items.Weapons.Magic.Staffs.PreHM;
using Waybound.Content.Items.Accessories.PreHardmode;
using Waybound.Content.Items.Tools.PreHM;
using Waybound.Content.Items.Ammo.Arrows;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Tiles.Furniture.DeepWoodFurniture;

namespace VictimaMod2.Common.Systems
{
    public class ChestsUpdate : ModSystem
	{
		public override void PostWorldGen()
		{
            #region ITEMS

            int[] placeDeepChest1 = new int[] { ItemType<DeepTreeItem>(), ItemType<DeepCrystalShardItem>(), ItemType<CrystalArrow>(),  ItemType<CrystalMushroomItem>() }; //unimportant things                        ItemType<deepstoneJavelin>(),
            int[] placeDeepChest2 = new int[] { 71, 72, 73 }; //coins
            int[] placeDeepChest3 = new int[] { ItemType<DeepStoneAxe>(), ItemType<DeepStonePickaxe>(), ItemType<DeepStoneHammer>(), ItemType<CrystalHeartNecklace>(), ItemType<DeepBoots>(), ItemType<Deepslate>() }; //delicious things
            //int[] placeDeepChest3_1 = new int[] { ItemType<depthsCutter>(), ItemType<darkDirtySaber>() }; //delicious things with little chance
            int[] placeDeepChest4 = new int[] { 288, 288, 289, 290, 292, 296, 2322, 2345, 2346, 2354, 2355 }; //potions
            int[] placeDeepChest5 = new int[] { 43, 166, 167, 188, 2350, 28, 52 }; //other items
            int[] placeDeepChest6 = new int[] { 22, 704, 21, 705, 19, 706 }; //bars
            //List<int> placeDeepbloomArmor = new() { ItemType<WanderingHelmet>(), ItemType<WanderingChestplate>(), ItemType<WanderingBoots>(), ItemType<glowingBlood>() }; //deeploom armor

            //int[] placeObsidianChest = new int[] { ItemType<bloodyAsh>() };

            //int[] placeWoodChest = new int[] { ItemType<robbersCape>(), ItemType<basebalBat>(), ItemType<ancientBow>() };

            //int[] placeGoldChest = new int[] { ItemType<Mace>() };

            //int[] placeLockedGoldChest = new int[] { ItemType<nightStalker>() };

            //int[] placeMushroomChest = new int[] { ItemType<sporePiercer>() };

            #endregion

            #region OTHER

            int nightStalkerCounter = 0;

            #endregion

            for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
            {
                Chest chest = Main.chest[chestIndex];

				if(chest == null)
					continue;

                #region DEEP CHESTS

                if (Main.tile[chest.x, chest.y].TileType == TileType<DeepChest>())
				{
                    if (Main.tile[chest.x, chest.y].TileFrameX == 0 * 36)
                    {
                        for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                        {
                            if (chest.item[inventoryIndex].type == ItemID.None)
                            {
                                if (Main.rand.Next(100) < 90)
                                {
                                    chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeDeepChest1));
                                    if (chest.item[inventoryIndex].type == ItemType<CrystalArrow>()) //|| chest.item[inventoryIndex].type == ItemType<deepstoneJavelin>())
                                    {
                                        chest.item[inventoryIndex].stack = Main.rand.Next(25, 76);
                                    }
                                    else if (chest.item[inventoryIndex].type == ItemType<CrystalMushroomItem>())
                                    {
                                        chest.item[inventoryIndex].stack = Main.rand.Next(1, 4);
                                    }
                                    else
                                    {
                                        chest.item[inventoryIndex].stack = Main.rand.Next(1, 11);
                                    }
                                    inventoryIndex += 1;
                                }

                                if (Main.rand.Next(100) < 90)
                                {
                                    chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeDeepChest2));
                                    switch (chest.item[inventoryIndex].type)
                                    {
                                        case 71:
                                            chest.item[inventoryIndex].stack = Main.rand.Next(60, 101);
                                            break;
                                        case 72:
                                            chest.item[inventoryIndex].stack = Main.rand.Next(20, 101);
                                            break;
                                        case 73:
                                            chest.item[inventoryIndex].stack = Main.rand.Next(1, 4);
                                            break;
                                    }
                                    inventoryIndex += 1;
                                }

                                if (Main.rand.Next(100) < 80)
                                {
                                    chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeDeepChest3));
                                    inventoryIndex += 1;
                                }
                                else if (Main.rand.Next(100) < 5)
                                {
                                    //chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeDeepChest3_1));
                                    //if (chest.item[inventoryIndex].type == ItemType<depthsCutter>())
                                    //    chest.item[inventoryIndex].stack = Main.rand.Next(1, 3);
                                    //inventoryIndex += 1;
                                }

                                if (Main.rand.Next(100) < 75)
                                {
                                    chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeDeepChest4));
                                    inventoryIndex += 1;
                                }

                                for (int i = 0; i < Main.rand.Next(1, 3); i++)
                                {
                                    if (Main.rand.Next(100) < 80)
                                    {
                                        chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeDeepChest5));
                                        switch (chest.item[inventoryIndex].type)
                                        {
                                            case 166:
                                                chest.item[inventoryIndex].stack = Main.rand.Next(10, 16);
                                                break;
                                            case 28:
                                            case 188:
                                                chest.item[inventoryIndex].stack = Main.rand.Next(4, 7);
                                                break;
                                            case 2350:
                                                chest.item[inventoryIndex].stack = Main.rand.Next(1, 4);
                                                break;
                                        }
                                        inventoryIndex += 1;
                                    }
                                }

                                if (Main.rand.Next(100) < 80)
                                {
                                    chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeDeepChest6));
                                    switch (chest.item[inventoryIndex].type)
                                    {
                                        //22, 704, 21, 705, 19, 706
                                        case 19:
                                        case 706:
                                            chest.item[inventoryIndex].stack = Main.rand.Next(2, 6);
                                            break;
                                        default:
                                            chest.item[inventoryIndex].stack = Main.rand.Next(5, 11);
                                            break;
                                    }
                                    inventoryIndex += 1;
                                }

                                break;
                            }
                        }
                    }
                    //else if(Main.tile[chest.x, chest.y].TileFrameX == 1 * 36)
                    //{
                    //    for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                    //    {
                    //        if (chest.item[inventoryIndex].type == ItemID.None)
                    //        {
                    //            if (placeDeepbloomArmor.Count > 0)
                    //            {
                    //                int item = placeDeepbloomArmor[0];
                    //                placeDeepbloomArmor.Remove(item);
                    //                chest.item[inventoryIndex].SetDefaults(item);
                    //                if (item == ItemType<glowingBlood>())
                    //                    chest.item[inventoryIndex].stack = Main.rand.Next(4, 7);
                    //            }
                    //        }
                    //    }
                    //}
                }

                #endregion

                if(Main.tile[chest.x, chest.y].TileType != TileID.Containers) //VANILLA CHESTS
                    continue;

                //#region OBSIDIAN CHESTS

                //if (Main.tile[chest.x, chest.y].TileFrameX == 4 * 36)
                //{
                //    for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                //    {
                //        if (chest.item[inventoryIndex].type == ItemID.None)
                //        {
                //            if (Main.rand.Next(100) < 35)
                //                chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeObsidianChest));
                //            break;
                //        }
                //    }
                //}

                //#endregion

                //#region WOOD CHESTS

                //if (Main.tile[chest.x, chest.y].TileFrameX == 0 * 36)
                //{
                //    for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                //    {
                //        if (chest.item[inventoryIndex].type == ItemID.None)
                //        {
                //            if (Main.rand.Next(100) < 60)
                //                chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeWoodChest));
                //            break;
                //        }
                //    }
                //}

                //#endregion

                //#region GOLD CHESTS

                //if (Main.tile[chest.x, chest.y].TileFrameX == 1 * 36)
                //{
                //    for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                //    {
                //        if (chest.item[inventoryIndex].type == ItemID.None)
                //        {
                //            if (Main.rand.Next(100) < 35)
                //                chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeGoldChest));
                //            break;
                //        }
                //    }
                //}

                //#endregion

                //#region LOCKED GOLD CHEST

                //if (Main.tile[chest.x, chest.y].TileFrameX == 2 * 36)
                //{
                //    for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                //    {
                //        if (chest.item[inventoryIndex].type == ItemID.None)
                //        {
                //            if (Main.rand.Next(100) < 25 || nightStalkerCounter == 0)
                //            {
                //                if (nightStalkerCounter == 0)
                //                {
                //                    chest.item[inventoryIndex].SetDefaults(ItemType<nightStalker>());
                //                    nightStalkerCounter++;
                //                }
                //                else
                //                    chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeLockedGoldChest));
                //            }
                //            break;
                //        }
                //    }
                //}

                //#endregion

                //#region MUSHROOM CHESTS

                //if (Main.tile[chest.x, chest.y].TileFrameX == 32 * 36)
                //{
                //    for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                //    {
                //        if (chest.item[inventoryIndex].type == ItemID.None)
                //        {
                //            if (Main.rand.Next(100) < 50)
                //                chest.item[inventoryIndex].SetDefaults(Main.rand.Next(placeMushroomChest));
                //            break;
                //        }
                //    }
                //}

                //#endregion
            }
        }
	}
}
