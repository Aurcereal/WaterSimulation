// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/NormalFromDepthOrthoCam"
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

            const float Size;
            const float CausticAspect;

            const float3 CausticCamRi;
            const float3 CausticCamUp;
            const float3 CausticCamFo;

            const float3 CausticCamPos;

            const int TextureWidth;
            const int TextureHeight;

            float3 GetPosFromDepthTexture(float2 uv) {
                float2 wsp = (uv*2.-1.) * Size * float2(CausticAspect, 1.);
                float3 ro = CausticCamPos + CausticCamRi * wsp.x + CausticCamUp * wsp.y;
                float3 rd = CausticCamFo;
                float distAlongCam = tex2D(_MainTex, uv).r;
                float3 pos = ro + rd*distAlongCam;
                return pos;
            }

            const float DepthDifferenceCutoff;

            inline bool invalidDepthPair(float currDepth, float otherDepth) { // Assume currDepth is valid
                return abs(currDepth-otherDepth) > DepthDifferenceCutoff || otherDepth >= 100000.0;
            }

            inline bool invalidDepth(float depth) {
                return depth >= 100000.0;
            }

            fixed4 frag(vOut i) : SV_Target
            {
                float2 oneTexel = 1./float2(TextureWidth, TextureHeight);

                float3 pos = GetPosFromDepthTexture(i.uv);

                float depth = tex2D(_MainTex, i.uv).r;
                if(invalidDepth(depth)) return float4(1.,.75,.79,0.);
                float depthPositiveX = tex2D(_MainTex, i.uv+float2(oneTexel.x, 0.)).r;
                float depthPositiveY = tex2D(_MainTex, i.uv+float2(0., oneTexel.y)).r;

                if(invalidDepth(depthPositiveX) || invalidDepth(depthPositiveY)) return float4(0.,0.,0.,0.);

                float normDir = 1.;

                float3 posX;
                if(invalidDepthPair(depth, depthPositiveX)) {
                    posX = GetPosFromDepthTexture(i.uv+float2(oneTexel.x, 0.));
                    normDir *= -1.;
                } else {
                    posX = GetPosFromDepthTexture(i.uv-float2(oneTexel.x, 0.));
                }

                float3 posY;
                if(invalidDepthPair(depth, depthPositiveY)) {
                    posY = GetPosFromDepthTexture(i.uv+float2(0., oneTexel.y));
                    normDir *= -1.;
                } else {
                    posY = GetPosFromDepthTexture(i.uv-float2(0., oneTexel.y));
                }
                
                float3 ddx = posX - pos;
                float3 ddy = posY - pos;

                float3 norm = normDir * normalize(cross(ddy, ddx));

                return float4(norm, 1.);
            }
            ENDCG
        }
    }
}