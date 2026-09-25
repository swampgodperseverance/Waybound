using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Waybound.Common.WUtils;
using Waybound.Content.Tiles.Blocks;
using Waybound.Content.Tiles.Ores;

namespace Waybound.Common.ModSystems.WorldGens; 
public class CryoSpringGen : BaseWorldGens {
    public override string NameGen => "Cryo Spring";
    public override string VanillaIndexName => "Ice";
    public override int Index => 1;
    public override bool GensBool { get; set; }

    public override bool Do_MakeGen(GenerationProgress progress) {
        progress.Message = Loc.GetChat("WorldGen.CryoSpring");

        int springsToCreate = Main.maxTilesX > 6000 ? 3 : 2;
        int created = 0;

        for (int attempt = 0; attempt < 15000 && created < springsToCreate; attempt++) {
            int x = WorldGen.genRand.Next(200, Main.maxTilesX - 200);
            int y = WorldGen.genRand.Next((int)GenVars.rockLayerLow + 80, Main.maxTilesY - 350);

            Tile tile = Framing.GetTileSafely(x, y);
            if (!tile.HasTile) { continue; };
            if (tile.TileType != TileID.SnowBlock && tile.TileType != TileID.IceBlock && tile.TileType != TileID.Slush) { continue; };
            if (TryCreateBigSpring(x, y, created == 1)) { created++; };
        };

        return created > 0;
    }
    static bool TryCreateBigSpring(int centerX, int centerY, bool big = false)
    {
        int radiusX = WorldGen.genRand.Next(22, 32);
        int radiusY = WorldGen.genRand.Next(14, 20);

        for (int i = -radiusX - 6; i <= radiusX + 6; i++)
        {
            for (int j = -radiusY - 8; j <= radiusY + 8; j++)
            {
                int x = centerX + i;
                int y = centerY + j;
                if (!WorldGen.InWorld(x, y, 30))
                    return false;

                Tile t = Framing.GetTileSafely(x, y);
                if (t.HasTile && Main.tileSolid[t.TileType]
                    && t.TileType != TileID.SnowBlock
                    && t.TileType != TileID.IceBlock
                    && t.TileType != TileID.Slush
                    && t.TileType != TileID.Dirt
                    && t.TileType != TileID.Stone
                    && t.TileType != TileID.ClayBlock)
                    return false;
            }
        }

        WayboundGenVars.CryoSpringPos.Add(new(centerX - radiusX - 4, centerX + radiusX + 4, centerY - radiusY - 5, centerY + radiusY + 5));

        CarveSpring(centerX, centerY, radiusX, radiusY);
        PlaceSnowShell(centerX, centerY, radiusX, radiusY);
        PlaceOreShell(centerX, centerY, radiusX, radiusY);

        if (big)
        {
            int startPos = centerX;
            int center = 0;
            bool left = true;

            int secondX = centerX + 120;
            for (int i = -radiusX - 6; i <= radiusX + 6; i++)
            {
                for (int j = -radiusY - 8; j <= radiusY + 8; j++)
                {
                    int x = secondX + i;
                    int y = centerY + j;
                    Tile t = Framing.GetTileSafely(x, y);
                    if (t.HasTile && Main.tileSolid[t.TileType]
                        && t.TileType != TileID.SnowBlock
                        && t.TileType != TileID.IceBlock
                        && t.TileType != TileID.Slush
                        && t.TileType != TileID.Dirt
                        && t.TileType != TileID.Stone
                        && t.TileType != TileID.ClayBlock)
                    {
                        left = false;
                    }
                }
            }

            if (!left)
                secondX = centerX - 120;

            int count = System.Math.Abs(startPos - secondX);
            WayboundGenVars.CryoSpringPos.Add(new(secondX - radiusX - 4, secondX + radiusX + 4, centerY - radiusY - 5, centerY + radiusY + 5));

            CarveSpring(secondX, centerY, radiusX, radiusY);
            PlaceSnowShell(secondX, centerY, radiusX, radiusY);
            PlaceOreShell(secondX, centerY, radiusX, radiusY);

            for (int i = 0; i < count; i++)
            {
                int jCount = WorldGen.genRand.Next(7, 12);
                for (int j = 0; j < jCount; j++)
                {
                    int pos = left ? startPos + i : startPos - i;
                    WorldGen.KillTile(pos, centerY - 2 - j);
                    if (i == count / 2)
                        center = pos;
                }
            }

            int k = 0;
            while (center + k > 0 && center + k < Main.maxTilesX && !Main.tile[center + k, centerY + 1].HasTile)
                k++;

            int tile = Main.tile[center + k, centerY - 1].TileType;
            WorldGen.PlaceTile(center, centerY - 1, tile);
            WorldGen.PlaceTile(center + 1, centerY - 1, tile);
            WorldGen.PlaceTile(center - 1, centerY - 1, tile);

            int chestIndex = WorldGen.PlaceChest(center - 1, centerY - 2);
            if (chestIndex >= 0)
                ChestLoot(Main.chest[chestIndex].item, 0);
        }

        return true;
    }

