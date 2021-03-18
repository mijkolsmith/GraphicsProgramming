#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// External Properties
float4x4 World, View, Projection;

float3 cameraPosition, lightPosition;
float Time;

Texture2D DayTex;
sampler2D DayTextureSampler = sampler_state
{
    Texture = <DayTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D NightTex;
sampler2D NightTextureSampler = sampler_state
{
    Texture = <NightTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D CloudsTex;
sampler2D CloudsTextureSampler = sampler_state
{
    Texture = <CloudsTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    AddressU = WRAP; //CLAMP, MIRROR
};

Texture2D MoonTex;
sampler2D MoonTextureSampler = sampler_state
{
    Texture = <MoonTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D SunTex;
sampler2D SunTextureSampler = sampler_state
{
    Texture = <SunTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

TextureCube SkyTex;
samplerCUBE SkyTextureSampler = sampler_state
{
    Texture = <SkyTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    AddressU = MIRROR;
    AddressV = MIRROR;
};

// Getting out vertex data from vertex shader to pixel shader
struct VertexShaderOutput {
    float4 position     : SV_POSITION;
    float4 color        : COLOR0;
    float2 uv           : TEXCOORD0;
    float3 worldPos     : TEXCOORD1;
    float3 worldNormal  : TEXCOORD2;
};

// Vertex shader, receives values directly from semantic channels
VertexShaderOutput MainVS(float4 position : POSITION, float4 color : COLOR0, float2 uv : TEXCOORD, float3 normal : NORMAL , float4 positionLight : TEXCOORD4) 
{ 
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.worldPos = normalize(mul(position, World)).xyz;
    output.position = normalize(mul(mul(mul(position, World), View), Projection));
    output.color = color;
    output.uv = uv;
    output.worldNormal = normalize(mul(normal, World)).xyz;

    return output;
}

// Pixel Shader, receives input from vertex shader, and outputs to COLOR semantic
float4 EarthPS(VertexShaderOutput input) : COLOR
{
    //textures
    float4 dayColor = tex2D(DayTextureSampler, input.uv);
    float4 nightColor = tex2D(NightTextureSampler, input.uv);
    float4 cloudsColor = tex2D(CloudsTextureSampler, input.uv + half2(Time * 0.01f, 0));
    float4 moonColor = tex2D(MoonTextureSampler, input.uv);

    //calculate vectors for lighting & specular
    float3 viewDirection = normalize(input.worldPos - cameraPosition);
    float3 lightDirection = normalize(input.worldPos - lightPosition);

    //calculate specular
    float3 refl = normalize(reflect(-lightDirection, input.worldNormal));
    float spec = pow(max(dot(refl, normalize(viewDirection)), 0.0), 4);

    //calculate lighting
    float light = max(dot(input.worldNormal, -lightDirection), 0.0);

    float3 skyColor = float3(.529f, 0.808f, 0.992f);
    float3 fresnel = pow(dot(input.worldNormal, viewDirection) * .5 + .5, 3) * 8 * light * skyColor;
    
    float3 diffuseColor = lerp(nightColor.rgb, dayColor.rgb, light) + cloudsColor.rgb * light;

    float3 reflectedViewDir = reflect(viewDirection, input.worldNormal);
    float3 skyReflection = texCUBE(SkyTextureSampler, reflectedViewDir).rgb;

    return float4((max(light, 0.2f) + spec * 2) * diffuseColor.rgb + fresnel, 1);
}

float4 MoonPS(VertexShaderOutput input) : COLOR
{
    //textures
    float4 moonColor = tex2D(MoonTextureSampler, input.uv);

    //calculate vectors for lighting
    float3 lightDirection = normalize(input.worldPos - lightPosition);

    //calculate lighting
    float light = min(max(dot(input.worldNormal, -lightDirection), 0.0) * 32, 1.0);

    return float4((max(light, 0.1f)) * moonColor.rgb, 1);
}

float4 SunPS(VertexShaderOutput input) : COLOR
{
    //textures
    float4 sunColor = tex2D(SunTextureSampler, input.uv);

    return float4(sunColor.rgb * 1.1f, 1);
}

float4 SkyPS(VertexShaderOutput input) : COLOR
{
    float3 viewDirection = normalize(input.worldPos - cameraPosition);

    float3 skyColor = texCUBE(SkyTextureSampler, viewDirection).rgb;

    return float4(pow(skyColor.rgb, 2), 1);
}

technique Earth
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL EarthPS();
    }
};

technique Moon
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MoonPS();
    }
};

technique Sun
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL SunPS();
    }
};

technique Sky
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL SkyPS();
    }
};