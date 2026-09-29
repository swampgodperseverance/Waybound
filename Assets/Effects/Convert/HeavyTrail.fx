matrix WorldViewProjection;

float Time;

texture SampleTexture;
sampler2D samplerMain = sampler_state 
{
    texture = <SampleTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

struct VertexShaderInput 
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput 
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoords : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) 
{
    VertexShaderOutput output;
    
    output.Position = mul(input.Position, WorldViewProjection);
    
    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 
{
    float4 color = tex2D(samplerMain, input.TexCoords);
    
    return color * input.Color;
}

technique Technique1 
{
    pass TrailPass 
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}