    static void CarveSpring(int centerX, int centerY, int radiusX, int radiusY)
    {
        for (int i = -radiusX - 4; i <= radiusX + 4; i++)
        {
            for (int j = -radiusY - 5; j <= radiusY + 5; j++)
            {
                int x = centerX + i;
                int y = centerY + j;
                if (!WorldGen.InWorld(x, y))
                    continue;

                float dx = i / (float)radiusX;
                float dy = j / (float)radiusY;
                float dist = dx * dx + dy * dy;
                if (j < 0)
                    dist *= 0.75f + WorldGen.genRand.NextFloat(0f, 0.25f);
                if (dist > 1.15f)
                    continue;

                Tile t = Framing.GetTileSafely(x, y);
                t.HasTile = false;
                t.LiquidAmount = 0;
                t.LiquidType = LiquidID.Water;
                t.WallType = WallID.None;
            }
        }

        for (int i = -radiusX; i <= radiusX; i++)
        {
            for (int j = 1; j <= radiusY + 2; j++)
            {
                int x = centerX + i;
                int y = centerY + j;
                if (!WorldGen.InWorld(x, y))
                    continue;

                float dx = i / (float)radiusX;
                float dy = j / (float)(radiusY + 1);
                if (dx * dx + dy * dy > 0.95f)
                    continue;

                Tile t = Framing.GetTileSafely(x, y);
                t.HasTile = false;
                t.LiquidAmount = 255;
                t.LiquidType = LiquidID.Water;
            }
        }
    }

