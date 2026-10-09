using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Waybound.Content.NPCs.Bosses.Korochun;

namespace Waybound.Content.Items.BossSummon
{
    public class IcePenny : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.25f, 0.45f, 0.7f);
            if (!Main.dedServ && Main.rand.NextBool(8))
            {
                Dust d = Dust.NewDustPerfect(Item.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.IceTorch, new Vector2(0f, Main.rand.NextFloat(-0.8f, -0.2f)), 100, default, Main.rand.NextFloat(0.8f, 1.2f));
                d.noGravity = true;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            DrawPenny(spriteBatch, Item.type, Item.Center - Main.screenPosition, lightColor, scale, rotation, 1f);
            return false;
        }

        public static void DrawPenny(SpriteBatch sb, int itemType, Vector2 screenPos, Color baseColor, float baseScale, float rotation, float alpha)
        {
            Texture2D tex = TextureAssets.Item[itemType].Value;
            Vector2 origin = tex.Size() / 2f;
            float t = Main.GlobalTimeWrappedHourly;

            Vector2 shake = new Vector2(
                (float)(Math.Sin(t * 97f) + Math.Sin(t * 61f + 1f)),
                (float)(Math.Cos(t * 83f) + Math.Sin(t * 47f + 2f))) * 0.9f;
            float shakeRot = (float)Math.Sin(t * 71f) * 0.06f;

            float beat = (float)Math.Pow(Math.Max(0.0, Math.Sin(t * 2.5f)), 6.0);
            float pulse = 1f + 0.07f * (float)Math.Sin(t * 5f) + 0.12f * beat;

            Vector2 pos = screenPos + shake;
            float rot = rotation + shakeRot;
            float s = baseScale * pulse;

            for (int k = 0; k < 3; k++)
            {
                float phase = (t * 0.9f + k / 3f) % 1f;
                float a = (1f - phase) * (1f - phase) * 0.35f * alpha;
                Color c = new Color(160, 210, 255, 0) * a;
                sb.Draw(tex, pos, null, c, rot, origin, baseScale * (1f + phase * 0.9f), SpriteEffects.None, 0f);
            }

            sb.Draw(tex, pos, null, baseColor * alpha, rot, origin, s, SpriteEffects.None, 0f);

            float glow = (0.3f + 0.2f * (float)Math.Sin(t * 5f) + 0.3f * beat) * alpha;
            sb.Draw(tex, pos, null, new Color(200, 230, 255, 0) * glow, rot, origin, s * 1.05f, SpriteEffects.None, 0f);
        }
    }

    public class IcePennyHauntSystem : ModSystem
    {
        private const float HauntRange = 900f;
        private const string HauntText = "I didn't mean to haunt you";
        private const float TextVisibleThreshold = 0.35f; 
        private const float SummonDelaySeconds = 4f;     

        public static float Haunt; 
        private static int hauntItem = -1;
        private static float hauntTextTimer; 

        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;

            Player player = Main.LocalPlayer;
            int pennyType = ModContent.ItemType<IcePenny>();
            int found = -1;
            float best = HauntRange * HauntRange;

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active || item.type != pennyType)
                    continue;

                float d = Vector2.DistanceSquared(item.Center, player.Center);
                if (d < best)
                {
                    best = d;
                    found = i;
                }
            }

            hauntItem = found;

            if (found >= 0)
            {
                Haunt = Math.Min(Haunt + 0.02f, 1f);

                if (Haunt > TextVisibleThreshold)
                {
                    hauntTextTimer += 1f / 60f; 

                    if (hauntTextTimer >= SummonDelaySeconds)
                    {
                        Item item = Main.item[found];
                        Vector2 spawnPos = item.Center;

                        item.active = false; 

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int bossType = ModContent.NPCType<Korochun>(); 
                            NPC.NewNPC(new EntitySource_Misc("IcePenny"), (int)spawnPos.X, (int)spawnPos.Y, bossType);
                        }
                        else
                        {

                        }

                        Haunt = 0f;
                        hauntItem = -1;
                        hauntTextTimer = 0f;
                    }
                }
            }
            else
            {
                Haunt = Math.Max(Haunt - 0.04f, 0f);
                hauntTextTimer = 0f; 
            }
        }

        public override void OnWorldUnload()
        {
            Haunt = 0f;
            hauntItem = -1;
            hauntTextTimer = 0f;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int invIndex = layers.FindIndex(l => l.Name == "Vanilla: Inventory");
            if (invIndex < 0)
                invIndex = 0;

            layers.Insert(invIndex, new LegacyGameInterfaceLayer("Waybound: IcePenny Darkness", () =>
            {
                DrawDarkness(Main.spriteBatch);
                return true;
            }, InterfaceScaleType.UI));

            int textIndex = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (textIndex < 0)
                textIndex = layers.Count;

            layers.Insert(textIndex, new LegacyGameInterfaceLayer("Waybound: IcePenny Text", () =>
            {
                DrawHauntText(Main.spriteBatch);
                return true;
            }, InterfaceScaleType.UI));
        }

        private static void DrawDarkness(SpriteBatch sb)
        {
            if (Main.gameMenu || Haunt <= 0.001f)
                return;

            float t = Main.GlobalTimeWrappedHourly;
            float beat = (float)Math.Pow(Math.Max(0.0, Math.Sin(t * 2.5f)), 6.0);
            float darkness = Haunt * (0.68f + 0.06f * beat);

            sb.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                Color.Black * darkness);

            if (hauntItem >= 0 && Main.item[hauntItem].active)
            {
                Item item = Main.item[hauntItem];
                DrawPenny(sb, item.type, item.Center - Main.screenPosition, Color.White, 1f, 0f, Haunt);
            }
        }

        private static void DrawPenny(SpriteBatch sb, int type, Vector2 screenPos, Color color, float scale, float rotation, float alpha)
        {
            IcePenny.DrawPenny(sb, type, screenPos, color, scale, rotation, alpha);
        }

        private static void DrawHauntText(SpriteBatch sb)
        {
            if (Main.gameMenu || Haunt <= TextVisibleThreshold)
                return;

            var font = FontAssets.DeathText.Value;
            float t = Main.GlobalTimeWrappedHourly;

            float a = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((Haunt - TextVisibleThreshold) / 0.65f, 0f, 1f));
            a *= 0.88f + 0.12f * (float)Math.Sin(t * 23f);

            float scale = MathHelper.Clamp(Main.screenWidth / 1920f, 0.6f, 1.2f) * 0.8f;
            Vector2 size = font.MeasureString(HauntText);
            Vector2 origin = size / 2f;
            Vector2 pos = new Vector2(Main.screenWidth / 2f, Main.screenHeight * 0.35f);

            float glitch = (float)Math.Pow(Math.Max(0.0, Math.Sin(t * 1.7f)), 30.0);
            pos.X += glitch * (float)Math.Sin(t * 200f) * 8f;
            pos.Y += (float)Math.Sin(t * 1.3f) * 2f;

            for (int i = 0; i < 8; i++)
            {
                Vector2 off = new Vector2(4f, 0f).RotatedBy(MathHelper.TwoPi / 8f * i);
                sb.DrawString(font, HauntText, pos + off, Color.White * (a * 0.08f), 0f, origin, scale, SpriteEffects.None, 0f);
            }

            sb.DrawString(font, HauntText, pos, Color.White * a, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}