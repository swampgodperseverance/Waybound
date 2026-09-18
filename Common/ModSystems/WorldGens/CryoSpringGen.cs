using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using Waybound.Common.ModSystems.WorldGens;
using Waybound.Common.Water;
using Waybound.Particles;

namespace Waybound.Content.WorldGen
{
    public class CryoSpringGen : BaseWorldGens
    {
        public override string NameGen => "Cryo Spring";
        public override string VanillaIndexName => "Ice";
        public override int Index => 1;
        public override bool GensBool { get; set; }

        public override bool Do_MakeGen(GenerationProgress progress)
        {
            progress.Message = "Forming the Cryo Springs";

            int springsToCreate = Main.maxTilesX > 6000 ? 3 : 2;
            int created = 0;

            for (int attempt = 0; attempt < 15000 && created < springsToCreate; attempt++)
            {
                int x = Terraria.WorldGen.genRand.Next(200, Main.maxTilesX - 200);
                int y = Terraria.WorldGen.genRand.Next((int)GenVars.rockLayerLow + 80, Main.maxTilesY - 350);

                Tile tile = Framing.GetTileSafely(x, y);
                if (!tile.HasTile) continue;
                if (tile.TileType != TileID.SnowBlock && tile.TileType != TileID.IceBlock && tile.TileType != TileID.Slush)
                    continue;

                if (TryCreateBigSpring(x, y))
                    created++;
            }

            return created > 0;
        }

        private bool TryCreateBigSpring(int centerX, int centerY)
        {
            int radiusX = Terraria.WorldGen.genRand.Next(22, 32);
            int radiusY = Terraria.WorldGen.genRand.Next(14, 20);

            for (int i = -radiusX - 6; i <= radiusX + 6; i++)
            {
                for (int j = -radiusY - 8; j <= radiusY + 8; j++)
                {
                    int x = centerX + i;
                    int y = centerY + j;
                    if (!Terraria.WorldGen.InWorld(x, y, 30))
                        return false;

                    Tile t = Framing.GetTileSafely(x, y);
                    if (t.HasTile && Main.tileSolid[t.TileType] &&
                        t.TileType != TileID.SnowBlock && t.TileType != TileID.IceBlock && t.TileType != TileID.Slush &&
                        t.TileType != TileID.Dirt && t.TileType != TileID.Stone && t.TileType != TileID.ClayBlock)
                        return false;
                }
            }

            for (int i = -radiusX - 4; i <= radiusX + 4; i++)
            {
                for (int j = -radiusY - 5; j <= radiusY + 5; j++)
                {
                    int x = centerX + i;
                    int y = centerY + j;
                    if (!Terraria.WorldGen.InWorld(x, y)) continue;

                    float dx = i / (float)radiusX;
                    float dy = j / (float)radiusY;
                    float dist = dx * dx + dy * dy;

                    if (j < 0)
                        dist *= 0.75f + Terraria.WorldGen.genRand.NextFloat(0f, 0.25f);

                    if (dist > 1.15f) continue;

                    Tile t = Framing.GetTileSafely(x, y);
                    t.HasTile = false;
                    t.LiquidAmount = 0;
                    t.LiquidType = 0;
                    t.WallType = 0;
                }
            }

            for (int i = -radiusX; i <= radiusX; i++)
            {
                for (int j = 1; j <= radiusY + 2; j++)
                {
                    int x = centerX + i;
                    int y = centerY + j;
                    if (!Terraria.WorldGen.InWorld(x, y)) continue;

                    float dx = i / (float)radiusX;
                    float dy = j / (float)(radiusY + 1);
                    if (dx * dx + dy * dy > 0.95f) continue;

                    Tile t = Framing.GetTileSafely(x, y);
                    t.HasTile = false;
                    t.LiquidAmount = 255;
                    t.LiquidType = 0;
                    CryoWaterSystem.RegisterTile(x, y);
                }
            }

            PlaceSnowShell(centerX, centerY, radiusX, radiusY);

            return true;
        }

