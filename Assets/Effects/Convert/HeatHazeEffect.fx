sampler2D SceneTexture;
sampler2D NoiseTexture;

float Time;
float Intensity;
float2 ScreenSize;

struct VertexToPixel
{
    float4 Position : POSITION;
    float2 UV : TEXCOORD0;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

VertexToPixel VertexShader(float4 inPos : POSITION, float2 inUV : TEXCOORD0)
{
    VertexToPixel output;
    output.Position = inPos;
    output.UV = inUV;
    return output;
}

PixelToFrame PixelShader(VertexToPixel input)
{
    PixelToFrame output;
    
    float2 noise = tex2D(NoiseTexture, input.UV * 0.3 + Time * 0.03).rg;
    float2 distortion = (noise - 0.5) * Intensity;
    
    distortion.y += sin(input.UV.y * 80 + Time * 8) * 0.002;
    distortion.x += cos(input.UV.x * 60 + Time * 6) * 0.001;
    
    float2 distortedUV = input.UV + distortion;
    float4 color = tex2D(SceneTexture, distortedUV);
    
    float vignette = 1 - length(input.UV - 0.5) * 0.4;
    color.rgb += float3(0.08, 0.02, 0.0) * vignette;
    
    float heatGlow = sin(input.UV.y * 200 - Time * 15) * 0.1;
    color.rgb += float3(heatGlow * 0.5, heatGlow * 0.1, 0);
    
    output.Color = color;
    return output;
}

technique HeatHaze
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexShader();
        PixelShader = compile ps_2_0 PixelShader();
    }
}