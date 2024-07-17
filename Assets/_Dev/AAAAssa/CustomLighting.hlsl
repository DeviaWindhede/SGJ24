#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
	#include "../../../Library/PackageCache/com.unity.render-pipelines.universal@14.0.11/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

    #if (SHADERPASS != SHADERPASS_FORWARD)
        #undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
    #endif
#endif

struct CustomLightingData
{
    // Surface attributes
    float3 albedo;
    float3 normal;
    float3 position;
    float3 viewDirection;
    float4 shadowCoord;
};

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(CustomLightingData d, Light light)
{

    float3 radiance = light.color * light.shadowAttenuation;

    float diffuse = saturate(dot(d.normal, light.direction));

    

    float3 color = d.albedo * radiance * diffuse * light.distanceAttenuation;

    return color;
}
#endif

float3 CalculateCustomLighting(CustomLightingData d)
{
#ifdef SHADERGRAPH_PREVIEW
    // In preview, estimate diffuse + specular
    float3 lightDir = float3(0.5, 0.5, 0);
    float intensity = saturate(dot(d.normal, lightDir));
    return d.albedo * intensity;
#else

    Light mainLight = GetMainLight(d.shadowCoord, d.position, 1);
    float3 color = 0;
    // Shade the main light
    color += CustomLightHandling(d, mainLight);

    const uint lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; i++)
    {
        Light additionalLight = GetAdditionalLight(i, d.position);
        color += CustomLightHandling(d, additionalLight);
    }


    //float rim = saturate(dot(d.normal, -d.viewDirection));
    //rim = pow(rim, 0.001f);
    //rim *= 100.0f;
    //float3 rimColor = float3(1, 1, 1) * 0.25f;
    //color += rimColor * rim;



    return color;
#endif
}

void CalculateCustomLighting_float(float3 Albedo, float3 Normal, float3 Position, out float3 Color)
{
    CustomLightingData d;
    d.albedo = Albedo;
    d.normal = Normal;
    d.position = Position;

    float3 cameraPosition = _WorldSpaceCameraPos;
    d.viewDirection = normalize(cameraPosition - Position);

#ifdef SHADERGRAPH_PREVIEW
    // In preview, there's no shadows or bakedGI
    d.shadowCoord = 0;
#else
    // Calculate the main light shadow coord
    // There are two types depending on if cascades are enabled
    float4 positionCS = TransformWorldToHClip(Position);
#if SHADOWS_SCREEN
        d.shadowCoord = ComputeScreenPos(positionCS);
#else
    d.shadowCoord = TransformWorldToShadowCoord(Position);
    #endif
#endif

    Color = CalculateCustomLighting(d);
}

#endif