// ============================================================
//  ImageAdjust.shader  –  HexaForge 3D to 2D Asset Capture
//
//  Simple CG (CGPROGRAM) image-effect shader for:
//  Brightness, Contrast, Saturation, Exposure, Gamma
//
//  Applied via Graphics.Blit() after the main render.
//  Uses UnityCG.cginc (built-in, URP-compatible for Blit).
// ============================================================

Shader "Hidden/HexaForge/ImageAdjust"
{
    Properties
    {
        _MainTex    ("Base (RGB)", 2D) = "white" {}
        _Brightness ("Brightness",  Float) =  0.0
        _Contrast   ("Contrast",    Float) =  1.0
        _Saturation ("Saturation",  Float) =  1.0
        _Exposure   ("Exposure",    Float) =  0.0
        _Gamma      ("Gamma",       Float) =  1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float     _Brightness;
            float     _Contrast;
            float     _Saturation;
            float     _Exposure;
            float     _Gamma;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // ── Exposure  (mul by 2^exp) ───────────────────
                col.rgb *= pow(2.0, _Exposure);

                // ── Brightness  (additive) ─────────────────────
                col.rgb += _Brightness;

                // ── Contrast  (pivot around 0.5) ───────────────
                col.rgb = (col.rgb - 0.5) * _Contrast + 0.5;

                // ── Saturation  (luma-weighted desaturate) ─────
                float luma = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                col.rgb = lerp(float3(luma, luma, luma), col.rgb, _Saturation);

                // ── Gamma  (power-curve) ───────────────────────
                col.rgb = pow(max(col.rgb, 0.0001), 1.0 / max(_Gamma, 0.0001));

                // Clamp colour, preserve original alpha
                col.rgb = clamp(col.rgb, 0.0, 1.0);
                return col;
            }
            ENDCG
        }
    }

    FallBack Off
}
