using Terraria;
using Terraria.GameContent;
using Waybound.Common.GlobalPlayer;

namespace Waybound.Common {
    public class BloodBarNPC : GlobalNPC {
        public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position) {
            //if (!npc.friendly) {
            //    BloodyNecklacePlayer modPlayer = Main.LocalPlayer.GetModPlayer<BloodyNecklacePlayer>();
            //    if (modPlayer.npcIndex == -1) { return base.DrawHealthBar(npc, hbPosition, ref scale, ref position); };
            //    if (modPlayer.WorkTime <= 0) { return base.DrawHealthBar(npc, hbPosition, ref scale, ref position); };
            //    if (Main.npc[modPlayer.npcIndex].whoAmI == npc.whoAmI) {
            //        int barProgress = (int)(TextureAssets.Hb1.Value.Width * Utils.UI.GetProgress(modPlayer.WorkTime, 300));
            //        int scaleY = npc.life == npc.lifeMax ? 0 : 12;
            //        float num3 = position.X - 18f * scale;
            //        float num4 = position.Y;

            //        if (Main.LocalPlayer.gravDir == -1f) {
            //            num4 -= Main.screenPosition.Y;
            //            num4 = Main.screenPosition.Y + (float)Main.screenHeight - num4;
            //        }

            //        Main.spriteBatch.Draw(TextureAssets.Hb2.Value, new Vector2(num3 - Main.screenPosition.X, num4 - Main.screenPosition.Y + scaleY), null, Color.PaleVioletRed, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
            //        Main.spriteBatch.Draw(TextureAssets.Hb1.Value, new Vector2(num3 - Main.screenPosition.X, num4 - Main.screenPosition.Y + scaleY), new Rectangle(0, 0, barProgress, TextureAssets.Hb1.Height()), Color.IndianRed, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 1f);
            //    }
            //}
            return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
        }
    }
}