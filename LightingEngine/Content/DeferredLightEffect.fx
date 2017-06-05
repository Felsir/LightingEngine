float ambient;
float4 ambientColor;
float lightAmbient;

sampler ColorMapSampler: register (s0); // This is the texture that SpriteBatch will try to set before drawing

Texture2D ShadingMap;
sampler ShadingMapSampler: register (s1)
{
	Texture = (ShadingMap);
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

Texture2D NormalMap;
sampler NormalMapSampler: register (s2)
{
	Texture = (NormalMap);
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

float4 DeferredLightPixelShader(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{	
	float4 col= tex2D(ColorMapSampler, texCoord.xy);
	float4 shading = ShadingMap.Sample(ShadingMapSampler, texCoord.xy);
	float3 normal = NormalMap.Sample(NormalMapSampler, texCoord.xy).rgb;

	if (any(normal))
	{
		return (col * ambientColor *ambient)+((shading * col) * lightAmbient);
	}
	else
	{
		return float4(0, 0, 0, 0);
	}
}

technique DeferredLightEffect
{
    pass Pass1
    {
        PixelShader = compile ps_4_0 DeferredLightPixelShader();
    }
}