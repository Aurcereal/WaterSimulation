// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/BilateralDepthBlur1D"
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

            #define PI 3.141592

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

            sampler2D GaussianKernel;
            int KernelRadius;

            int ScreenWidth;
            int ScreenHeight;

            float DepthBlurBilateralFalloff;

            fixed4 frag(vOut i) : SV_Target
            {
                float2 oneTexel = 1./float2(ScreenWidth, ScreenHeight);
                float2 oneOffset = oneTexel*float2(1.,0.);

                float4 sumVal;
                float totalWeight = 0.;

                float depth = tex2D(_MainTex, i.uv).r;

                for(int l=-KernelRadius; l<=KernelRadius; l++) {
                    float otherDepth = tex2D(_MainTex, i.uv+l*oneOffset).r;
                    float t = DepthBlurBilateralFalloff * (otherDepth - depth);
                    float weightMultiplier = exp(-t*t);

                    float weight = tex2D(GaussianKernel, float2(float(l+KernelRadius)/(2.0*KernelRadius), 0.5)).r * weightMultiplier;
                    totalWeight += weight; // Can't assume normalized anymore

                    sumVal += tex2D(_MainTex, i.uv+l*oneOffset) * weight;
                }

                return sumVal/totalWeight;
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5

            #define PI 3.141592

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

            sampler2D GaussianKernel;
            int KernelRadius;

            int ScreenWidth;
            int ScreenHeight;

            float DepthBlurBilateralFalloff;

            fixed4 frag(vOut i) : SV_Target
            {
                float2 oneTexel = 1./float2(ScreenWidth, ScreenHeight);
                float2 oneOffset = oneTexel*float2(0.,1.);

                float4 sumVal;
                float totalWeight = 0.;

                float depth = tex2D(_MainTex, i.uv).r;

                for(int l=-KernelRadius; l<=KernelRadius; l++) {
                    float otherDepth = tex2D(_MainTex, i.uv+l*oneOffset).r;
                    float t = DepthBlurBilateralFalloff * (otherDepth - depth);
                    float weightMultiplier = exp(-t*t);

                    float weight = tex2D(GaussianKernel, float2(float(l+KernelRadius)/(2.0*KernelRadius), 0.5)).r * weightMultiplier;
                    totalWeight += weight; // Can't assume normalized anymore

                    sumVal += tex2D(_MainTex, i.uv+l*oneOffset) * weight;
                }

                return sumVal/totalWeight;
            }
            ENDCG
        }
    }
}