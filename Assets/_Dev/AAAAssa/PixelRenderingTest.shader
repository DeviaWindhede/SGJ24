Shader "Assa/PixelRenderingTest"
{
    HLSLINCLUDE
    
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // The Blit.hlsl file provides the vertex shader (Vert),
        // the input structure (Attributes), and the output structure (Varyings)
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    
        Texture2D _CameraNormalsTexture;
        Texture2D _CameraDepthTexture  ;

        void GetSobelMatrices(out float3x3 horizontal, out float3x3 vertical)
        {
            horizontal = float3x3(
    	        +1, 0, -1,
    	        +2, 0, -2,
    	        +1, 0, -1
            );

            vertical = float3x3(
    	         1,  2,  1,
    	         0,  0,  0,
    	        -1, -2, -1
            );
        }

        inline float CustomLinearEyeDepth(float z)
        {
            return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
        }

        float GetDepthAt(float2 UV)
        {
            return /* CustomLinearEyeDepth */(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_LinearClamp, UV.xy).r);
        }

        void SobelFromDepth_float(float2 UV, out float SobelResult)
        {
            float3x3 sobelMatrixH;
            float3x3 sobelMatrixV;
            GetSobelMatrices(sobelMatrixH, sobelMatrixV);


            float2 texelSize = 1.0f / float2(_ScreenParams.x, _ScreenParams.y);

            float3 sumH = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    float2 offset = float2(x, y);
                    float3 sample = GetDepthAt(UV + offset * texelSize);
                    sumH += sample * sobelMatrixH[x + 1][y + 1];
                }
            }

            float3 sumV = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    float2 offset = float2(x, y);
                    float3 sample = GetDepthAt(UV + offset * texelSize);
                    sumV += sample * sobelMatrixV[x + 1][y + 1];
                }
            }

            float maxH = max(max(sumH.x, sumH.y), sumH.z);
            float maxV = max(max(sumV.x, sumV.y), sumV.z);

            SobelResult = max(length(sumH), length(sumV)) * 100.0f;
        }

        float3 GetNormalAt(float2 UV)
        {
            return SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_LinearClamp, UV).rgb;
        }

        void SobelFromNormal_float(float2 UV, out float SobelResult)
        {
            float3x3 sobelMatrixH;
            float3x3 sobelMatrixV;
            GetSobelMatrices(sobelMatrixH, sobelMatrixV);

            float2 texelSize = 1.0f / float2(_ScreenParams.x, _ScreenParams.y);

            float3 sumH = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    float2 offset = float2(x, y);
                    float3 sample = GetNormalAt(UV + offset * texelSize);
                    sumH += sample * sobelMatrixH[x + 1][y + 1];
                }
            }
                       
            float3 sumV = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    float2 offset = float2(x, y);
                    float3 sample = GetNormalAt(UV + offset * texelSize);
                    sumV += sample * sobelMatrixV[x + 1][y + 1];
                }
            }
            
            float maxH = max(max(sumH.x, sumH.y), sumH.z);
            float maxV = max(max(sumV.x, sumV.y), sumV.z);

            SobelResult = max(length(sumH), length(sumV));
        }

        float4 RedTint (Varyings input) : SV_Target
        {
            float3 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord).rgb;

            //float3 normal = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_LinearClamp, input.texcoord).rgb;
            //float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_LinearClamp, input.texcoord).r;
            
            //float depthValue = LinearEyeDepth(depth);


            float SobelResultNormal;
            SobelFromNormal_float(input.texcoord, SobelResultNormal);

            float SobelResultDepth;
            SobelFromDepth_float(input.texcoord, SobelResultDepth);
            

            float4 outCol = float4(color, 1.0f);
            if (SobelResultDepth > 0.2f)
            {
                outCol.rgb *= 0.75f;
            }
            else if (SobelResultNormal > 2.0f)
            {
                outCol.rgb *= 1.2f;
            }


            return outCol;

        }

        float4 SimpleBlit (Varyings input) : SV_Target
        {
            float3 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord).rgb;
            return float4(color.rgb, 1);
        }
    
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off
        Pass
        {
            Name "RedTint"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment RedTint
            
            ENDHLSL
        }
        
        Pass
        {
            Name "SimpleBlit"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment SimpleBlit
            
            ENDHLSL
        }
    }
}

        