// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/GaussianBlur1D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass // make do uble pass later
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

            fixed4 frag(vOut i) : SV_Target
            {
                float2 oneTexel = 1./float2(ScreenWidth, ScreenHeight);

                float4 sumVal;

                for(int x=-KernelRadius; x<=KernelRadius; x++) {
                    sumVal += tex2D(_MainTex, i.uv+float2(x*oneTexel.x, 0.)) * tex2D(GaussianKernel, float2(float(x+KernelRadius)/(2.0*KernelRadius), 0.5)).r;
                }

                return sumVal;// * float4(1.,0.4,0.4,1.);
            }
            ENDCG
        }
    }
}