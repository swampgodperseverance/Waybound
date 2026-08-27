sampler2D uImage : register(s0);
float uTime;
float uIntensity;
float uScale = 3.0;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 noiseOffset = float2(
        sin(coords.y * 50 + uTime * 5) * 0.01,
        cos(coords.x * 50 + uTime * 5) * 0.01
    );
    noiseOffset *= uIntensity;
    
    float2 distortedCoords = coords + noiseOffset;
    float4 color = tex2D(uImage, distortedCoords);
    
    return color;
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}