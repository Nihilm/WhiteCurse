Shader "Hidden/Sketch" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _LineWidth ("LineWidth", Range(0, 8)) = 1.0
        _DepthThreshold ("DepthThreshold", Range(0, 10)) = 0.5
        _NormalThreshold ("NormalThreshold", Range(0, 10)) = 0.5
        _DepthNormalThreshold ("DepthNormalThreshold", Range(0, 1)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler2D _CameraDepthNormalsTexture;
            float4 _MainTex_TexelSize;

            float _LineWidth;
            float _DepthThreshold;
            float _NormalThreshold;
            float _DepthNormalThreshold;
            float4x4 _ViewProjectInverse;

            float hash(uint2 x){
                uint2 q = 0x41c64e6d * ((x>>0x1) ^ (x.yx));
                uint n = 0x41c64e6d * ((q.x) ^ (q.y>>0x3));
                return float(n) * (1.0/float(0xffffffff));
            }
            float noise(in float2 p){
                const uint2 k = uint2(0,1);
                uint2 i = uint2(floor(p));
                float2 f = frac(p);
                float2 u = f*f*(3.0-2.0*f);
                return lerp(lerp(hash(i + k.xx), hash(i + k.yx), u.x),
                            lerp(hash(i + k.xy), hash(i + k.yy), u.x), u.y);
            }
            float2 _rotate(in float2 p, in float a){return mul(p, float2x2(sin(a),cos(a),-cos(a),sin(a)));}

            float SampleDepth(float2 uv){
                float depth = tex2D(_CameraDepthTexture, uv).r;
                return Linear01Depth(depth);// * _ProjectionParams.z;
            }
            float3 SampleNormal(float2 uv){
                float3 normal; float depth;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, uv),depth,normal);
                return normal*0.5f-0.5f;
            }
            float EdgeDetection(float2 uv, float width){
                float2 delta = float2(-floor(width * 0.5), ceil(width * 0.5));
                float d0, d1, d2, d3; float3 n0, n1, n2, n3;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,uv+_MainTex_TexelSize.xy*delta.xx),d0,n0);
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,uv+_MainTex_TexelSize.xy*delta.yy),d1,n1);
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,uv+_MainTex_TexelSize.xy*delta.yx),d2,n2);
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,uv+_MainTex_TexelSize.xy*delta.xy),d3,n3);
                float dhr = d1-d0, dvt = d3-d2;
                float edgeDepth = sqrt(dhr * dhr + dvt * dvt);
                float3 nhr = 0.5*(n1-n0), nvt = 0.5*(n3-n2);
                float edgeNormal = sqrt(dot(nhr,nhr) + dot(nvt,nvt));

                float NdotV = 1+8*saturate((1 - dot(n0,float3(0,0,1)) - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
                edgeDepth *= step(_DepthThreshold * d0 * NdotV, edgeDepth);
                edgeNormal *= step(_NormalThreshold, edgeNormal);
                return max(edgeDepth, edgeNormal);
            }
            float fbm(in float2 p){
                const float2x2 m = float2x2(1.5,1.3,-1.1,1.7);
                float total = 0.0f, amplitude = 0.5f;
                for(int i = 0; i < 4; i++){
                    total += noise(p+1024.0) * amplitude;
                    p = mul(p, m) + amplitude*noise(0.5*p+2048.0);
                    amplitude *= 0.5;
                }
                return total;
            }
            float3 tint(in float2 uv, float intensity){
                intensity += lerp(-0.2,0.2,fbm(uv * 0.05));
                intensity = floor(intensity * 8) / 8;
                intensity += lerp(-0.2,0.2,fbm(uv * 0.01));

                float3 color = float3(0.08,0.16,0.2);
                color = lerp(color,float3(0.2,0.3,0.4), smoothstep(0.0,0.3,intensity));
                color = lerp(color,float3(0.4,0.5,0.5), smoothstep(0.3,0.5,intensity));
                color = lerp(color,float3(0.7,0.6,0.5), smoothstep(0.5,0.7,intensity));
                color = lerp(color,float3(0.9,0.8,0.6), smoothstep(0.7,0.9,intensity));
                color = lerp(color,float3(1.0,1.0,0.8), smoothstep(1.0,2.0,intensity));

                return color;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 noiseUV = i.uv * _MainTex_TexelSize.zw;// + _WorldSpaceCameraPos.xy;
                float2 uv = i.uv;

                i.uv.x += 0.012*noise(noiseUV * 0.06 + 1023);
                i.uv.y += 0.016*noise(noiseUV * 0.04 + 9742);

                fixed4 color = tex2D(_MainTex, i.uv);
                float intensity = 1-Luminance(GammaToLinearSpace(color.rgb));
                float mask = smoothstep(0,0.5,fbm(noiseUV * 0.01 + 7091));
                mask = 0.4*mask+0.6*floor(mask*8)/8;
                color = fixed4(tint(noiseUV, 1-intensity), min(color.a, mask));

                float edgeNoise = noise(noiseUV * 0.02);
                uv += 0.008*noise(noiseUV * 0.01 + 2093);
                float edge = saturate(1.0-EdgeDetection(uv, _LineWidth*10*edgeNoise));
                color = lerp(fixed4(0,0,0,1), color, pow(edge, 3.0));
                
                return color;
            }
            ENDCG
        }
    }
}