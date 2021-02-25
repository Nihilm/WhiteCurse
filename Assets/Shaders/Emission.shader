Shader "Unlit/Emission"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
                float linearDepth : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            sampler2D_float _CameraDepthTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                output.screenPos = ComputeScreenPos(output.pos);
                output.linearDepth = -(UnityObjectToViewPos(input.vertex).z * _ProjectionParams.w);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 uv = input.screenPos.xy / input.screenPos.w;
                float camDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                camDepth = Linear01Depth (camDepth);
                float diff = saturate(input.linearDepth - camDepth);
                if(diff < 0.001)
                    col = float4(1, 0, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}