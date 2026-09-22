using System.IO;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Biome;
using Waybound.Common.NetCodeUtil;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Common.Water;

public class CryoFluid : ModWaterStyle
{
    public override string Texture => "Waybound/Assets/Textures/Water/CryoFluid";
    public override int ChooseWaterfallStyle() => GetInstance<CryoFluidFall>().Slot;
    public override int GetSplashDust() => DustID.IceTorch;
    public override int GetDropletGore() => GoreID.WaterDripIce;

    public override void LightColorMultiplier(ref float r, ref float g, ref float b)
    {
        r = 0.85f;
        g = 0.92f;
        b = 1.05f;
    }

    public override Color BiomeHairColor() => new Color(140, 210, 255);
}

public class CryoFluidParticles : ModSystem
{
    public override void PostUpdateEverything()
    {
        if (Main.dedServ)
            return;

        if (ParticleSystem.MegasparkBuffer == null)
            return;

        Player player = Main.LocalPlayer;
        if (!player.active || player.dead)
            return;

        if (!player.InModBiome<CryoLake>())
            return;

        int spawnAttempts = 100;
        for (int attempt = 0; attempt < spawnAttempts; attempt++)
        {
            if (!Main.rand.NextBool(2))
                continue;

            int radius = 34;
            int spawnX = (int)(player.Center.X / 16f) + Main.rand.Next(-radius, radius);
            int spawnY = (int)(player.Center.Y / 16f) + Main.rand.Next(-radius, radius);

            if (!WorldGen.InWorld(spawnX, spawnY, 10))
                continue;

            Tile tile = Framing.GetTileSafely(spawnX, spawnY);
            if (tile.LiquidAmount == 0 || tile.LiquidType != LiquidID.Water)
                continue;

            int aboveY = spawnY - 1;
            if (!WorldGen.InWorld(spawnX, aboveY, 10))
                continue;

            Tile above = Framing.GetTileSafely(spawnX, aboveY);
            if (above.LiquidAmount > 0)
                continue;

            Vector2 worldPos = new Vector2(spawnX * 16 + 8, spawnY * 16 + 4);

            Vector2 velocity = new Vector2(
                Main.rand.NextFloat(-0.2f, 0.2f),
                Main.rand.NextFloat(-0.6f, -0.2f)
            );

            float scale = Main.rand.NextFloat(16f, 36f);

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                worldPos.ToNumerics(),
                velocity.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(scale),
                new Color(150, 215, 255, 255),
                Main.rand.Next(50, 95)
            ));
        }
    }
}
public class CryoWaterTeleportPlayer : ModPlayer
{
    private bool wasInWater;

    public override void PostUpdate()
    {
        bool inWater = Collision.DrownCollision(Player.position, Player.width, Player.height, Player.gravDir);

        if (inWater && !wasInWater && Player.velocity.Y > 3f)
        {
            if (Player.InModBiome<CryoLake>())
            {
                TryTeleportToSnow();
            }
        }

        wasInWater = inWater;
    }

    private void TryTeleportToSnow()
    {
        if (!TryFindSnowTeleport(out Vector2 destination))
            return;

        Vector2 oldPos = Player.Center;

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            ExecuteTeleport(oldPos, destination);
        }
        else if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
        {
            MultiplayerSystem.SendPacket(new CryoPacket((byte)Player.whoAmI, destination));
        }
    }

    public void ExecuteTeleport(Vector2 oldPos, Vector2 destination)
    {
        Player.Teleport(destination, 1, 0);
        Player.velocity = Vector2.Zero;

        if (Main.netMode != NetmodeID.Server)
        {
            SpawnTeleportEffects(oldPos, destination);
        }
    }

    private void SpawnTeleportEffects(Vector2 oldPos, Vector2 newPos)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 spawnPos = oldPos + Main.rand.NextVector2Circular(24f, 24f);
            Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) - Vector2.UnitY * 1.5f;
            float scale = Main.rand.NextFloat(8f, 16f);

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                spawnPos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(scale),
                new Color(160, 220, 255, 255),
                Main.rand.Next(30, 55)
            ));
        }

        for (int i = 0; i < 40; i++)
        {
            Vector2 spawnPos = newPos + Main.rand.NextVector2Circular(40f, 40f);
            Vector2 vel = -Main.rand.NextVector2Circular(2f, 2f);
            float scale = Main.rand.NextFloat(10f, 20f);

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                spawnPos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(scale),
                new Color(150, 215, 255, 255),
                Main.rand.Next(35, 65)
            ));
        }

        SoundEngine.PlaySound(SoundID.Item8, oldPos);
    }

    private bool TryFindSnowTeleport(out Vector2 destination)
    {
        destination = Vector2.Zero;
        int attempts = 300;

        for (int i = 0; i < attempts; i++)
        {
            int x = Main.rand.Next(200, Main.maxTilesX - 200);
            int y = Main.rand.Next(100, Main.maxTilesY - 200);

            if (!WorldGen.InWorld(x, y, 10))
                continue;

            Tile tile = Main.tile[x, y];

            bool isSnow = tile.TileType == TileID.SnowBlock
                       || tile.TileType == TileID.IceBlock
                       || tile.TileType == TileID.BreakableIce;

            if (!isSnow || !tile.HasTile)
                continue;

            int belowY = y + 1;
            if (!WorldGen.InWorld(x, belowY, 10))
                continue;

            Tile below = Main.tile[x, belowY];
            if (!below.HasTile || !Main.tileSolid[below.TileType])
                continue;

            Tile above1 = Main.tile[x, y - 1];
            Tile above2 = Main.tile[x, y - 2];
            if (above1.HasTile || above2.HasTile)
                continue;

            if (tile.LiquidAmount > 0 || above1.LiquidAmount > 0)
                continue;

            destination = new Vector2(x * 16f + 8f, y * 16f);
            return true;
        }

        return false;
    }
}

internal sealed class CryoPacket : NetPacket
{
    private byte playerIndex;
    private Vector2 destination;

    public CryoPacket() { }

    public CryoPacket(byte playerIndex, Vector2 destination)
    {
        this.playerIndex = playerIndex;
        this.destination = destination;

        Writer.Write(playerIndex);
        Writer.WriteVector2(destination);
    }

    public override void Read(BinaryReader reader, int sender)
    {
        byte idx = reader.ReadByte();
        Vector2 dest = reader.ReadVector2();

        if (Main.netMode == NetmodeID.Server)
        {
            MultiplayerSystem.SendPacket(new CryoPacket(idx, dest), -1, sender);
            return;
        }

        Player target = Main.player[idx];
        if (target.active)
        {
            CryoWaterTeleportPlayer modPlayer = target.GetModPlayer<CryoWaterTeleportPlayer>();
            Vector2 oldPos = target.Center;
            modPlayer.ExecuteTeleport(oldPos, dest);
        }
    }
}