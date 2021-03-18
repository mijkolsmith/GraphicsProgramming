#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// External Properties
float4x4 World;
float4x4 View;
float4x4 Projection;

float3 lightDirection, viewDirection, ambient, cameraPosition;
float time;

Texture2D WaterTex;
sampler2D WaterTextureSampler = sampler_state
{
    Texture = <WaterTex>;
    MipFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D DirtTex;
sampler2D DirtTextureSampler = sampler_state
{
    Texture = <DirtTex>;
    MipFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D GrassTex;
sampler2D GrassTextureSampler = sampler_state
{
    Texture = <GrassTex>;
    MipFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D RockTex;
sampler2D RockTextureSampler = sampler_state
{
    Texture = <RockTex>;
    MipFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D SnowTex;
sampler2D SnowTextureSampler = sampler_state
{
    Texture = <SnowTex>;
    MipFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

TextureCube SkyTex;
samplerCUBE SkyTextureSampler = sampler_state
{
    Texture = <SkyTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};


// Getting out vertex data from vertex shader to pixel shader
struct VertexShaderOutput {
    float4 position : SV_POSITION;
    float4 color : COLOR0;
    float2 tex : TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;
    float3 texCube : TEXCOORD3;
    float3 binormal : TEXCOORD4;
    float3 tangent : TEXCOORD5;
    float3 objectPos : TEXCOORD6;
};

// Vertex shader, receives values directly from semantic channels
VertexShaderOutput MainVS( float4 position : POSITION, float4 color : COLOR0, float3 normal : NORMAL, float3 binormal : BINORMAL, float3 tangent : TANGENT, float2 tex : TEXCOORD0 )
{
    VertexShaderOutput o = (VertexShaderOutput)0;

    o.position = mul( mul( mul(position, World), View ), Projection );
    o.color = color;
    o.normal = mul(normal, (float3x3)World);
    o.tex = tex;
    o.worldPos = mul(position, World);
    o.objectPos = position;

    return o;
}

// Pixel Shader, receives input from vertex shader, and outputs to COLOR semantic
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float d = distance(input.worldPos, cameraPosition);

    //Water texture
    float3 waterFar = tex2D(WaterTextureSampler, input.tex / 10).rgb;
    float3 waterMedium = tex2D(WaterTextureSampler, input.tex * 2).rgb;
    float3 waterClose = tex2D(WaterTextureSampler, input.tex * 50).rgb;

    float3 closeLerpWater = lerp(waterClose, waterMedium, clamp(d / 500, 0, 1));
    float3 water = lerp(closeLerpWater, waterFar, clamp((d - 1000) / 500, 0, 1));

    //Dirt texture
    float3 dirtFar = tex2D(DirtTextureSampler, input.tex / 10).rgb;
    float3 dirtMedium = tex2D(DirtTextureSampler, input.tex * 5).rgb;
    float3 dirtClose = tex2D(DirtTextureSampler, input.tex * 40).rgb;

    float3 closeLerpDirt = lerp(dirtClose, dirtMedium, clamp(d / 500, 0, 1));
    float3 dirt = lerp(closeLerpDirt, dirtFar, clamp((d - 1000) / 500, 0, 1));

    //Grass texture
    float3 grassFar = tex2D(GrassTextureSampler, input.tex / 5).rgb;
    float3 grassMedium = tex2D(GrassTextureSampler, input.tex * 10).rgb;
    float3 grassClose = tex2D(GrassTextureSampler, input.tex * 100).rgb;

    float3 closeLerpGrass = lerp(grassClose, grassMedium, clamp(d / 1000, 0, 1));
    float3 grass = lerp(closeLerpGrass, grassFar, clamp((d - 1000) / 500, 0, 1));

    //Rock texture
    float3 rockFar = tex2D(RockTextureSampler, input.tex / 10).rgb;
    float3 rockMedium = tex2D(RockTextureSampler, input.tex * 10).rgb;
    float3 rockClose = tex2D(RockTextureSampler, input.tex * 30).rgb;

    float3 closeLerpRock = lerp(rockClose, rockMedium, clamp(d / 500, 0, 1));
    float3 rock = lerp(closeLerpRock, rockFar, clamp((d - 1000) / 500, 0, 1));

    //Snow texture
    float3 snowFar = tex2D(SnowTextureSampler, input.tex / 10).rgb;
    float3 snowMedium = tex2D(SnowTextureSampler, input.tex * 2).rgb;
    float3 snowClose = tex2D(SnowTextureSampler, input.tex * 50).rgb;

    float3 closeLerpSnow = lerp(snowClose, snowMedium, clamp(d / 500, 0, 1));
    float3 snow = lerp(closeLerpSnow, snowFar, clamp((d - 1000) / 500, 0, 1));

    //added cos(time) to simulate seasons
    float wd = clamp((input.worldPos.y - 25 + cos(time/5) * 10)/ 10, -1, 1) * .5 + .5;
    float dg = clamp((input.worldPos.y - 60 - cos(time/5) * 10) / 10, -1, 1) * .5 + .5;
    float gr = clamp((input.worldPos.y - 100 + cos(time/5) * 10) / 10, -1, 1) * .5 + .5;
    float rs = clamp((input.worldPos.y - 290 + cos(time/5) * 40) / 10, -1, 1) * .5 + .5;

    float3 texColor = lerp(lerp(lerp(lerp(water, dirt, wd), grass, dg), rock, gr), snow, rs);

    //Lighting calculation
    float3 lighting = max( dot(input.normal, lightDirection), 0.0) + ambient;

    //fog
    float fogAmount = clamp((d - 250) / 1500, 0, 1);
    float3 fogColor = float3(188, 214, 231) / 255.0;

    //Output
    return float4(lerp(texColor * lighting, fogColor, fogAmount), 1);
}

VertexShaderOutput SkyVS(float4 position : POSITION, float3 normal : NORMAL, float2 tex : TEXCOORD0)
{
    VertexShaderOutput o = (VertexShaderOutput)0;

    o.position = mul(mul(mul(position, World), View), Projection);
    o.normal = mul(normal, (float3x3)World);
    o.worldPos = mul(position, World).xyz;

    float4 vertexPosition = mul(position, World);
    o.texCube = normalize(vertexPosition - cameraPosition);

    return o;
}

float4 SkyPS(VertexShaderOutput input) : COLOR
{
    float3 topColor = float3(68, 118, 189) / 255.0;
    float3 botColor = float3(188, 214, 231) / 255.0;

    float3 viewDirection = normalize(input.worldPos - cameraPosition);

    float sun = pow(max(dot(-viewDirection, lightDirection), 0.0f), 512);
    float3 sunColor = float3(255, 200, 50) / 255.0;

    return float4(lerp(botColor, topColor, viewDirection.y) + sun * sunColor, 1);
}

float4 UnlitTransparentPS(VertexShaderOutput input) : COLOR
{
    float4 texColor = tex2D(GrassTextureSampler, input.tex);

    //Alpha Test, if alpha < .5 discard
    //clip(texColor.a - (sin(time) * 0.5 + 0.5));
    //if (texColor.a - . 5 < 0) discard;

    return texColor;
}

float4 HeatDistortionPS(VertexShaderOutput input, float2 uv : VPOS) : COLOR
{
    uv = (uv + .5) * float2(1.0 / 1080.0, 1.0 / 1080.0);
#if OPENGL
    uv.y = 1 - uv.y;
#endif

    //distort screen uv
    float2 normal = tex2D(WaterTextureSampler, input.tex + float2(0, time));
    normal = (normal - .5) * 2;

    uv += (normal * 0.01) * (-input.objectPos.z * .5 + .5);

    float4 screenColor = tex2D(GrassTextureSampler, uv);

    return 1 - screenColor;
}

technique Terrain
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

technique SkyBox
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL SkyVS();
        PixelShader = compile PS_SHADERMODEL SkyPS();
    }
};

technique UnlitTransparent
{
    pass
    {
        //AlphaBlendEnable = true;
        //SrcBlend = SrcAlpha;
        //DestBlend = InvSrcAlpha;

        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL UnlitTransparentPS();
    }
};

technique HeatDistortion
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL HeatDistortionPS();
    }
};