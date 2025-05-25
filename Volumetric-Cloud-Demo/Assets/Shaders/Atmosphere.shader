Shader "Hidden/AtmosphereBackground"
{
    Properties
    {
        _AtmosphereExposure ("Exposure", Float) = 20.0
        _AtmosphereHeight ("Atmosphere Height", Float) = 100000.0
        _MieG ("Mie Phase G", Float) = 0.85
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        // Pass 0: 渲染背景区域的大气散射
        Pass
        {
            Name "AtmosphereBackground"
            ZWrite Off
            ZTest Greater
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "CloudInput.hlsl"
            #include "AtmosphereScattering.hlsl"

            float _AtmosphereExposure;
            float _AtmosphereHeight;
            float _MieG;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.vertex.xyz);
                o.positionCS = posInputs.positionCS;
                o.uv = v.uv;

                float2 ndc = v.uv * 2.0 - 1.0;
                float4 ndcPos = float4(ndc, 1, 1);
                float3 viewDir = mul(unity_CameraInvProjection, ndcPos).xyz;
                o.viewDirWS = mul(unity_CameraToWorld, float4(viewDir, 0.0)).xyz;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).x;
                float dstToObj = LinearEyeDepth(depth, _ZBufferParams);
                if (dstToObj < 100000.0)
                    return half4(0, 0, 0, 0); // 只在远处绘制

                float3 cameraPos = GetCameraPositionWS();
                float3 viewDir = normalize(i.viewDirWS);
                Light mainLight = GetMainLight();

                float3 lightDir = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;

                float3 transmittance;
                float3 color = IntegrateScattering(
                    cameraPos,
                    viewDir,
                    _AtmosphereHeight,
                    lightDir,
                    lightColor * _AtmosphereExposure,
                    transmittance,
                    _MieG
                );

                return half4(color, 1.0); // 全 alpha（用作写入背景）
            }
            ENDHLSL
        }

        // Pass 1: 把 RT 混合进屏幕（体积云也可采样）
        Pass
        {
            Name "BlendAtmosphere"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert_blend
            #pragma fragment frag_blend
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "CloudInput.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert_blend(appdata v)
            {
                v2f o;
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.vertex.xyz);
                o.positionCS = posInputs.positionCS;
                o.uv = v.uv;
                return o;
            }

            half4 frag_blend(v2f i) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_AtmosphereBackgroundTex, sampler_AtmosphereBackgroundTex, i.uv);
            }
            ENDHLSL
        }
    }
}
