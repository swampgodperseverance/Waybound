
using Waybound.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace Waybound.Common.ModSystems.WorldGens
{
    public class GenerationSnowVillage : BaseWorldGens {
        //x = 103, y = 25
        static readonly byte[,] SnowVilageTiles =
        {
            // 0 - empty / 1 - snow block / 2 - ice block / 3 - Stone Slab / 4 - Stone / 5 - GrayBrick / 6 - Corrode Brick / 7 - Valhallite Brick / 8 - everwood Beam / 9 - Living Wood / 10:a - Boreal Wood Platform / 11:b - Everwood Platform / 12:c - Purple Brick Platform / 13:d - Wood / 14:e - EverWood / 15: f - Tin Roof / 16:g - Chain / 17:h - Silver Brick / 18:i - Diamond / 19:j - Blue Brick Platform / 20:k - Amber / 21:l - Zircon
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 }, // 1
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2 }, // 2
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2 }, // 3
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2 }, // 4
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 }, // 5
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0 }, // 6
            {0,1,1,1,1,1,1,1,1,1,1,3,3,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,5,5,3,3,1,1,1,1,3,4,4,3,3,3,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0 }, // 7
            {0,1,1,1,3,1,1,1,1,1,5,5,1,1,1,1,1,1,3,5,3,1,1,1,1,1,5,5,3,3,1,1,1,1,1,1,1,1,1,1,2,2,2,2,1,1,1,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,5,3,3,4,3,3,4,4,5,4,3,3,3,3,5,5,3,4,3,1,1,1,1,0,0,0,0,0,0 }, // 8
            {0,0,1,1,4,4,1,1,3,5,3,5,3,3,1,1,3,3,4,4,3,1,1,5,5,4,4,1,1,1,1,1,1,1,1,1,1,1,2,2,2,1,7,7,7,3,6,3,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,3,4,5,5,3,3,4,5,3,4,3,5,5,5,4,4,3,5,5,1,0,0,0,0,0,0,0,0 }, // 9
            {0,0,0,1,3,5,5,3,3,3,3,4,4,5,5,5,3,4,5,5,5,3,3,5,4,4,5,5,4,3,1,0,1,0,0,0,1,2,2,3,7,6,6,3,3,7,7,3,3,3,7,2,2,2,2,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 10
            {0,0,0,0,0,0,3,4,5,5,5,3,3,4,3,5,5,4,4,3,5,5,3,5,3,3,5,3,1,0,0,0,0,0,0,0,0,6,6,7,3,3,3,7,7,6,6,6,3,7,6,3,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 11
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,0,0,8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 12
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,0,0,8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,9,9,a,a,a,0,0,0,0,0,0,0,0,0,0,0,0,9,9,0,0,0,0,0,0,0,0,0 }, // 13
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,0,0,8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,9,9,0,0,0,0,0,0,0,0,0,0,a,c,b,b,0,9,d,0,0,0,0,0,0,0,0,0 }, // 14
            {0,0,0,0,0,0,e,a,0,0,0,0,0,0,0,0,0,0,0,a,a,a,3,3,0,0,3,3,0,0,0,0,0,0,0,0,0,0,0,0,g,0,a,a,8,0,0,8,0,0,0,0,e,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,e,d,e,0,0,a,a,a,0,0,0,0,0,0,0,0,0,0,e,0,0,0,0,0,0,0,0,0 }, // 15
            {0,0,0,0,0,d,d,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,3,3,3,3,3,0,0,0,0,0,0,0,0,0,0,e,0,0,g,0,0,0,8,0,0,8,0,g,0,9,d,9,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,f,0,e,9,9,0,0,0,0,0,0,0,0,0,0,0,0,0,d,d,0,0,0,0,0,0,0,0 }, // 16
            {0,0,0,0,0,e,9,d,0,0,0,0,0,0,0,0,0,0,0,3,3,3,3,3,3,f,f,0,0,0,0,0,0,0,0,0,0,d,d,9,g,9,d,9,8,d,9,8,9,9,9,9,d,e,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,f,f,f,9,9,d,e,d,9,0,0,0,0,0,0,0,0,0,e,e,d,e,0,0,0,0,0,0,0 }, // 17
            {0,0,0,0,d,e,9,e,0,j,j,j,j,0,0,0,0,3,3,3,3,f,f,f,f,0,0,0,0,0,0,0,0,0,0,0,4,9,9,d,d,d,d,9,9,d,d,9,e,e,d,d,d,d,d,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,f,f,f,f,e,d,9,9,d,d,9,e,0,0,9,9,d,d,0,f,0,0,0,0,0,0,0 }, // 18
            {0,0,0,f,f,d,9,d,e,0,0,0,0,0,e,e,d,3,f,f,f,f,f,0,0,0,0,0,0,0,0,0,0,0,0,3,4,9,4,i,e,d,4,d,1,1,h,3,4,3,e,4,4,e,4,1,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,f,f,f,f,0,0,0,9,9,d,d,e,d,d,0,0,f,0,0,0,0,0,0,0 }, // 19
            {0,f,f,f,f,f,e,9,e,0,0,0,e,9,d,f,f,f,f,f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,h,5,4,9,4,4,4,e,e,1,1,5,5,k,4,4,i,5,h,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,f,f,f,f,f,f,f,0,0,0,0,0,0,f,f,f,0,0,0,0,0,0 }, // 20
            {0,0,f,f,f,0,d,9,e,e,d,d,e,0,f,f,f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,4,5,h,l,4,4,1,1,1,1,1,h,5,5,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,f,f,f,f,f,f,f,f,f,0,0,0,0,0,0,0,0 }, // 21
            {0,0,0,f,f,f,f,d,9,9,d,d,f,f,f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,4,5,5,1,1,0,0,1,1,1,4,1,1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 22
            {0,0,0,0,0,f,f,f,e,e,0,f,f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,5,1,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 23
            {0,0,0,0,0,0,f,f,f,f,f,f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 24
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 25
        };
        static readonly byte[,] SnowVilageSlopes =
        {
            // 0 - empty / 1 - hamer / 2 - /| / 3 - |/ / 4 - \| / 5 - |\
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 1
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 2
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 3
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 4
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 5
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 6
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 7
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 8
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 9
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 10
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 11
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 12
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0 }, // 13
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0 }, // 14
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 15
            {0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0 }, // 16
            {0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,4,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 17
            {0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0 }, // 18
            {0,0,0,0,0,0,0,0,3,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 19
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 20
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 21
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,1,0,0,0,0,0,5,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 22
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 23
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 24
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 25
        };
        static readonly byte[,] SnowVilageWalls =
        {
            // 0 - empty / 1 - BackWoods Root Wall / 2 - Stone Slab Wall / 3 - Palm Wood Fence + Brown / 4 - Shadewood Fence + Brown / 5 - Gray brick Wall / 6 - Living wood wall / 7 - Rich Mahogany Fence + Brown / 8 - Wood Wall / 9 - Planked Wall / 10:a - Glass Wall / 11:b - ice Brick Wall / 12:c - Resistant Wood Fence / 13:d - Everwood Wall / 14:e - Cloud Wail / 15:f - Tin Brick Wall
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 1
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 2
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 3
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 4
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 5
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 6
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0 }, // 7
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0 }, // 8
            {0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0 }, // 9
            {0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,2,2,2,2,5,5,2,5,2,2,3,4,4,3,2,0,1,1,0,0,0,0,0,0,0,0 }, // 10
            {0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,2,0,1,1,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,5,5,2,2,2,2,2,5,2,4,3,4,7,5,0,0,0,0,0,0,0,0,0,0,0 }, // 11
            {0,0,0,0,0,0,0,5,5,5,2,2,2,2,5,5,2,5,2,2,2,2,2,0,0,0,0,0,1,0,0,0,0,0,0,0,0,c,b,b,5,5,5,d,b,6,2,2,b,b,b,5,5,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,6,6,8,a,a,a,a,8,9,8,4,3,7,7,6,0,0,0,0,0,0,0,0,0,0,0 }, // 12
            {0,0,0,0,0,0,0,2,2,2,5,5,5,2,2,2,5,2,2,5,5,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,c,5,b,2,b,b,2,5,5,b,b,2,2,2,b,b,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,8,9,a,a,a,a,6,8,9,7,3,4,3,8,0,0,0,0,0,0,0,0,0,0,0 }, // 13
            {0,0,0,0,0,0,0,8,d,8,a,a,6,8,6,8,d,d,8,6,9,9,0,0,0,2,2,0,0,0,0,0,0,0,0,0,0,c,0,a,a,a,8,b,2,9,2,2,9,a,a,d,d,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,6,6,8,a,a,a,a,6,9,9,7,7,4,3,8,8,0,0,0,0,0,0,0,0,0,0 }, // 14
            {0,0,0,0,0,0,0,9,9,8,a,a,6,8,6,8,d,d,8,8,8,8,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,c,0,e,e,a,8,b,d,9,2,2,9,a,a,8,8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,6,8,8,9,9,8,8,6,6,3,7,3,7,6,6,0,0,0,0,0,0,0,0,0,0 }, // 15
            {0,0,0,0,0,0,0,8,9,8,a,a,8,8,6,6,8,8,9,9,8,8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,c,e,e,e,8,6,d,d,9,2,2,9,6,8,8,9,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,f,8,8,8,6,6,8,8,8,8,8,3,4,4,3,8,9,0,0,0,0,0,0,0,0,0,0 }, // 16
            {0,0,0,0,0,0,0,8,6,8,a,a,8,8,8,8,8,8,8,8,8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,c,e,e,e,e,e,d,9,9,9,9,9,d,d,d,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,8,8,9,9,8,4,7,3,3,8,0,0,0,0,0,0,0,0,0,0,0 }, // 17
            {0,0,0,0,0,0,0,d,d,8,8,8,8,6,6,d,6,9,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,7,3,4,f,f,f,f,0,0,0,0,0,0,0,0 }, // 18
            {0,0,0,0,0,0,0,0,0,8,8,8,d,d,8,8,8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,f,f,f,0,0,0,f,f,f,0,f,f,0,0,0,0,0,0,0,0 }, // 19
            {0,0,0,0,f,f,0,0,0,8,8,8,8,d,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,f,f,f,f,f,f,0,0,0,0,0,0,0,0,0 }, // 20
            {0,0,0,0,f,f,0,0,0,0,0,0,f,f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,e,e,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,e,e,e,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 21
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,e,e,e,e,0,0,0,0,e,e,e,e,e,e,0,e,e,e,0,0,e,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 22
            {0,0,0,0,0,0,0,0,0,0,f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,e,e,0,e,0,0,0,0,0,e,e,e,e,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 23
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,e,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 24
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // 25
        };

        static readonly int[] SnowVilageGenTiles = [147, 161, 163, 200];
        bool GenerateSnowVilage = false;
        public override bool GensBool { get => GenerateSnowVilage; set => GenerateSnowVilage = value; }
        public override string NameGen => "[Waybound] Snow Vilage";
        public override bool Do_MakeGen(GenerationProgress progress) {
            if (progress != null) {
                progress.Message = Language.GetTextValue("Mods.Waybound.WorldGenString.Vilage");
                progress.Set(0.33f);
            }

            List<Point> list = [];

            foreach (int k in SnowVilageGenTiles) {
                for (int i = Main.maxTilesX / 5; i < Main.maxTilesX / 5 * 4; i++) {
                    int y = 200;
                    while (!WorldGen.SolidOrSlopedTile(i, y + 1)) y++;
                    bool canBeGenerated = true;
                    for (int j = 0; j < 15; j++) {
                        if (!WorldGen.SolidOrSlopedTile(i + j, y + 1) || Main.tile[i + j, y + 1].TileType != k) {
                            canBeGenerated = false;
                        }
                    }
                    if (canBeGenerated) {
                        for (int a = 0; a < 7; a++) {
                            if (WorldGen.SolidOrSlopedTile(i - 1, y - a) || WorldGen.SolidOrSlopedTile(i + 33, y - a)) {
                                canBeGenerated = false;
                            }
                        }
                    }
                    if (canBeGenerated) list.Add(new Point(i, y));
                }
                if (list.Count > 0) {
                    Point point = list[WorldGen.genRand.Next(0, list.Count)];
                    WayboundGenVars.SnowVillagePositionX = point.X; WayboundGenVars.SnowVillagePositionY = point.Y;
                    goto GenerateBuild;
                }
            }

            return false;

        GenerateBuild:
            NPC.NewNPC(new EntitySource_WorldGen(), (WayboundGenVars.SnowVillagePositionX + 13) * 16, (WayboundGenVars.SnowVillagePositionY - 11) * 16, NPCID.Merchant, 0, 0f, 0f, 0f, 0f, 255);

            int width = SnowVilageTiles.GetLength(1);
            int height = SnowVilageTiles.GetLength(0);

            WorldHelper.Cleaning(WayboundGenVars.SnowVillagePositionX + 3, WayboundGenVars.SnowVillagePositionY - 11, WayboundGenVars.SnowVillagePositionX + 100, WayboundGenVars.SnowVillagePositionY - 0, TileID.SnowBlock, TileID.IceBlock, TileID.Grass, TileID.Dirt);

            WorldHelper.Cleaning(WayboundGenVars.SnowVillagePositionX + 34, WayboundGenVars.SnowVillagePositionY - 11, WayboundGenVars.SnowVillagePositionX + 35, WayboundGenVars.SnowVillagePositionY - 10, TileID.SnowBlock, TileID.IceBlock, TileID.Trees, TileID.Grass, TileID.Dirt);
            WorldHelper.Cleaning(WayboundGenVars.SnowVillagePositionX + 7, WayboundGenVars.SnowVillagePositionY - 18, WayboundGenVars.SnowVillagePositionX + 56, WayboundGenVars.SnowVillagePositionY - 11, TileID.SnowBlock, TileID.IceBlock, TileID.Trees, TileID.Grass, TileID.Dirt); // - первый 2 домика
            WorldHelper.Cleaning(WayboundGenVars.SnowVillagePositionX + 57, WayboundGenVars.SnowVillagePositionY - 30, WayboundGenVars.SnowVillagePositionX + 77, WayboundGenVars.SnowVillagePositionY - 10, TileID.SnowBlock, TileID.IceBlock, TileID.Grass, TileID.Dirt); // - двор
            WorldHelper.Cleaning(WayboundGenVars.SnowVillagePositionX + 78, WayboundGenVars.SnowVillagePositionY - 20, WayboundGenVars.SnowVillagePositionX + 97, WayboundGenVars.SnowVillagePositionY - 9, TileID.SnowBlock, TileID.IceBlock, TileID.Trees, TileID.Grass, TileID.Dirt); // - последний дом

            for (int X = 0; X < width; X++) {
                for (int Y = 0; Y < height; Y++) {
                    int worldX = WayboundGenVars.SnowVillagePositionX + X;
                    int worldY = WayboundGenVars.SnowVillagePositionY - Y;

                    if (!WorldGen.InWorld(worldX, worldY, 10))
                        continue;

                    Tile tile = Framing.GetTileSafely(WayboundGenVars.SnowVillagePositionX + X, WayboundGenVars.SnowVillagePositionY - Y);
                    tile.ClearEverything();

                    switch (SnowVilageTiles[Y, X]) {
                        case 0: break;
                        case 1: tile.TileType = TileID.SnowBlock; tile.HasTile = true; break;
                        case 2: tile.TileType = TileID.IceBlock; tile.HasTile = true; break;
                        case 3: tile.TileType = TileID.StoneSlab; tile.HasTile = true; break;
                        case 4: tile.TileType = TileID.Stone; tile.HasTile = true; break;
                        case 5: tile.TileType = TileID.GrayBrick; tile.HasTile = true; break;
                        case 6: tile.TileType = TileID.Stone; tile.HasTile = true; break;
                        case 7: tile.TileType = TileID.GrayBrick; tile.HasTile = true; break;
                        case 8: tile.TileType = TileID.BorealBeam; tile.HasTile = true; break;
                        case 9: tile.TileType = TileID.LivingWood; tile.HasTile = true; break;
                        case a: WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + X, WayboundGenVars.SnowVillagePositionY - Y, TileID.Platforms, false, false, -1, 19); break;
                        case b: WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + X, WayboundGenVars.SnowVillagePositionY - Y, TileID.Platforms, false, false, -1, 19); break;
                        case c: WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + X, WayboundGenVars.SnowVillagePositionY - Y, TileID.Platforms, false, false, -1, 19); break;
                        case d: tile.TileType = TileID.WoodBlock; tile.HasTile = true; break;
                        case e: tile.TileType = TileID.BorealWood; tile.HasTile = true; break;
                        case f: tile.TileType = TileID.RedDynastyShingles; tile.HasTile = true; break;
                        case g: tile.TileType = TileID.Chain; tile.HasTile = true; break;
                        case h: tile.TileType = TileID.SilverBrick; tile.HasTile = true; break;
                        case i: tile.TileType = TileID.Diamond; tile.HasTile = true; break;
                        case j: WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + X, WayboundGenVars.SnowVillagePositionY - Y, TileID.Platforms, false, false, -1, 6); break;
                        case k: tile.TileType = TileID.AmberStoneBlock; tile.HasTile = true; break;
                        case l: tile.TileType = TileID.Ruby; tile.HasTile = true; break;
                    }
                    switch (SnowVilageWalls[Y, X]) {
                        case 0: WorldGen.KillWall(X, Y); break;
                        case 1: tile.WallType = WallID.WoodenFence; tile.WallColor = PaintID.BrownPaint; break;
                        case 2: tile.WallType = WallID.StoneSlab; break;
                        case 3: tile.WallType = WallID.PalmWoodFence; tile.WallColor = PaintID.BrownPaint; break;
                        case 4: tile.WallType = WallID.ShadewoodFence; tile.WallColor = PaintID.BrownPaint; break;
                        case 5: tile.WallType = WallID.GrayBrick; break;
                        case 6: tile.WallType = WallID.LivingWood; break;
                        case 7: tile.WallType = WallID.RichMahoganyFence; tile.WallColor = PaintID.BrownPaint; break;
                        case 8: tile.WallType = WallID.Wood; break;
                        case 9: tile.WallType = WallID.Planked; break;
                        case a: tile.WallType = WallID.Glass; break;
                        case b: tile.WallType = WallID.IceBrick; break;
                        case c: tile.WallType = WallID.WoodenFence; tile.WallColor = PaintID.BrownPaint; break;
                        case e: tile.WallType = WallID.Cloud; break;
                        case f: tile.WallType = WallID.TinBrick; break;
                    }
                    switch (SnowVilageSlopes[Y, X]) {
                        case 0: break;
                        case 1: tile.IsHalfBlock = true; break;
                        case 2: tile.Slope = SlopeType.SlopeDownRight; break;
                        case 3: tile.Slope = SlopeType.SlopeUpLeft; break;
                        case 4: tile.Slope = SlopeType.SlopeUpRight; break;
                        case 5: tile.Slope = SlopeType.SlopeDownLeft; break;
                    }
                    if (SnowVilageTiles[Y, X] != 0) {
                        WayboundGenVars.VillageTiles.Add(new Vector2(WayboundGenVars.SnowVillagePositionX + X, WayboundGenVars.SnowVillagePositionY - Y));
                    }
                    if (SnowVilageWalls[Y, X] != 0) {
                        WayboundGenVars.VillageWalles.Add(new Vector2(WayboundGenVars.SnowVillagePositionX + X, WayboundGenVars.SnowVillagePositionY - Y));
                    }
                }
            }

            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 86, WayboundGenVars.SnowVillagePositionY - 9, TileID.Lampposts);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 80, WayboundGenVars.SnowVillagePositionY - 9, TileID.Tables, mute: false, 28);
            // WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 66, WayboundGenVars.SnowVillagePositionY - 9, Valhalla.Find<ModTile>("Millstone").Type);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 5, WayboundGenVars.SnowVillagePositionY - 10, TileID.Statues, mute: false, 32);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 21, WayboundGenVars.SnowVillagePositionY - 11, TileID.Statues);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 62, WayboundGenVars.SnowVillagePositionY - 10, TileID.Lampposts);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 29, WayboundGenVars.SnowVillagePositionY - 10, TileID.Lampposts);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 56, WayboundGenVars.SnowVillagePositionY - 10, TileID.WaterFountain, mute: false, 3);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 27, WayboundGenVars.SnowVillagePositionY - 11, TileID.Anvils, mute: false, 2);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 18, WayboundGenVars.SnowVillagePositionY - 11, TileID.WorkBenches, mute: false, 23);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 16, WayboundGenVars.SnowVillagePositionY - 11, TileID.FishingCrate, mute: false, 18);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 9, WayboundGenVars.SnowVillagePositionY - 11,  TileID.Tables, mute: false, 28);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 11, WayboundGenVars.SnowVillagePositionY - 11, TileID.Chairs, mute: false, 30);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 13, WayboundGenVars.SnowVillagePositionY - 11, TileID.Beds, mute: false, 24);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 38, WayboundGenVars.SnowVillagePositionY - 11, TileID.Lamps);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 40, WayboundGenVars.SnowVillagePositionY - 11, TileID.Sawmill);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 42, WayboundGenVars.SnowVillagePositionY - 11, TileID.WorkBenches, mute: false, 23);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 45, WayboundGenVars.SnowVillagePositionY - 11, TileID.GrandfatherClocks, mute: false, 30);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 49, WayboundGenVars.SnowVillagePositionY - 11, TileID.Fireplace);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 90, WayboundGenVars.SnowVillagePositionY - 9, TileID.Lamps);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 92, WayboundGenVars.SnowVillagePositionY - 11, TileID.ClosedDoor, mute: false, 30);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 76, WayboundGenVars.SnowVillagePositionY - 11, TileID.ClosedDoor, mute: false, 30);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 78, WayboundGenVars.SnowVillagePositionY - 13, TileID.FishingCrate, mute: false, 1);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 42, WayboundGenVars.SnowVillagePositionY - 12, TileID.MusicBoxes, mute: false, 14);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 52, WayboundGenVars.SnowVillagePositionY - 12, TileID.ClosedDoor, mute: false, 30);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 22, WayboundGenVars.SnowVillagePositionY - 12, TileID.ClosedDoor, mute: false, 44);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 6, WayboundGenVars.SnowVillagePositionY - 12, TileID.ClosedDoor, mute: false, 30);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 10, WayboundGenVars.SnowVillagePositionY - 18, TileID.Furnaces);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 9, WayboundGenVars.SnowVillagePositionY - 16, TileID.Banners, false, 2);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 12, WayboundGenVars.SnowVillagePositionY - 16, TileID.Banners, false, 2);
            WorldGen.PlaceObject(WayboundGenVars.SnowVillagePositionX + 89, WayboundGenVars.SnowVillagePositionY - 12, TileID.LightningBuginaBottle);


            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 37, WayboundGenVars.SnowVillagePositionY - 11, TileID.ClayPot, 0);
            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 7, WayboundGenVars.SnowVillagePositionY - 13, TileID.Candles, 0);
            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 80, WayboundGenVars.SnowVillagePositionY - 15, TileID.Books, 0);
            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 81, WayboundGenVars.SnowVillagePositionY - 15, TileID.Books, 0);
            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 82, WayboundGenVars.SnowVillagePositionY - 15, TileID.Books, 0);
            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 42, WayboundGenVars.SnowVillagePositionY - 15, TileID.Books, 0);
            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 43, WayboundGenVars.SnowVillagePositionY - 15, TileID.Books, 0);
            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 19, WayboundGenVars.SnowVillagePositionY - 15, TileID.Books, 0);
            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 20, WayboundGenVars.SnowVillagePositionY - 15, TileID.Books, 0);
            WorldGen.Place1x1(WayboundGenVars.SnowVillagePositionX + 7, WayboundGenVars.SnowVillagePositionY - 15, TileID.Candles, 50);
            WorldGen.Place1x2(WayboundGenVars.SnowVillagePositionX + 78, WayboundGenVars.SnowVillagePositionY - 9, TileID.Chairs, 17);
            WorldGen.Place1x2(WayboundGenVars.SnowVillagePositionX + 51, WayboundGenVars.SnowVillagePositionY - 11, TileID.Chairs, 17);

            WorldGen.Place3x2(WayboundGenVars.SnowVillagePositionX + 83, WayboundGenVars.SnowVillagePositionY - 9, TileID.Dressers, 18);

            WorldGen.Place2x1(WayboundGenVars.SnowVillagePositionX + 79, WayboundGenVars.SnowVillagePositionY - 11, TileID.Bowls, 0);
            WorldGen.Place2x1(WayboundGenVars.SnowVillagePositionX + 8, WayboundGenVars.SnowVillagePositionY - 13, TileID.Bowls, 1);
            WorldGen.Place2x2(WayboundGenVars.SnowVillagePositionX + 88, WayboundGenVars.SnowVillagePositionY - 14, TileID.UlyssesButterflyJar, 0);
            WorldGen.Place2x2(WayboundGenVars.SnowVillagePositionX + 90, WayboundGenVars.SnowVillagePositionY - 14, TileID.Heart, 0);
            WorldGen.Place2x2(WayboundGenVars.SnowVillagePositionX + 49, WayboundGenVars.SnowVillagePositionY - 13, TileID.FishBowl, 0);
           // WorldGen.Place2x2(WayboundGenVars.SnowVillagePositionX + 84, WayboundGenVars.SnowVillagePositionY - 11, (ushort)TileType<OtherworldlyMusicBox2>(), 0);

            WorldGen.Place3x2(WayboundGenVars.SnowVillagePositionX + 25, WayboundGenVars.SnowVillagePositionY - 11, TileID.Blendomatic, 0);
            WorldGen.Place3x3(WayboundGenVars.SnowVillagePositionX + 85, WayboundGenVars.SnowVillagePositionY - 16, TileID.Chandeliers, 0);

            WorldGen.PlaceBanner(WayboundGenVars.SnowVillagePositionX + 91, WayboundGenVars.SnowVillagePositionY - 16, TileID.Banners, 124);
            WorldGen.PlaceBanner(WayboundGenVars.SnowVillagePositionX + 83, WayboundGenVars.SnowVillagePositionY - 16, TileID.Banners, 126);

            WorldGen.PlaceOnTable1x1(WayboundGenVars.SnowVillagePositionX + 81, WayboundGenVars.SnowVillagePositionY - 11, TileID.Bottles, 4);
            WorldGen.PlaceOnTable1x1(WayboundGenVars.SnowVillagePositionX + 82, WayboundGenVars.SnowVillagePositionY - 11, TileID.Candles, 0);
            WorldGen.PlaceOnTable1x1(WayboundGenVars.SnowVillagePositionX + 50, WayboundGenVars.SnowVillagePositionY - 13, TileID.Bottles, 1);
            WorldGen.PlaceOnTable1x1(WayboundGenVars.SnowVillagePositionX + 18, WayboundGenVars.SnowVillagePositionY - 12, TileID.Bottles, 1);
            WorldGen.PlaceOnTable1x1(WayboundGenVars.SnowVillagePositionX + 19, WayboundGenVars.SnowVillagePositionY - 12, TileID.Candles, 0);
            WorldGen.PlaceOnTable1x1(WayboundGenVars.SnowVillagePositionX + 10, WayboundGenVars.SnowVillagePositionY - 13, TileID.Bottles, 4);

            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 103, WayboundGenVars.SnowVillagePositionY - 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 103, WayboundGenVars.SnowVillagePositionY - 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 103, WayboundGenVars.SnowVillagePositionY + 0, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 103, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 103, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 104, WayboundGenVars.SnowVillagePositionY - 0, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 104, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 104, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 102, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 101, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 86, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 87, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 88, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 60, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 61, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 62, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 63, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 64, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 65, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 66, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 67, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 68, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 69, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 70, WayboundGenVars.SnowVillagePositionY + 2, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 103, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 102, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 101, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 100, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 99, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 98, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 97, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 92, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 91, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 90, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 89, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 88, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 87, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 86, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 85, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 84, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 72, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 71, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 70, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 69, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 68, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 67, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 66, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 65, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 64, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 63, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 62, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 61, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 60, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 59, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 58, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 57, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 56, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 55, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 54, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);
            WorldGen.PlaceTile(WayboundGenVars.SnowVillagePositionX + 53, WayboundGenVars.SnowVillagePositionY + 1, TileID.SnowBlock);

            int BarrelIndex = WorldGen.PlaceChest(WayboundGenVars.SnowVillagePositionX + 33, WayboundGenVars.SnowVillagePositionY - 9, TileID.Containers, false, 0);

            if (BarrelIndex != -1) { GenerateBarrelLoot(Main.chest[BarrelIndex].item, 0); }

            WayboundGenVars.SnowVillageGen = true;

            return true;
        }
        static void GenerateBarrelLoot(Item[] ChestInventory, int BarrelIndex)
        {
            ChestInventory[BarrelIndex].SetDefaults(Terraria.Utils.SelectRandom(WorldGen.genRand, ItemID.IceBlade, ItemID.IceBoomerang, ItemID.Snowball));
            ChestInventory[BarrelIndex].stack = ChestInventory[BarrelIndex].type == ItemID.Snowball ? 150 : 1; BarrelIndex++;
            WorldHelper.RandomLootInCoutainer(ChestInventory, ref BarrelIndex, 1, 1, ItemID.BlizzardinaBottle, ItemID.FlurryBoots, ItemID.IceSkates);
            WorldHelper.LootInContainers(ChestInventory, ref BarrelIndex, ItemID.Fish, 1, 1);
            WorldHelper.RandomLootInCoutainer(ChestInventory, ref BarrelIndex, 3, 7, ItemID.Topaz, ItemID.Amethyst, ItemID.Sapphire, ItemID.Amber, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond);
            WorldHelper.IfOreTireLoot(ChestInventory, ref BarrelIndex, 4, ItemID.GoldBar, ItemID.PlatinumBar, 5, 15);
            WorldHelper.LootInContainers(ChestInventory, ref BarrelIndex, ItemID.FlinxFur, 5, 10);
            WorldHelper.LootInContainers(ChestInventory, ref BarrelIndex, ItemID.HealingPotion, 3, 5);
            WorldHelper.LootInContainers(ChestInventory, ref BarrelIndex, ItemID.GoldCoin, 1, 3);
        }
    }
}