        private void PlaceSnowShell(int centerX, int centerY, int radiusX, int radiusY)
        {
            int shellOuter = 5;
            for (int i = -radiusX - shellOuter; i <= radiusX + shellOuter; i++)
            {
                for (int j = -radiusY - shellOuter; j <= radiusY + shellOuter + 3; j++)
                {
                    int x = centerX + i;
                    int y = centerY + j;
                    if (!Terraria.WorldGen.InWorld(x, y, 10)) continue;
                    float dx = i / (float)(radiusX + 1);
                    float dy = j / (float)(radiusY + 1);
                    float dist = dx * dx + dy * dy;
                    if (j < 0)
                        dist *= 0.82f + Terraria.WorldGen.genRand.NextFloat(0f, 0.18f);
                    if (dist > 1.35f) continue;
                    if (dist < 0.92f) continue;
                    Tile t = Framing.GetTileSafely(x, y);
                    if (t.HasTile && Main.tileSolid[t.TileType] &&
                        t.TileType != TileID.Dirt && t.TileType != TileID.Stone && t.TileType != TileID.ClayBlock)
                        continue;
                    int tileType = Terraria.WorldGen.genRand.NextBool(7)
                        ? TileID.IceBrick
                        : TileID.SnowBlock;
                    t.HasTile = true;
                    t.TileType = (ushort)tileType;
                    t.LiquidAmount = 0;
                    t.Slope = 0;
                    t.IsHalfBlock = false;
                }
            }
        }
    }
        public class CryoWaterSystem : ModSystem
    {
        private static readonly List<Point> _pending = new();
        private static int[] _xs = Array.Empty<int>();
        private static int[] _ys = Array.Empty<int>();

        public static void RegisterTile(int x, int y)
        {
            _pending.Add(new Point(x, y));
        }

        public override void OnWorldLoad()
        {
            if (Main.dedServ) return;

            int cryoType = ModContent.GetInstance<CryoFluid>().Slot;

            ApplyTo(_pending, cryoType);

            for (int i = 0; i < _xs.Length; i++)
            {
                if (!Terraria.WorldGen.InWorld(_xs[i], _ys[i])) continue;
                Tile t = Framing.GetTileSafely(_xs[i], _ys[i]);
                if (t.LiquidAmount > 0)
                    t.LiquidType = cryoType;
            }
        }

        public override void OnWorldUnload()
        {
            _pending.Clear();
            _xs = Array.Empty<int>();
            _ys = Array.Empty<int>();
        }

        private static void ApplyTo(List<Point> list, int cryoType)
        {
            foreach (var p in list)
            {
                if (!Terraria.WorldGen.InWorld(p.X, p.Y)) continue;
                Tile t = Framing.GetTileSafely(p.X, p.Y);
                if (t.LiquidAmount > 0)
                    t.LiquidType = cryoType;
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var all = new List<Point>(_pending);
            for (int i = 0; i < _xs.Length; i++)
                all.Add(new Point(_xs[i], _ys[i]));

            if (all.Count == 0) return;

            int[] xs = new int[all.Count];
            int[] ys = new int[all.Count];
            for (int i = 0; i < all.Count; i++)
            {
                xs[i] = all[i].X;
                ys[i] = all[i].Y;
            }

            tag["cryoX"] = xs;
            tag["cryoY"] = ys;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (!tag.ContainsKey("cryoX") || !tag.ContainsKey("cryoY")) return;

            _xs = tag.GetIntArray("cryoX");
            _ys = tag.GetIntArray("cryoY");

            _pending.Clear();
        }

        public override void ClearWorld()
        {
            _pending.Clear();
            _xs = Array.Empty<int>();
            _ys = Array.Empty<int>();
        }
    }

    public class CryoParticleSystem : ModSystem
    {
        public override void PostUpdateWorld()
        {
            if (Main.dedServ) return;
            if (Main.GameUpdateCount % 3 != 0) return;

            int startX = (int)(Main.screenPosition.X / 16f) - 2;
            int endX = (int)((Main.screenPosition.X + Main.screenWidth) / 16f) + 2;
            int startY = (int)(Main.screenPosition.Y / 16f) - 2;
            int endY = (int)((Main.screenPosition.Y + Main.screenHeight) / 16f) + 2;

            int cryoType = ModContent.GetInstance<CryoFluid>().Slot;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (!Terraria.WorldGen.InWorld(x, y)) continue;
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (tile.LiquidAmount < 60 || tile.LiquidType != cryoType) continue;
                    if (!Main.rand.NextBool(10)) continue;

                    Vector2 pos = new Vector2(x * 16 + Main.rand.Next(2, 15), y * 16 + Main.rand.Next(1, 9));
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.9f, -0.15f));

                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        position: pos.ToNumerics(),
                        velocity: vel.ToNumerics(),
                        rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                        scale: new System.Numerics.Vector2(Main.rand.NextFloat(0.45f, 0.9f)),
                        color: new Color(110, 190, 255, 180) * Main.rand.NextFloat(0.75f, 1.05f),
                        duration: Main.rand.Next(40, 65)
                    ));
                }
            }
        }
    }

}