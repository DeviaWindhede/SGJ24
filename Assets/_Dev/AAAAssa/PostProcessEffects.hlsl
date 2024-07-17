#ifndef CUSTOM_PP_EFFECTS_INCLUDED
#define CUSTOM_PP_EFFECTS_INCLUDED

//float Sobel(float2 coord, Texture2D texture, float2 texelSize, bool horizontal)
//{
//    float3x3 sobelMatrix = horizontal ? float3x3(
//					+1, 0, -1,
//					+2, 0, -2,
//					+1, 0, -1
//				) : float3x3(
//					 1, 2, 1,
//					 0, 0, 0,
//					-1, -2, -1
//				);

              
//    float3 sum = 0;
//    for (int y = -1; y <= 1; y++)
//    {
//        for (int x = -1; x <= 1; x++)
//        {
//            float2 offset = float2(x, y);
//            float3 sample = tex2D(texture, coord + offset * texelSize.xy).rgb;
//            sum += sample * sobelMatrix[x + 1][y + 1];
//        }
//    }

//    return sum.r;
//}

//float GetSobelNeighbours(float2 coord, Texture2D texture, float2 texelSize)
//{
//    float2 sobelX = float2(Sobel(coord, texture, texelSize, true), 0);
//    float2 sobelY = float2(0, Sobel(coord, texture, texelSize, false));
//    float sobel = length(sobelX + sobelY);
//    return sobel;
//}

//float GetSobelResultHV(float2 coord, Texture2D texture, float2 texelSize)
//{
//    float sobelH = Sobel(coord, texture, texelSize, true);
//    float sobelV = Sobel(coord, texture, texelSize, false);

//    float sobelMax = max(sobelH, sobelV);         
//    sobelMax = max(sobelMax, GetSobelNeighbours(coord, texture, texelSize));
//    return sobelMax;
//}

inline float LinearEyeDepth(float z)
{
    return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
}

float GetDepthAt(float4 UV)
{
    return LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy));
}

void SobelFromDepth_float(float4 UV, out float SobelResult)
{
    float3x3 sobelMatrixH = float3x3(
    	+1, 0, -1,
    	+2, 0, -2,
    	+1, 0, -1
    );

    float3x3 sobelMatrixV = float3x3(
    	 1,  2,  1,
    	 0,  0,  0,
    	-1, -2, -1
    );

    float4 texelSize = 1.0 / float4(_ScreenParams.x, _ScreenParams.y, 1.0f, 1.0f);

    float3 sumH = 0;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float4 offset = float4(x, y, 0, 0);
            float3 sample = GetDepthAt(UV + offset * texelSize);
            sumH += sample * sobelMatrixH[x + 1][y + 1];
        }
    }

    float3 sumV = 0;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float4 offset = float4(x, y, 0, 0);
            float3 sample = GetDepthAt(UV + offset * texelSize);
            sumV += sample * sobelMatrixV[x + 1][y + 1];
        }
    }

    float maxH = max(max(sumH.x, sumH.y), sumH.z);
    float maxV = max(max(sumV.x, sumV.y), sumV.z);

    SobelResult = max(maxH, maxV);
}

float3 GetNormalAt(float4 UV)
{
    return SHADERGRAPH_SAMPLE_SCENE_NORMAL(UV.xy);
}

void SobelFromNormal_float(float4 UV, out float SobelResult)
{
    float3x3 sobelMatrixH = float3x3(
    	+1, 0, -1,
    	+2, 0, -2,
    	+1, 0, -1
    );

    float3x3 sobelMatrixV = float3x3(
    	 1, 2, 1,
    	 0, 0, 0,
    	-1, -2, -1
    );

    float4 texelSize = 1.0 / float4(_ScreenParams.x, _ScreenParams.y, 1.0f, 1.0f);

    float3 sumH = 0;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float4 offset = float4(x, y, 0, 0);
            float3 sample = GetNormalAt(UV + offset * texelSize);
            sumH += sample * sobelMatrixH[x + 1][y + 1];
        }
    }

    float3 sumV = 0;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float4 offset = float4(x, y, 0, 0);
            float3 sample = GetNormalAt(UV + offset * texelSize);
            sumV += sample * sobelMatrixV[x + 1][y + 1];
        }
    }

    float maxH = max(max(sumH.x, sumH.y), sumH.z);
    float maxV = max(max(sumV.x, sumV.y), sumV.z);

    SobelResult = max(maxH, maxV);
}


#endif