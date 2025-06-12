// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/NormalFromDepth"
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
			#pragma target 4.5

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vOut
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;

            vOut vert (vIn v)
            {
                vOut o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                return o;
            }

            const float FovY;
            const float Aspect;

            const float3 CamRi;
            const float3 CamUp;
            const float3 CamFo;

            const float3 CamPos;

            const int ScreenWidth;
            const int ScreenHeight;

            float3 Raycast(float2 uv) {
                float2 p = uv*2.0-1.0;

                float3 rd = normalize(float3(tan(FovY*0.5) * (p * float2(Aspect, 1.0)), 1.0));
                return CamRi * rd.x + CamUp * rd.y + CamFo * rd.z; // float3x3(row, row, row) not column..
            }

            float3 GetPosFromDepthTexture(float2 uv) {
                float3 rd = Raycast(uv);
                float distAlongCam = tex2D(_MainTex, uv).r;
                float distAlongRay = distAlongCam / dot(rd, CamFo);
                float3 pos = CamPos + rd*distAlongRay;
                return pos;
            }

            inline bool invalidDepth(float depth) {
                return depth >= 100000.0;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                float2 oneTexel = 1./float2(ScreenWidth, ScreenHeight);

                float3 pos = GetPosFromDepthTexture(i.uv);

                float depth = tex2D(_MainTex, i.uv);
                if(invalidDepth(depth)) return float4(1.,.75,.79,0.);
                float depthPositiveX = tex2D(_MainTex, i.uv+float2(oneTexel.x, 0.));
                float depthPositiveY = tex2D(_MainTex, i.uv+float2(0., oneTexel.y));

                float3 posX;
                if(invalidDepth(depthPositiveX)) {
                    posX = GetPosFromDepthTexture(i.uv+float2(oneTexel.x, 0.));
                } else {
                    posX = GetPosFromDepthTexture(i.uv-float2(oneTexel.x, 0.));
                }

                float3 posY;
                if(invalidDepth(depthPositiveY)) {
                    posY = GetPosFromDepthTexture(i.uv+float2(0., oneTexel.y));
                } else {
                    posY = GetPosFromDepthTexture(i.uv-float2(0., oneTexel.y));
                }
                
                float3 ddx = posX - pos;
                float3 ddy = posY - pos;

                float3 norm = normalize(cross(ddy, ddx));

                return float4(norm, 1.0);
            }
            ENDCG
        }
    }
}