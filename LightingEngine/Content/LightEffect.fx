float4x4 World;

float screenWidth;
float screenHeight;
float4 ambientColor;

float lightStrength;
float lightDecay;
float3 lightPosition;
float4 lightColor;
float lightRadius;
float specularStrength;

float3 coneDirection;
float coneAngle;
float coneDecay;

bool invertY;


Texture2D ColorMap;
sampler ColorMapSampler : register (s0) {
	Texture = (ColorMap);
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

Texture2D NormalMap;
sampler NormalMapSampler : register (s1) {
	Texture = (NormalMap);
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};


struct VertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
};

struct PixelToFrame
{
	float4 Color : COLOR0;
};

VertexToPixel DeferredLightVertexShader(float4 inPos : POSITION0, float2 texCoord : TEXCOORD0, float4 color : COLOR0)
{
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;
	Output.TexCoord = texCoord;
	Output.Color = color;
	
	return Output;
}

PixelToFrame PointLightShader(VertexToPixel PSIn) 
{	
	PixelToFrame Output = (PixelToFrame)0;
	
	float4 colorMap = tex2D(ColorMapSampler, PSIn.TexCoord);
	float3 normal = (2.0f * (tex2D(NormalMapSampler, PSIn.TexCoord).rgb)) - 1.0f;
	
    if(invertY)
    {
        normal.y = -normal.y;
    }

	float3 pixelPosition;
	pixelPosition.x = screenWidth * PSIn.TexCoord.x;
	pixelPosition.y = screenHeight * PSIn.TexCoord.y;
	pixelPosition.z = 0;

	float3 lightDirection = lightPosition - pixelPosition;
	float3 lightDirNorm = normalize(lightDirection);
	float3 halfVec = float3(0, 0, 1);
		
	float amount = max(dot(normal, lightDirNorm), 0);
	float coneAttenuation = saturate(1.0f - length(lightDirection) / lightDecay); 
					
	float3 reflect = normalize(2 * amount * normal - lightDirNorm);
	float specular = min(pow(saturate(dot(reflect, halfVec)), 10), amount);
				
	Output.Color = (colorMap * lightColor * lightStrength + (specular * specularStrength) + amount*0.5f)*coneAttenuation;

	return Output;
}


PixelToFrame SpotLightShader(VertexToPixel PSIn) 
{	
	PixelToFrame Output = (PixelToFrame)0;
	
	float4 colorMap = tex2D(ColorMapSampler, PSIn.TexCoord);
	float3 normal = (2.0f * (tex2D(NormalMapSampler, PSIn.TexCoord).rgb)) - 1.0f;

    if (invertY)
    {
        normal.y = -normal.y;
    }
			
	float3 pixelPosition;
	pixelPosition.x = screenWidth * PSIn.TexCoord.x;
	pixelPosition.y = screenHeight * PSIn.TexCoord.y;
	pixelPosition.z = 0;

	float3 lightVector = normalize(lightPosition - pixelPosition);
    float SdL = dot(coneDirection, -lightVector);
	
	float3 shading = float3(0, 0, 0);
	if (SdL > 0) 
	{
		float3 lightPos = float3(lightPosition.x, lightPosition.y, lightPosition.z);
		float3 lightVector = lightPos - pixelPosition;
		lightVector = normalize(lightVector);
		
		float3 coneDirectionTemp = coneDirection;

		float spotIntensity = pow(abs(SdL), coneDecay);

		float3 lightDirection = lightPos - pixelPosition;
		float3 halfVec = float3(0, 0, 1);
		
		float amount = max(dot(normal, lightVector), 0);
		float coneAttenuation = saturate(1.0f - length(lightDirection) / (lightDecay*2)); 
		
		float3 reflect = normalize(2 * amount * normal - lightVector);

		float3 r = normalize(2 * dot(lightVector, normal) * normal - lightVector);
		float3 v = normalize(mul(normalize(coneDirectionTemp), (float3x3)World));
		float dotProduct = dot(r, v);

		float specular = min(pow(saturate(dot(reflect, halfVec)), 10), amount);
		
		shading = lightColor.rgb * lightStrength;
		shading += specular * specularStrength;
		shading += amount;
		shading *= spotIntensity;

		shading *= coneAttenuation;
	}
	Output.Color = float4(shading.r, shading.g, shading.b, 1.0f);
	return Output;
}

technique DeferredPointLight
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DeferredLightVertexShader();
        PixelShader = compile ps_4_0 PointLightShader();
    }
}

technique DeferredSpotLight
{
    pass Pass1
    {
		VertexShader = compile vs_4_0 DeferredLightVertexShader();
        PixelShader = compile ps_4_0 SpotLightShader();
    }
}

