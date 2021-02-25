Shader "Hidden/PapyrusMask" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _LightDirection ("LightDirection", Vector) = (0.2,0.2,1.0,0.0)
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
            float4 _MainTex_TexelSize;
            sampler2D _MaskMap;

            float4 _LightDirection;

            float3 hash(uint3 x){
                const uint k = 0x41c64e6d;
                x = ((x>>0x8)^x.yzx)*k;
                x = ((x>>0x8)^x.yzx)*k;
                x = ((x>>0x8)^x.yzx)*k;
                return float3(x)*(1.0/float(0xffffffff));
            }
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
            float3 worley(in float2 p){
                float2 n = floor(p);
                float2 f = frac(p);
                float3 r = float3(1,1,1);
                for(int x=-1;x<=1;x++)
                for(int y=-1;y<=1;y++){
                    float2 xy = float2(x,y);
                    float2 w = hash(uint3(abs(n + xy),0)).xy + xy - f;
                    float d = length(w);
                    if(d < r.z) r = float3(w, d);
                }
                return r;
            }
            float fbm(in float2 p){
                const float2x2 m = float2x2(1.6,1.2,-1.2,1.6);
                float total = 0.0f, amplitude = 0.5f;
                for(int i = 0; i < 4; i++){
                    total += noise(p+1024.0) * amplitude;
                    p = mul(p, m) + amplitude * noise(p+2048.0+amplitude*_Time.y);
                    amplitude *= 0.5;
                }
                return floor(total*16)/16;
            }
            float3 surfaceNormal(in float2 uv){
                float size = 4.0, weight = 1.0;
                float3 normal = float3(0,0,2);
                float2 offset = 0.01 * float2(sin(uv.x*48),cos(uv.y*36));
                for(int i=0;i<4;i++){
                    float3 w = worley(size * (uv + offset));
                    normal.xy += weight * w.xy * w.z;
                    size *= 2; weight *= 0.8;
                }
                return normalize(normal);
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 mask = tex2D(_MaskMap, i.uv + 0.01*noise(i.uv*16));
                fixed4 color = tex2D(_MainTex, i.uv);
                float intensity = Luminance(color.rgb);
                intensity = (0.2+0.8*intensity) * step(0.01, intensity);

                float3 N = surfaceNormal(i.uv * _MainTex_TexelSize.zw * 0.001 + 0.02 * _WorldSpaceCameraPos.xy);
                float3 L = normalize(_LightDirection.xyz);
                float NdotL = max(dot(N, L), 0.0);
                fixed4 background = fixed4(0.1,0.3,0.4,1);
                background.rgb = lerp(background.rgb, float3(1,0.96,0.88), NdotL);

                float depth = 0.75*(intensity + 0.5*fbm(i.uv*8 + 8*mask.a*intensity));
                float alpha = saturate(2*mask.a - depth);

                color = lerp(background, color, min(color.a, alpha));
                return color;
            }
            ENDCG
        }
    }
}