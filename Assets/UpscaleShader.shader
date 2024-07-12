Shader "Unlit/UpscaleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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


                float2 screenPos = i.uv * _MainTex_TexelSize.zw;
                int x = int(screenPos.x);
                int y = int(screenPos.y);
                float bayer = bayer4[x % dim][y % dim] * (1.0f / (dim*dim));
                bayer -= 0.5f;
               
                float spread = 0.005f;
                
                float4 noised = col + bayer * spread;

                float4 quantizedColor = QuantizeColor(noised, 8);

                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                
                return quantizedColor;
            }
            ENDCG
        }
    }
}
