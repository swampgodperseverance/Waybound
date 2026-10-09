using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using System.IO;
using Terraria;
using Terraria.ID;
using Waybound.Common.Biome;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.NetCodeUtil;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Common.Biome.Water;

public class CryoFluid : WayboundWater {
    public override string Texture => GetTexturePath();
    public override int ChooseWaterfallStyle() => GetInstance<CryoFluidFall>().Slot;
    public override int GetSplashDust() => DustID.IceTorch;
    public override int GetDropletGore() => GoreID.WaterDripIce;
    public override Color BiomeHairColor() => Color.White;
    public override void Draw(SpriteBatch spriteBatch, Vector2 pos, bool beginSpriteBatch) {
        spriteBatch.End();

        Effect effect = Resources.Effects.IceShimer.Value;
        effect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly * 1.44f);
        effect.Parameters["alpha"].SetValue(Main.LocalPlayer.GetModPlayer<IceShimerPlayer>().Alpha);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Main.Transform);
        spriteBatch.Draw(Main.waterTarget, pos, color: Color.White);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
    }
}

public class CryoFluidParticles : ModSystem
{
    public override void PostUpdateEverything()
    {
        if (Main.dedServ) { return; }
        if (ParticleSystem.MegasparkBuffer == null) { return; }
        Player player = Main.LocalPlayer;
        if (!player.active || player.dead) { return; }
        if (!player.InModBiome<CryoLake>()) { return; }
        int spawnAttempts = 100;
        for (int attempt = 0; attempt < spawnAttempts; attempt++)
        {
            if (!Main.rand.NextBool(2)) { continue; }

            int radius = 34;
            int spawnX = (int)(player.Center.X / 16f) + Main.rand.Next(-radius, radius);
            int spawnY = (int)(player.Center.Y / 16f) + Main.rand.Next(-radius, radius);

            if (!WorldGen.InWorld(spawnX, spawnY, 10)) { continue; }
            Tile tile = Framing.GetTileSafely(spawnX, spawnY);
            if (tile.LiquidAmount == 0 || tile.LiquidType != LiquidID.Water) { continue; }
            int aboveY = spawnY - 1;
            if (!WorldGen.InWorld(spawnX, aboveY, 10)) { continue; }
            Tile above = Framing.GetTileSafely(spawnX, aboveY);
            if (above.LiquidAmount > 0) { continue; }
            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(new Vector2(spawnX * 16 + 8, spawnY * 16 + 4).ToNumerics(), new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-0.6f, -0.2f)).ToNumerics(), Main.rand.NextFloat(MathHelper.TwoPi), new SysVector2(Main.rand.NextFloat(16f, 36f)), new Color(150, 215, 255, 255), Main.rand.Next(50, 95)));
        }
    }
}
internal sealed class CryoPacket : NetPacket {
    public CryoPacket(byte playerIndex, Vector2 destination) {
        Writer.Write(playerIndex);
        Writer.WriteVector2(destination);
    }

    public override void Read(BinaryReader reader, int sender) {
        byte idx = reader.ReadByte();
        Vector2 dest = reader.ReadVector2();

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new CryoPacket(idx, dest), -1, sender);
            return;
        }

        Player target = Main.player[idx];
        if (target.active) {
            IceShimerPlayer modPlayer = target.GetModPlayer<IceShimerPlayer>();
            Vector2 oldPos = target.Center;
            modPlayer.ExecuteTeleport(oldPos, dest);
        }
    }
}