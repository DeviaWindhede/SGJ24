    Shader "Assa/Downscale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _QuantizationValue("Quantization Value", Int) = 8
        _DownscaleSteps("Downscale Steps", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            int _QuantizationValue;
            int _DownscaleSteps;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 QuantizeColor(float4 color, int paletteSize)
            {
                float4 quantizedColor = floor(color * (paletteSize-1)+0.5f) / (paletteSize-1);
				return quantizedColor;
            }


            float Sobel(float2 coord, bool horizontal)
            {
                float3x3 sobelMatrix = horizontal ? float3x3(
					+1, 0, -1,
					+2, 0, -2,
					+1, 0, -1
				) : float3x3(
					 1,  2,  1,
					 0,  0,  0,
					-1, -2, -1
				);

              
                float3 sum = 0;
				for (int y = -1; y <= 1; y++)
				{
					for (int x = -1; x <= 1; x++)
					{
						float2 offset = float2(x, y);
						float3 sample = tex2D(_MainTex, coord + offset * _MainTex_TexelSize.xy).rgb;
						sum += sample * sobelMatrix[x + 1][y + 1];
					}
				}

				return sum.r;
            }

            float GetSobelNeighbours(float2 coord)
			{
				float2 sobelX = float2(Sobel(coord, true), 0);
				float2 sobelY = float2(0, Sobel(coord, false));
				float sobel = length(sobelX + sobelY);
				return sobel;
			}

            float GetSobelResultHV(float2 coord)
            {
                float sobelH = Sobel(coord, true);
                float sobelV = Sobel(coord, false);

                float sobelMax = max(sobelH, sobelV);
                sobelMax = max(sobelMax, GetSobelNeighbours(coord));
                return sobelMax;
            }

            float4 GetDownscaledColor(float2 coord)
            {
                int s = pow(2, _DownscaleSteps);
                coord *= s;
                coord = floor(coord);
                coord /= s;
                

                return tex2D(_MainTex, coord);
            }


            fixed4 frag (v2f i) : SV_Target
            {
               const float4x4 bayer4 = float4x4(
                   0, 8, 2, 10,
                   12, 4, 14, 6,
                   3, 11, 1, 9,
                   15, 7, 13, 5
               );
               const int dim = 4;
               
               // sample the texture
               fixed4 col = tex2D(_MainTex, i.uv);
               float sobelMax = GetSobelResultHV(i.uv);
               
               col.rgb -= abs(sobelMax);

               float2 screenPos = i.uv * _MainTex_TexelSize.zw;
               int x = int(screenPos.x);
               int y = int(screenPos.y);
               float bayer = bayer4[x % dim][y % dim] * (1.0f / (dim*dim));
               bayer -= 0.5f;
               
               float spread = 0.005f;
               
               float4 noised = col + bayer * spread;
               
               // float sobel = GetSobelResultHV(i.uv);
                        
               // float4 downscaled = GetDownscaledColor(i.uv);

            
               // float4 final = downscaled;
               // final -= sobel;

               float4 quantizedColor = QuantizeColor(noised, _QuantizationValue);
               //return final;
               return noised;

            }
            ENDCG
        }
    }
}
