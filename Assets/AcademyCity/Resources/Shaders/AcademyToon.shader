// Academy City toon shader (URP): two-tone cel shading, a soft rim light and an ink outline.
// The look gets darker later in the story by changing the light, shade colour and post effects, not this shader.
Shader "AcademyCity/Toon"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
        _ShadeColor ("Shade colour (multiplies the texture)", Color) = (0.62,0.56,0.72,1)
        _ShadeStep ("Shade edge", Range(0,1)) = 0.48
        _ShadeSoft ("Shade softness", Range(0.001,0.3)) = 0.03
        _RimColor ("Rim light", Color) = (1,0.9,0.8,0.35)
        _RimPower ("Rim tightness", Range(1,10)) = 4
        _OutlineColor ("Outline colour", Color) = (0.12,0.07,0.08,1)
        _OutlineWidth ("Outline width (pixels)", Range(0,6)) = 1.6
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _ShadeColor;
            half _ShadeStep;
            half _ShadeSoft;
            half4 _RimColor;
            half _RimPower;
            half4 _OutlineColor;
            float _OutlineWidth;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ToonForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 normalWS : TEXCOORD1; float3 positionWS : TEXCOORD2; float fog : TEXCOORD3; };

            Varyings vert(Attributes i)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(i.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                float3 n = normalize(i.normalWS);
                Light light = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half ndl = dot(n, light.direction) * 0.5 + 0.5;
                half lit = smoothstep(_ShadeStep - _ShadeSoft, _ShadeStep + _ShadeSoft, ndl * lerp(0.4, 1.0, light.shadowAttenuation));
                half3 col = lerp(tex.rgb * _ShadeColor.rgb, tex.rgb, lit) * light.color;
                col += tex.rgb * SampleSH(n) * 0.3;
                float3 v = normalize(GetWorldSpaceViewDir(i.positionWS));
                half rim = pow(1.0 - saturate(dot(n, v)), _RimPower) * lit;
                col += _RimColor.rgb * rim * _RimColor.a;
                col = MixFog(col, i.fog);
                return half4(col, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes i)
            {
                Varyings o;
                float4 cs = TransformObjectToHClip(i.positionOS.xyz);
                float3 nWS = TransformObjectToWorldNormal(i.normalOS);
                float2 nCS = mul((float3x3)GetWorldToHClipMatrix(), nWS).xy;
                float len = max(length(nCS), 1e-5);
                cs.xy += (nCS / len) * _OutlineWidth * 2.0 / _ScreenParams.xy * cs.w;
                o.positionCS = cs;
                return o;
            }

            half4 frag(Varyings i) : SV_Target { return _OutlineColor; }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
        UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
        UsePass "Universal Render Pipeline/Lit/DEPTHNORMALS"
    }
}
