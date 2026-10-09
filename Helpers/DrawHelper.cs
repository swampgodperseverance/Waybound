using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;

namespace Waybound.Helpers;

public static class DrawHelper
{
    public static void DrawBar(Vector2 position, int value, int maxValue, Color color, float alpha,
        float scale = 1f, bool noFlip = false)
    {
        if (value <= 0)
            return;

        float progress = MathHelper.Clamp((float)value / maxValue, 0, 1);

        int fillWidth = (int)(36f * progress);
        if (fillWidth < 3)
            fillWidth = 3;
        
        float drawX = position.X - 18f * scale;
        float drawY = position.Y;
        
        if ((int)Main.player[Main.myPlayer].gravDir == -1 && !noFlip) {
            drawY -= Main.screenPosition.Y;
            drawY = Main.screenPosition.Y + Main.screenHeight - drawY;
        }


        SpriteBatch spriteBatch = Main.spriteBatch;
        if (fillWidth < 34) {
            spriteBatch.Draw(TextureAssets.Hb2.Value, new Vector2(drawX - Main.screenPosition.X + fillWidth * scale, drawY - Main.screenPosition.Y), new Rectangle(2, 0, 2, TextureAssets.Hb2.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Hb2.Value, new Vector2(drawX - Main.screenPosition.X + (fillWidth + 2) * scale, drawY - Main.screenPosition.Y), new Rectangle(fillWidth + 2, 0, 36 - fillWidth - 2, TextureAssets.Hb2.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Hb1.Value, new Vector2(drawX - Main.screenPosition.X, drawY - Main.screenPosition.Y), new Rectangle(0, 0, fillWidth - 2, TextureAssets.Hb1.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Hb1.Value, new Vector2(drawX - Main.screenPosition.X + (fillWidth - 2) * scale, drawY - Main.screenPosition.Y), new Rectangle(32, 0, 2, TextureAssets.Hb1.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
        }
        else {
            if (fillWidth < 36)
                spriteBatch.Draw(TextureAssets.Hb2.Value, new Vector2(drawX - Main.screenPosition.X + fillWidth * scale, drawY - Main.screenPosition.Y), new Rectangle(fillWidth, 0, 36 - fillWidth, TextureAssets.Hb2.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);

            spriteBatch.Draw(TextureAssets.Hb1.Value, new Vector2(drawX - Main.screenPosition.X, drawY - Main.screenPosition.Y), new Rectangle(0, 0, fillWidth, TextureAssets.Hb1.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
        }
    }

    public class RibbonTrail(
        int lifeTime,
        Texture2D texture = null,
        BasicEffect effect = null)
    {
        private int Tick;
        
        private GraphicsDevice GraphicsDevice => Main.instance.GraphicsDevice;
        
        private DynamicVertexBuffer DynamicBuffer;
        private VertexPositionColorTexture[] VertexCache;

        public void Initialize()
        {
            DynamicBuffer = new DynamicVertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), 1024, BufferUsage.WriteOnly);
            VertexCache = new VertexPositionColorTexture[1024];
        }
        
        private struct RibbonPoint(
            Vector2 position,
            int lifeTime)
        {
            public Vector2 Position = position;
            public int Timestamp = lifeTime;
        }

        private List<RibbonPoint> Points = new();

        public void Add(Vector2 newPoint) =>
            Points.Add(new RibbonPoint(newPoint, Tick + lifeTime));

        public void Clear()
        {
            Points.Clear();
        }
        
        public void Update(int ticksPassed = 1)
        {
            Tick += ticksPassed;
            while(Points.Count > 0 && Points[0].Timestamp < Tick)
                Points.RemoveAt(0);
        }

        private class RibbonPass(
            Func<float, (float, Color, float, Color, Vector2?)> progressFunc
            )
        {
            private (float, Color, float, Color, Vector2?) _result;

            public ref float LeftIndent => ref _result.Item1;
            public ref Color LeftColor => ref _result.Item2;
            public ref float RightIndent => ref _result.Item3;
            public ref Color RightColor => ref _result.Item4;
            public ref Vector2? TexturePos => ref _result.Item5;

            public void Calculate(float progress) => _result = progressFunc(progress);

            public static RibbonPass Default()
            {
                return new RibbonPass(_ => (
                    -8f,
                    Color.White,
                    8f,
                    Color.White,
                    null
                ));
            }
        }
        
        private List<RibbonPass> Passes = []; 
        public void SetPasses(params Func<float, (float, Color, float, Color, Vector2?)>[] passes)
        {
            Passes.Clear();
            foreach (var p in passes)
                Passes.Add(new RibbonPass(p));
        }
        
        public void Draw(Color? lightColor = null, bool mirrored = false, bool sideways = true, Vector2? offset = null)
        {
            if (Main.dedServ || Points.Count < 3)
                return;
            
            if (Effect == null || Effect.IsDisposed)
            {
                Effect = new BasicEffect(GraphicsDevice)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = true,
                    LightingEnabled = false,
                    World = Matrix.Identity
                };
            }
            
            Effect.Texture = Texture ?? TextureAssets.MagicPixel.Value;
            Effect.View = Main.GameViewMatrix.TransformationMatrix;
            Effect.Projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);
            
            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                pass.Apply();
            VertexBufferBinding[] buffer = GraphicsDevice.GetVertexBuffers();
            
            if(Passes.Count == 0)
                Passes.Add(RibbonPass.Default());
            foreach (RibbonPass pass in Passes)
                DrawPass(pass, lightColor ?? Color.White, mirrored, sideways, offset ?? Vector2.Zero);
            
            GraphicsDevice.SetVertexBuffers(buffer);
            foreach (EffectPass pass in Main.pixelShader.CurrentTechnique.Passes)
                pass.Apply();
        }
        
        private void DrawPass(RibbonPass pass, Color lightColor, bool mirrored, bool sideways, Vector2 offset)
        {
            int vertexCount = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                RibbonPoint point = Points[i];

                Vector2 forward = Vector2.Zero;
                if (i < Points.Count - 1)
                    forward += Points[i].Position - Points[i + 1].Position;
                if (i > 0)
                    forward += Points[i - 1].Position - Points[i].Position;
                if(forward == Vector2.Zero)
                    forward = Vector2.UnitX;
                forward.Normalize();

                float progress = 1f - (point.Timestamp - Tick) / (float)LifeTime;
                pass.Calculate(progress);
                
                Vector2 normal = sideways ? new Vector2(-forward.Y, forward.X) : forward;

                float leftIndent = !mirrored ? pass.LeftIndent : -pass.RightIndent;
                Color leftColor = !mirrored ? pass.LeftColor : pass.RightColor;
                float rightIndent = !mirrored ? pass.RightIndent : -pass.LeftIndent;
                Color rightColor = !mirrored ? pass.RightColor : pass.LeftColor;
                
                Vector2 leftPoint = point.Position + offset + normal * leftIndent - Main.screenPosition;
                Vector2 rightPoint = point.Position + offset + normal * rightIndent - Main.screenPosition;

                VertexCache[vertexCount++] = new VertexPositionColorTexture(new Vector3(leftPoint, 0f), leftColor.MultiplyRGBA(lightColor), pass.TexturePos ?? Vector2.Zero);
                VertexCache[vertexCount++] = new VertexPositionColorTexture(new Vector3(rightPoint, 0f), rightColor.MultiplyRGBA(lightColor), pass.TexturePos ?? Vector2.Zero);
            }
            
            DynamicBuffer.SetData(VertexCache, 0, vertexCount, SetDataOptions.Discard);
            GraphicsDevice.SetVertexBuffer(DynamicBuffer);
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, vertexCount - 2);
        }
    }
}