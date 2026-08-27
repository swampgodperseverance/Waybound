sampler2D ScreenTexture : register(s0);

float uTime;

float4 MainPS(float2 uv : TEXCOORD0) : COLOR0

    float2 gridUV = uv * 3.0;
    float2 cell = floor(gridUV);
    float2 localUV = frac(gridUV);

    float2 shake;
    shake.x = sin(uTime * 2.5 + cell.x * 1.7 + cell.y * 2.1) * 0.01;
    shake.y = cos(uTime * 2.1 + cell.x * 2.4 + cell.y * 1.3) * 0.01;

    localUV += shake;

    float4 color = tex2D(ScreenTexture, localUV);

    float line = 0.0;

    float gx = frac(uv.x * 3.0);
    line += step(gx, 0.02) + step(0.98, gx);

    float gy = frac(uv.y * 3.0);
    line += step(gy, 0.02) + step(0.98, gy);


    float3 borderColor = float3(0, 0, 0);

    color.rgb = lerp(color.rgb, borderColor, saturate(line));

    return color;
}

technique OriginOfSymmetry
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}