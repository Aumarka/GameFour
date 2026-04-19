Shader "Hidden/DunGen/Dungeon Crawler Sample/Create Distance Field"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TextureSize ("Texture Size", Float) = 512
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            float _TextureSize;

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
                output.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                output.uv = uv;

                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                static const int MAX_RADIUS = 8;
                const float maxRadiusF = 8.0;
                const float texelSize = 1.0 / _TextureSize;
                const float2 halfTexel = float2(0.5 * texelSize, 0.5 * texelSize);

                float2 uv = (floor(input.uv * _TextureSize) / _TextureSize) + halfTexel;
                uv = clamp(uv, halfTexel, 1.0 - halfTexel);

                float startSample = _MainTex.Sample(sampler_MainTex, uv).r > 0.0 ? 1.0 : 0.0;
                float minDist = maxRadiusF;

                [loop]
                for (int y = -MAX_RADIUS; y < MAX_RADIUS; y++)
                {
                    [loop]
                    for (int x = -MAX_RADIUS; x < MAX_RADIUS; x++)
                    {
                        float2 offset = float2((float)x, (float)y);
                        float2 curUV = uv + offset * texelSize;
                        curUV = (floor(curUV * _TextureSize) / _TextureSize) + halfTexel;
                        curUV = clamp(curUV, halfTexel, 1.0 - halfTexel);

                        float curSample = _MainTex.Sample(sampler_MainTex, curUV).r > 0.0 ? 1.0 : 0.0;

                        if (curSample != startSample)
                        {
                            minDist = min(minDist, length(offset));
                        }
                    }
                }

                float outputValue = (minDist - 0.5) / (maxRadiusF - 0.5);
                outputValue *= (startSample == 0.0) ? -1.0 : 1.0;
                outputValue = (outputValue + 1.0) * 0.5;

                return float4(outputValue, outputValue, outputValue, 1.0);
            }
            ENDHLSL
        }
    }
}
