// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Unlit/Primitive/Circle"
{
    Properties
    {
        _Color ("Color", Color) = (1.0,1.0,1.0,1.0)
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
			#pragma target 4.5

            struct vIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vOut
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            vOut vert (vIn v, uint instanceID : SV_InstanceID)
            {
                vOut o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 _Color;

            fixed4 frag (vOut o) : SV_Target
            {
                float2 p = o.uv*2.0-1.0;
                float exists = step(length(p), 1.0);

                return float4(_Color.rgb, exists);
            }
            ENDCG
        }
    }
}
