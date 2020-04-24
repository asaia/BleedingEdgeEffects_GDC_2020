Shader "GDC Demo/UV Transformation"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        BlendOp [_BlendOp]
        Blend One One

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
            
            float3 _Center;
            float _Radius;
            float _Hardness;
            float _Strength;

            struct appdata
            {
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 positionWS : TEXCOORD1;
            };

            float SphereMask(float3 position, float3 center, float radius, float hardness)
            {
                return 1 - saturate((distance(position, center) - radius) / (1 - hardness));
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                //Store Vertex world position
				o.positionWS = mul(unity_ObjectToWorld, v.vertex);

                //Transform vertex position into UV space
                o.uv = v.uv;
				float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2( 2, 2) - float2(1, 1));
				o.vertex = uv;
            
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {            
                float falloff = SphereMask(i.positionWS, _Center.xyz, _Radius, _Hardness);
                return float4(falloff * _Strength, 0, 0, 0.0);
            }
            ENDCG
        }
    }
}