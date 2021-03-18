#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float time;

Texture2D MainTex;
sampler2D MainTexSampler = sampler_state
{
    Texture = <MainTex>;
};

float4 InvertPS(float2 uv : VPOS) : COLOR
{
    uv = (uv + .5) * float2(1.0 / 1080.0, 1.0 / 1080.0);
    float4 color = tex2D(MainTexSampler, uv);

    return 1 - color;
}

technique Invert
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL InvertPS();
    }
};

float4 ChromaticAbberationPS(float2 uv : VPOS) : COLOR
{
    uv = (uv + .5) * float2(1.0 / 1080.0, 1.0 / 1080.0);

    float strength = 30 * (cos(time) + .1);
    float3 rgbOffset = 1 + float3(.01, .005, 0) * strength;
    float dist = distance(uv, float2(.5, .5));
    float2 dir = uv - float2(.5, .5);

    //normalize & scale offset
    rgbOffset = normalize(rgbOffset * dist);

    //calculate uv's for each color channel
    float2 uvR = float2(.5, .5) + rgbOffset.r * dir;
    float2 uvG = float2(.5, .5) + rgbOffset.g * dir;
    float2 uvB = float2(.5, .5) + rgbOffset.b * dir;

    float4 colorR = tex2D(MainTexSampler, uvR);
    float4 colorG = tex2D(MainTexSampler, uvG);
    float4 colorB = tex2D(MainTexSampler, uvB);

    return float4(colorR.r, colorG.g, colorB.b, 1);
}

technique ChromaticAbberation
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL ChromaticAbberationPS();
    }
};

float4 VignettePS(float2 uv : VPOS) : COLOR
{
    uv = (uv + .5) * float2(1.0 / 1080.0, 1.0 / 1080.0);
    float4 color = tex2D(MainTexSampler, uv);

    float offset = .5;
    float power = 1;
    float scale = 2.4;

    float vig = 1 - pow(clamp(distance(uv, float2(.5, .5)) * scale - offset, .6, 1), power);

    return color * vig;
}

technique Vignette
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL VignettePS();
    }
};

float4 BlurPS(float2 uv : VPOS) : COLOR
{
    uv = (uv + .5) * float2(1.0 / 1080.0, 1.0 / 1080.0);

    float pi = 6.28318530718; // Pi*2

    // GAUSSIAN BLUR SETTINGS {{{
    float directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
    float quality = 2.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
    float size = 4.0 * sin(time); // BLUR SIZE (Radius)
    // GAUSSIAN BLUR SETTINGS }}}

    float2 radius = size / float2(1080.0, 1080.0);

    // Pixel colour
    float4 color = tex2D(MainTexSampler, uv);

    // Blur calculations
    for (float d = 0.0; d < pi; d += pi / directions)
    {
        for (float i = 1.0 / quality; i <= 1.0; i += 1.0 / quality)
        {
            color += tex2D(MainTexSampler, uv + float2(cos(d), sin(d)) * radius * i);
        }
    }

    // Output to screen
    color /= quality * directions - 15.0;
    return color;
}

technique Blur
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL BlurPS();
    }
};