// Code by SerNik
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Waybound.Helpers
{
    public static class WayboundHelper
    {
        public static bool ClosestNPC(ref NPC target, Vector2 position, float maxDistance = 0, int type = 0,
            bool ignoreTiles = false, bool withoutRepeat = false, NPC[] hittedNPC = null, int maxHittedNPC = -1,
            float[] npcAI = null, int whoAmI = -1)
        {
            bool foundTarget = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                float distance = (npc.Center - position).Length();
                if (npc == null || npc.life <= 0 || !npc.active || !npc.CanBeChasedBy() ||
                    (type != npc.type && type != 0) || (distance >= maxDistance && maxDistance > 0) ||
                    (!Collision.CanHit(position, 0, 0, npc.Center, 0, 0) && !ignoreTiles) ||
                    (whoAmI == npc.whoAmI && whoAmI != -1))
                    continue;
                bool no = false;
                if (withoutRepeat)
                    for (int j = 0; j < maxHittedNPC; j++)
                        if (hittedNPC[j] == npc)
                            no = true;
                if (npcAI != null)
                    for (int g = 0; g <= 3; g++)
                        if (npcAI[g] != npc.ai[g] && npcAI[g] != -1)
                            no = true;
                if (!no)
                {
                    target = npc;
                    foundTarget = true;
                    maxDistance = distance;
                }
            }

            return foundTarget;
        }

        public static bool ClosestProj(ref Projectile target, Vector2 position, float maxDistance = 0, int type = 0,
            bool ignoreTiles = false, int whoAmI = -1, float[] projAI = null)
        {
            bool foundTarget = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                float distance = (proj.Center - position).Length();
                if ((type != proj.type && type != 0) || (distance >= maxDistance && maxDistance > 0) ||
                    (!Collision.CanHit(position, 0, 0, proj.Center, 0, 0) && !ignoreTiles) ||
                    (whoAmI == proj.whoAmI && whoAmI != -1))
                    continue;
                bool no = false;
                if (projAI != null)
                    for (int g = 0; g <= 1; g++)
                        if (projAI[g] != proj.ai[g] && projAI[g] != -1)
                            no = true;
                if (!no)
                {
                    target = proj;
                    foundTarget = true;
                    maxDistance = distance;
                }
            }

            return foundTarget;
        }
        public static bool IsOnPlatformNPC(this Terraria.NPC npc, Vector2 offset)
        {
            int tileBY = (int)((npc.Bottom.Y + offset.Y) / 16);
            int tileBX1 = (int)((npc.Left.X - offset.X) / 16);
            int tileBX2 = (int)((npc.Right.X + offset.X) / 16);
            for (int tileBX = tileBX1; tileBX < tileBX2; tileBX++)
            {
                if ((Main.tileSolidTop[Framing.GetTileSafely(tileBX, tileBY).TileType] ||
                     TileID.Sets.Platforms[Framing.GetTileSafely(tileBX, tileBY).TileType]) &&
                    Main.tile[tileBX, tileBY].HasTile)
                {
                    if (!Main.tileSolidTop[Framing.GetTileSafely(tileBX, tileBY).TileType] &&
                        Main.tileSolid[Framing.GetTileSafely(tileBX, tileBY).TileType] &&
                        !TileID.Sets.Platforms[Framing.GetTileSafely(tileBX, tileBY).TileType] &&
                        Main.tile[tileBX, tileBY].HasTile)
                    {
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }
        public static bool HasTileOnSide(this Terraria.NPC npc, int side, Vector2 offset, bool ignoreTopSolid = false)
        {
            if (side > 4)
            {
                side = 1;
            }

            switch (side)
            {
                case 1: //left
                    int tileLX = (int)((npc.Left.X - offset.X) / 16);
                    int tileLY1 = (int)((npc.Top.Y - offset.Y) / 16);
                    int tileLY2 = (int)((npc.Bottom.Y + offset.Y) / 16);
                    for (int tileLY = tileLY1; tileLY < tileLY2; tileLY++)
                    {
                        if (Main.tileSolid[Framing.GetTileSafely(tileLX, tileLY).TileType] &&
                            Main.tile[tileLX, tileLY].HasTile &&
                            (!Main.tileSolidTop[Framing.GetTileSafely(tileLX, tileLY).TileType] && ignoreTopSolid))
                            return true;
                        else if (Main.tileSolid[Framing.GetTileSafely(tileLX, tileLY).TileType] &&
                                 Main.tile[tileLX, tileLY].HasTile && !ignoreTopSolid)
                            return true;
                    }

                    break;
                case 2: //right
                    int tileRX = (int)((npc.Right.X + offset.X) / 16);
                    int tileRY1 = (int)((npc.Top.Y - offset.Y) / 16);
                    int tileRY2 = (int)((npc.Bottom.Y + offset.Y) / 16);
                    for (int tileRY = tileRY1; tileRY < tileRY2; tileRY++)
                    {
                        if (Main.tileSolid[Framing.GetTileSafely(tileRX, tileRY).TileType] &&
                            Main.tile[tileRX, tileRY].HasTile &&
                            (!Main.tileSolidTop[Framing.GetTileSafely(tileRX, tileRY).TileType] && ignoreTopSolid))
                            return true;
                        else if (Main.tileSolid[Framing.GetTileSafely(tileRX, tileRY).TileType] &&
                                 Main.tile[tileRX, tileRY].HasTile && !ignoreTopSolid)
                            return true;
                    }

                    break;
                case 3: //top
                    int tileTY = (int)((npc.Top.Y - offset.Y) / 16);
                    int tileTX1 = (int)((npc.Left.X - offset.X) / 16);
                    int tileTX2 = (int)((npc.Right.X + offset.X) / 16);
                    for (int tileTX = tileTX1; tileTX < tileTX2; tileTX++)
                    {
                        if (Main.tileSolid[Framing.GetTileSafely(tileTX, tileTY).TileType] &&
                            Main.tile[tileTX, tileTY].HasTile &&
                            (!Main.tileSolidTop[Framing.GetTileSafely(tileTX, tileTY).TileType] && ignoreTopSolid))
                            return true;
                        else if (Main.tileSolid[Framing.GetTileSafely(tileTX, tileTY).TileType] &&
                                 Main.tile[tileTX, tileTY].HasTile && !ignoreTopSolid)
                            return true;
                    }

                    break;
                case 4: //bottom
                    int tileBY = (int)((npc.Bottom.Y + offset.Y) / 16);
                    int tileBX1 = (int)((npc.Left.X - offset.X) / 16);
                    int tileBX2 = (int)((npc.Right.X + offset.X) / 16);
                    for (int tileBX = tileBX1; tileBX < tileBX2; tileBX++)
                    {
                        if (Main.tileSolid[Framing.GetTileSafely(tileBX, tileBY).TileType] &&
                            Main.tile[tileBX, tileBY].HasTile &&
                            (!Main.tileSolidTop[Framing.GetTileSafely(tileBX, tileBY).TileType] && ignoreTopSolid))
                            return true;
                        else if (Main.tileSolid[Framing.GetTileSafely(tileBX, tileBY).TileType] &&
                                 Main.tile[tileBX, tileBY].HasTile && !ignoreTopSolid)
                            return true;
                    }

                    break;
            }

            return false;
        }
    }
}