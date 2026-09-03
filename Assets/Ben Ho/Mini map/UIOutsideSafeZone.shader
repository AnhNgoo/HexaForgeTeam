Shader "UI/Outside Safe Zone"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _ZoneCenter ("Zone Center", Vector) = (0.5, 0.5, 0, 0)
        _ZoneRadii ("Zone Radii", Vector) = (0.25, 0.25, 0, 0)
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.1)) = 0.015
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 worldPosition : TEXCOORD1;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _ZoneCenter;
            float4 _ZoneRadii;
            float _EdgeSoftness;

            v2f vert(appdata input)
            {
                v2f output;
                output.worldPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 radii = max(_ZoneRadii.xy, float2(0.0001, 0.0001));
                float distanceFromCenter =
                    length((input.uv - _ZoneCenter.xy) / radii);

                float outsideAlpha = smoothstep(
                    1.0 - _EdgeSoftness,
                    1.0,
                    distanceFromCenter
                );

                float4 _ClipRect;

                fixed4 color = tex2D(_MainTex, input.uv) * input.color;
                color.a *= outsideAlpha;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(
                    input.worldPosition.xy,
                    _ClipRect
                );
                #endif
                return color;
            }
            ENDCG
        }
    }
}