// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Unlit/Primitive/OutlineBox"
{
    Properties
    {
        _Color ("Color", Color) = (1.0,1.0,1.0,1.0)
        _Dim ("Dimensions", Vector) = (1.0,1.0,0.0,0.0)
        _Thickness ("Thickness", Float) = 0.1
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
            float2 _Dim;
            float _Thickness;

            fixed4 frag (vOut o) : SV_Target
            {
                float2 p = _Dim * (o.uv-0.5);
                p = abs(p) - _Dim*.5;
                float exists = max(step(-_Thickness, p.x), step(-_Thickness, p.y));

                return float4(_Color.rgb, exists);
            }
            ENDCG
        }
    }
}
