
Shader "Unlit/InstancedParticle2D"
{
    Properties
    {
        _Radius ("Radius", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing//#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma editor_sync_compilation
			#pragma target 4.5

            StructuredBuffer<float2> positionBuffer;
            StructuredBuffer<float4> colorBuffer;

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vOut
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD1;
            };

            float _Radius;

            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                float2 pos = positionBuffer[instanceID];

                vOut o;
                v.vertex.xyz *= _Radius;
                o.vertex = mul(UNITY_MATRIX_VP, v.vertex + float4(pos.x, pos.y, 0.0, 0.0));
                o.uv = v.uv;
                o.color = colorBuffer[instanceID];
                return o;
            }

            fixed4 frag (vOut o) : SV_Target
            {
                float2 p = o.uv*2.0-1.0;
                float exists = step(length(p), 1.0);

                return float4(o.color.rgb,exists);
            }
            ENDCG
        }
    }
}
