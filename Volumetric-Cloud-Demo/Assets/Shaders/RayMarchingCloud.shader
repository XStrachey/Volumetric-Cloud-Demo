Shader "Hidden/Ray Marching Cloud"
{
    Properties
    {
        _MainTex ("", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        
        HLSLINCLUDE
        #include "CloudInput.hlsl"
        #include "CloudShape.hlsl"
        #include "CloudShade.hlsl"
        #include "Intersection.hlsl"
        #include "AtmosphereScattering.hlsl"
        ENDHLSL
        
        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata
            {
                float4 positionOS : POSITION;
                float2 texcoord   : TEXCOORD;
            };
            
            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 viewDir    : TEXCOORD1;
            };
            
            v2f vert(appdata v)
            {
                v2f output;
                
                VertexPositionInputs vertexPos = GetVertexPositionInputs(v.positionOS.xyz);
                output.positionCS = vertexPos.positionCS;
                output.uv = v.texcoord;
                
                float3 viewDir = mul(unity_CameraInvProjection, float4(v.texcoord * 2.0 - 1.0, 0, -1)).xyz;
                output.viewDir = mul(unity_CameraToWorld, float4(viewDir, 0)).xyz;
                
                return output;
            }
            
            half4 frag(v2f input): SV_Target
            {
                float3 cameraPos = GetCameraPositionWS();
                float3 viewDir = normalize(input.viewDir);

                float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv).x;
                float dstToObj = LinearEyeDepth(depth, _ZBufferParams);

                float2 cloudHit = RayCloudLayerDst(_EarthCenter, _EarthRadius, _CloudHeightMin, _CloudHeightMax, cameraPos, viewDir, true);
                float dstToCloud = cloudHit.x;
                float dstInCloud = cloudHit.y;

                if (dstInCloud <= 0 || dstToObj < dstToCloud)
                    return half4(0, 0, 0, 1);

                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;

                float dstLimit = min(dstToObj - dstToCloud, dstInCloud);
                float stepLen = 128.0;
                stepLen = lerp(8, 1, abs(viewDir.y)) * stepLen;

                float cosTheta = dot(viewDir, lightDir);
                float phase = HGScatterMax(cosTheta, _VolumetricCloudForwardMieG, _VolumetricCloudBackwardMieG);
                phase = mieScatterBase + phase * mieScatterMultiply;

                float3 entryPoint = cameraPos + viewDir * dstToCloud;

                float totalDensity = 0.0;
                float transmittance = 1.0;
                float3 accumulatedColor = 0;
                float dstTravelled = 0;

                const int maxSteps = 128;

                [loop]
                for (int i = 0; i < maxSteps; ++i)
                {
                    if (dstTravelled > dstLimit || transmittance < 0.01)
                        break;

                    float3 samplePos = entryPoint + dstTravelled * viewDir;
                    float heightFraction = GetHeightFractionFromSphere(samplePos);
                    if (heightFraction < 0.0 || heightFraction > 1.0)
                        break;

                    // 生成每步的扰动 step
                    float rnd = frac(sin(dot(samplePos.xyz, float3(12.9898, 78.233, 37.719))) * 43758.5453);
                    float localStep = stepLen * (0.9 + 0.2 * rnd); // 在 ±10% 范围扰动

                    float density = SampleCloudDensity(samplePos, 0);
                    if (density > 0.05)
                    {
                        float absorption = density * localStep;
                        totalDensity += absorption;

                        float dl = SampleCloudDensityAlongCone(samplePos, lightDir);
                        float brightness = saturate(dot(lightDir, float3(0,1,0))) * 0.1;

                        float3 scattering = GetLightEnergy(heightFraction, dl, density, phase, cosTheta, brightness, mainLight.color, viewDir, lightDir);
                        accumulatedColor += transmittance * scattering * absorption;

                        transmittance *= BeerLaw(absorption); // Beer-Lambert
                    }

                    dstTravelled += localStep;
                }

                float3 cloudColor = accumulatedColor;
                cloudColor *= (1.0 - exp(-totalDensity * 1.2));

                float3 atmosphereColor = SAMPLE_TEXTURE2D(_AtmosphereBackgroundTex, sampler_AtmosphereBackgroundTex, input.uv).rgb;

                // 混合公式：cloud over background
                float3 finalColor = atmosphereColor * transmittance + cloudColor * (1.0 - transmittance);

                finalColor = pow(finalColor, 1.0 / 2.2);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        pass
        {
            // 最后的颜色应当为backColor.rgb * transmittance + totalLum, 但是因为分帧渲染，混合需要特殊处理
            Blend One SrcAlpha
            
            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "CloudInput.hlsl"
            
            #pragma vertex vert_blend
            #pragma fragment frag_blend
            
            struct appdata
            {
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex: SV_POSITION;
                float2 uv: TEXCOORD0;
            };
            
            v2f vert_blend(appdata v)
            {
                v2f output;
                
                VertexPositionInputs vertexPos = GetVertexPositionInputs(v.vertex.xyz);
                output.vertex = vertexPos.positionCS;
                output.uv = v.uv;
                return output;
            }
            
            half4 frag_blend(v2f input): SV_Target
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
            }
            
            ENDHLSL
        }
    }
}
