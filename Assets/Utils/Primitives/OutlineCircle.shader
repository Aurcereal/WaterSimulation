// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Unlit/Primitive/OutlineCircle"
{
    Properties
    {
        _Color ("Color", Color) = (1.0,1.0,1.0,1.0)
        _Radius ("Radius", Float) = 1.0
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
            float _Radius;
            float _Thickness;

            fixed4 frag (vOut o) : SV_Target
            {
                float2 p = _Radius*(o.uv*2.0-1.0);
                float r = length(p);
                float exists = step(_Radius - _Thickness, r) * step(r, _Radius);

                return float4(_Color.rgb, exists);
            }
            ENDCG
        }
    }
}
