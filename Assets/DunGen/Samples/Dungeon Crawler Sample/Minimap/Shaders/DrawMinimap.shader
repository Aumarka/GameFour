Shader "DunGen/Dungeon Crawler Sample/Draw Minimap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WallSize ("Wall Size", Float) = 0.03
        _FillColour ("Fill Colour", Color) = (0.5, 0.5, 0.5, 1.0)
        _WallColour ("Wall Colour", Color) = (1.0, 1.0, 1.0, 1.0)
        _ShadowColour ("Shadow Colour", Color) = (0.0, 0.0, 0.0, 1.0)
        _EdgeFadeDistance ("Edge Fade Distance", Float) = 0.1
        _EdgeFadeColour ("Edge Fade Colour", Color) = (0.0, 0.0, 0.0, 1.0)
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
            float _WallSize;
            float4 _FillColour;
            float4 _WallColour;
            float4 _ShadowColour;
            float _EdgeFadeDistance;
            float4 _EdgeFadeColour;

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
                float distanceValue = _MainTex.Sample(sampler_MainTex, input.uv).r * 2.0 - 1.0;
                float wallHalfWidth = _WallSize;

                float outlineMask = step(distanceValue, wallHalfWidth) * step(-wallHalfWidth, distanceValue);
                float fillMask = step(0.0, distanceValue);

                float3 col = fillMask * _FillColour.rgb;

                float shadowMask = step(0.0, distanceValue) * smoothstep(1.0, 0.0, distanceValue);
                col = lerp(col, _ShadowColour.rgb, shadowMask);
                col = lerp(col, _WallColour.rgb, outlineMask);

                float2 uv = input.uv;
                float edgeFadeMask = smoothstep(_EdgeFadeDistance, 0.0, uv.x);
                edgeFadeMask = max(edgeFadeMask, smoothstep(1.0 - _EdgeFadeDistance, 1.0, uv.x));
                edgeFadeMask = max(edgeFadeMask, smoothstep(_EdgeFadeDistance, 0.0, uv.y));
                edgeFadeMask = max(edgeFadeMask, smoothstep(1.0 - _EdgeFadeDistance, 1.0, uv.y));

                col = lerp(col, _EdgeFadeColour.rgb, edgeFadeMask);

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
