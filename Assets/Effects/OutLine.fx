sampler2D texture2d;

float uTime;
float alpha;

float4 outLine(float2 texCoord : TEXCOORD0) : COLOR0 {
    float4 color = tex2D(texture2d, texCoord);

    if (color.a <= 0.0) { return color; };
    if (color.r < 0.01 && color.g < 0.01 && color.b < 0.01) { return color; };
    
    float pulse = sin(uTime * 3.0) * 0.5 + 0.5;
    float3 darkGold = float3(0.45, 0.25, 0.03);
    float3 lightGold = float3(1.0, 0.75, 0.2);
    float3 gold = lerp(darkGold, lightGold, pulse);

    return float4(gold * alpha, alpha);
}

technique Technique1 {
    pass P0 { PixelShader = compile ps_2_0 outLine(); }
}