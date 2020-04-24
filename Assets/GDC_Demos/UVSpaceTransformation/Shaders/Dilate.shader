Shader "GDC Demo/Dilate"
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
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            static const int MAX_STEPS = 8;
			static const int TEXEL_DIST = 1;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 offsets[8] = {float2(-TEXEL_DIST,0), float2(TEXEL_DIST,0), float2(0,TEXEL_DIST), float2(0,-TEXEL_DIST), float2(-TEXEL_DIST,TEXEL_DIST), float2(TEXEL_DIST,TEXEL_DIST), float2(TEXEL_DIST,-TEXEL_DIST), float2(-TEXEL_DIST,-TEXEL_DIST)};
				float2 uv = i.uv;
				float4 sample = tex2D(_MainTex, uv);

				float4 sampleMax = sample;
				for	(int i = 0; i < MAX_STEPS; i++)
				{
					float2 curUV = uv + offsets[i] * _MainTex_TexelSize.xy;
					float4 offsetsample = tex2D(_MainTex, curUV);
					sampleMax = max(offsetsample, sampleMax);
				}

				sample = sampleMax;

				return sample;
            }
            ENDCG
        }
    }
}
