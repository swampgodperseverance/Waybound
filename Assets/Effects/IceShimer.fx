sampler2D texture2d : register(s0);

float uTime;

float4 iceShimer(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(texture2d, texCoord);
    
    if (color.a <= 0.0)
        return color;
    if (color.r < 0.01 && color.g < 0.01 && color.b < 0.01)
        return color;
    
    const float TAU = 6.28318530718f;
    const int MAX_ITER = 5; // Снова 5 итераций, эффект будет очень красивым и детальным
    
    float time = uTime * 0.5 + 23.0;
    
    // Эффект красиво распределяется по координатам
    float2 p = fmod(texCoord * TAU, TAU) - 250.0;
    float2 i = p;
    float c = 1.0;
    float inten = 0.005;

    for (int n = 0; n < MAX_ITER; n++)
    {
        float t = time * (1.0 - (3.5 / float(n + 1)));
        
        // Разделяем вычисления, чтобы компилятор не ругался
        float nextX = cos(t - i.x) + sin(t + i.y);
        float nextY = sin(t - i.y) + cos(t + i.x);
        i = p + float2(nextX, nextY);
        
        c += 1.0 / length(float2(p.x / (sin(i.x + t) / inten), p.y / (cos(i.y + t) / inten)));
    }
    
    c /= float(5);
    c = 1.17 - pow(abs(c), 1.4);
    
    float3 colour = float3(pow(abs(c), 8.0), pow(abs(c), 8.0), pow(abs(c), 8.0));
    colour = clamp(colour + float3(0.0, 0.35, 0.5), 0.0, 1.0);
    
    return float4(colour * color.rgb, color.a);
}

technique Technique1
{
    pass P0
    {
        // Используем ps_3_0, так как Terraria и tModLoader работают в HiDef-режиме
        PixelShader = compile ps_3_0 iceShimer();
    }
}