    static void PlaceOreShell(int centerX, int centerY, int radiusX, int radiusY)
    {
        ushort oreType = (ushort)ModContent.TileType<HielitiumOre>();

        for (int i = -radiusX - 8; i <= radiusX + 8; i++)
        {
            for (int j = -radiusY - 8; j <= radiusY + 8 + 3; j++)
            {
                int x = centerX + i;
                int y = centerY + j;
                if (!WorldGen.InWorld(x, y, 10))
                    continue;

                float dx = i / (float)(radiusX + 1);
                float dy = j / (float)(radiusY + 1);
                float dist = dx * dx + dy * dy;
                if (j < 0)
                    dist *= 0.82f + WorldGen.genRand.NextFloat(0f, 0.18f);

                if (dist > 1.55f || dist < 1.32f)
                    continue;

                Tile t = Framing.GetTileSafely(x, y);
                if (t.HasTile && Main.tileSolid[t.TileType]
                    && t.TileType != TileID.Dirt
                    && t.TileType != TileID.Stone
                    && t.TileType != TileID.ClayBlock
                    && t.TileType != TileID.SnowBlock
                    && t.TileType != TileID.IceBlock
                    && t.TileType != TileID.Slush)
                    continue;

                if (!WorldGen.genRand.NextBool(2))
                    continue;

                t.HasTile = true;
                t.TileType = oreType;
                t.LiquidAmount = 0;
                t.Slope = 0;
                t.IsHalfBlock = false;
            }
        }
    }
    static void PlaceSnowShell(int centerX, int centerY, int radiusX, int radiusY)
    {
        int shellOuter = 5;
        for (int i = -radiusX - shellOuter; i <= radiusX + shellOuter; i++)
        {
            for (int j = -radiusY - shellOuter; j <= radiusY + shellOuter + 3; j++)
            {
                int x = centerX + i;
                int y = centerY + j;
                if (!WorldGen.InWorld(x, y, 10)) { continue; }
                float dx = i / (float)(radiusX + 1);
                float dy = j / (float)(radiusY + 1);
                float dist = dx * dx + dy * dy;
                if (j < 0) { dist *= 0.82f + WorldGen.genRand.NextFloat(0f, 0.18f); }
                if (dist > 1.35f) { continue; }
                if (dist < 0.92f) { continue; }
                Tile t = Framing.GetTileSafely(x, y);
                if (t.HasTile && Main.tileSolid[t.TileType] && t.TileType != TileID.Dirt && t.TileType != TileID.Stone && t.TileType != TileID.ClayBlock) { continue; }
                int tileType = WorldGen.genRand.NextBool(2) ? ModContent.TileType<SpringbrickTile>() : TileID.SnowBlock;
                t.HasTile = true;
                t.TileType = (ushort)tileType;
                t.LiquidAmount = 0;
                t.Slope = 0;
                t.IsHalfBlock = false;
            }
        }
    }
    
    static void ChestLoot(Item[] inv, int indexItem = 0) {
        WUtils.Chest loot = new(inv, indexItem);
        loot.SetItem(Tables.Set.IceLoot);
    }
};
public class CryoParticleSystem : ModSystem {
    public override void PostUpdateWorld() {
        //Main.NewText(WayboundGenVars.CryoSpringPos[0].Start.ToString() +" " + Main.LocalPlayer.position.ToString());
        //Main.LocalPlayer.position = WayboundGenVars.CryoSpringPos[0].End * 16;
        //if (Main.dedServ) return;
        //if (Main.GameUpdateCount % 3 != 0) return;

        //int startX = (int)(Main.screenPosition.X / 16f) - 2;
        //int endX = (int)((Main.screenPosition.X + Main.screenWidth) / 16f) + 2;
        //int startY = (int)(Main.screenPosition.Y / 16f) - 2;
        //int endY = (int)((Main.screenPosition.Y + Main.screenHeight) / 16f) + 2;

        //int cryoType = ModContent.GetInstance<CryoFluid>().Slot;

        //for (int x = startX; x <= endX; x++)
        //{
        //    for (int y = startY; y <= endY; y++)
        //    {
        //        if (!WorldGen.InWorld(x, y)) continue;
        //        Tile tile = Framing.GetTileSafely(x, y);

        //        if (tile.LiquidAmount < 60 || tile.LiquidType != cryoType) continue;
        //        if (!Main.rand.NextBool(10)) continue;

        //        Vector2 pos = new Vector2(x * 16 + Main.rand.Next(2, 15), y * 16 + Main.rand.Next(1, 9));
        //        Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.9f, -0.15f));

        //        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
        //            position: pos.ToNumerics(),
        //            velocity: vel.ToNumerics(),
        //            rotation: Main.rand.NextFloat(MathHelper.TwoPi),
        //            scale: new System.Numerics.Vector2(Main.rand.NextFloat(0.45f, 0.9f)),
        //            color: new Color(110, 190, 255, 180) * Main.rand.NextFloat(0.75f, 1.05f),
        //            duration: Main.rand.Next(40, 65)
        //        ));
        //    }
    }
};