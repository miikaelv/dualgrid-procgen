Shader "DualGrid/DualGridBuilderShader"
{
    Properties
    {
        [NoScaleOffset] [MainTexture] _BaseMap ("Sprite Sheet (4x4)", 2D) = "white" {}
        [HideInInspector] [NoScaleOffset] _DataMap ("Data Texture (Tile Indices)", 2D) = "white" {}
        [HideInInspector] _GridSize ("Grid Dimensions", Vector) = (10, 10, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "DualGridBuilder"="True"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct attributes
            {
                float4 position_os : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct varyings
            {
                float4 position_cs : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DataMap);
            SAMPLER(sampler_DataMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _GridSize;
            CBUFFER_END

            varyings vert(attributes input)
            {
                varyings output;
                output.position_cs = TransformObjectToHClip(input.position_os.xyz);
                output.uv = input.uv;
                return output;
            }
            
            half4 frag (varyings input) : SV_Target
            {
                // Sample DataMap red channel for raw tile index value
                half raw_index = _DataMap.Sample(sampler_DataMap, input.uv, 0).r;
                
                // Convert 0-1 range to the tile int index from byte
                half tile_index = raw_index * 254.999h;
                
                // Convert index to column/row
                half row = floor(tile_index * 0.25f);
                half col = tile_index - row * 4.0;

                // Calculate point to sample from inside tile bounds
                float2 tile_offset = float2(col, row) * 0.25;
                float2 local_uv = frac(input.uv * _GridSize.xy);
                float2 final_uv = tile_offset + local_uv * 0.25;

                return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, final_uv);
            }
            
            ENDHLSL
        }
    }
}