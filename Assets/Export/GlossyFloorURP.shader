// Glossy textured floor for URP (WebGL2 compatible).
// Put the plane at the reflection height, with a mirrored duplicate of your scene below it.
// Where alpha < 1 the duplicate shows through the floor. The floor also receives URP main-light shadows.
Shader "Custom/URP/GlossyFloor"
{
    Properties
    {
        [Header(Floor)]
        _BaseMap        ("Floor Texture",          2D)     = "white" {}
        _Color          ("Floor Tint",             Color)  = (1, 1, 1, 1)
        _TexScale       ("Texture Tiles per Meter", Float) = 0.25

        [Header(Shadows)]
        _ShadowTint     ("Shadow Color",           Color)  = (0.55, 0.60, 0.70, 1)

        [Header(Where the floor is see through)]
        [Toggle] _FullFloor ("Whole Floor (ignore area box)", Float) = 1
        _AreaCenter     ("Area Center (X, Z)",     Vector) = (0, 0, 0, 0)
        _AreaHalfSize   ("Area Half Size (X, Z)",  Vector) = (20, 20, 0, 0)
        _AreaCorner     ("Area Corner",            Float)  = 3.0
        _EdgeSoftness   ("Edge Softness",          Float)  = 4.0
        _EdgeWobble     ("Edge Wobble",            Float)  = 3.0
        _SeeThrough     ("See Through (head-on)",  Range(0,1)) = 0.30
        _Fresnel        ("Fresnel Amount",         Range(0,1)) = 0.50
        _FresnelPower   ("Fresnel Power",          Float)  = 3.0

        [Header(Patches that hide the reflection)]
        _Wear           ("Wear Amount",            Range(0,1)) = 0.45
        _PatchScale     ("Patch Frequency (lower = bigger patches)", Float) = 0.12

        [Header(Optional tile lines)]
        _GroutColor     ("Grout Color",            Color)  = (0.35, 0.36, 0.40, 1)
        _GroutStrength  ("Grout Strength (0 = off)", Range(0,1)) = 0
        _TileSize       ("Grout Tile Size",        Float)  = 2.0
        _GroutPx        ("Grout Thickness (px)",   Float)  = 1.0

        [Toggle] _DebugAlpha ("Debug: show alpha (white = opaque, black = see-through)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "GlossyFloor"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0      // needed for fwidth() on GLES3 / WebGL2

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)     // keeps the shader SRP Batcher compatible
                half4  _Color;
                half4  _ShadowTint;
                half4  _GroutColor;
                float  _TexScale;
                float  _FullFloor;
                float  _DebugAlpha;
                float4 _AreaCenter;
                float4 _AreaHalfSize;
                float  _AreaCorner;
                float  _EdgeSoftness;
                float  _EdgeWobble;
                float  _SeeThrough;
                float  _Fresnel;
                float  _FresnelPower;
                float  _Wear;
                float  _PatchScale;
                float  _GroutStrength;
                float  _TileSize;
                float  _GroutPx;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                return OUT;
            }

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i),                 hash(i + float2(1, 0)), f.x),
                            lerp(hash(i + float2(0, 1)),  hash(i + float2(1, 1)), f.x), f.y);
            }

            float sdRoundBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 xz = IN.positionWS.xz;

                // 1. where is the floor see-through? (area box, or the whole floor)
                float d    = sdRoundBox(xz - _AreaCenter.xy, _AreaHalfSize.xy, _AreaCorner)
                           + (vnoise(xz * 0.3) - 0.5) * _EdgeWobble;
                float mask = 1.0 - smoothstep(-_EdgeSoftness, _EdgeSoftness, d);
                mask = lerp(mask, 1.0, _FullFloor);

                // 2. fresnel: more see-through at grazing angles
                float3 V    = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float  fres = pow(1.0 - saturate(V.y), _FresnelPower);

                // 3. broad, static patches that hide the reflection (dull / worn areas)
                float n      = vnoise(xz * _PatchScale) * 0.65 + vnoise(xz * _PatchScale * 4.0) * 0.35;
                float polish = 1.0 - _Wear * smoothstep(0.35, 0.75, n);

                // 4. optional grout lines
                float2 g         = xz / _TileSize;
                float2 gd        = abs(frac(g - 0.5) - 0.5) / max(fwidth(g), 1e-5);
                float  groutMask = (1.0 - saturate(min(gd.x, gd.y) - _GroutPx)) * _GroutStrength;

                float see   = mask * polish * saturate(_SeeThrough + fres * _Fresnel);
                float alpha = max(1.0 - see, groutMask);          // grout is always opaque

                // 5. floor colour = texture * tint, darkened by main-light shadows
                half3 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, xz * _TexScale).rgb * _Color.rgb;
                half  shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(IN.positionWS));
                col *= lerp(_ShadowTint.rgb, half3(1, 1, 1), shadow);
                col  = lerp(col, _GroutColor.rgb, groutMask);

                if (_DebugAlpha > 0.5) return half4(alpha.xxx, 1);
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}