Shader "DualGrid/DualGridBuilderShader"
{
    Properties
    {
        [MainTexture] _BaseMap ("Sprite Sheet (4x4)", 2D) = "white" {}
        _DataMap ("Data Texture (Tile Indexes)", 2D) = "white" {}
        _GridSize ("Grid Dimensions", Vector) = (10, 10, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

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
                float4 _BaseMap_ST;
                float4 _GridSize;
            CBUFFER_END

            varyings vert(attributes input)
            {
                varyings output;
                output.position_cs = TransformObjectToHClip(input.position_os.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }
            
            half4 frag (varyings input) : SV_Target
            {
                // Use Point Sampling to get red channel value
                float raw_index = SAMPLE_TEXTURE2D_LOD(_DataMap, sampler_DataMap, input.uv, 0).r;
                
                // Convert 0-1 range to the tile int index
                float tile_index = round(raw_index * 255.0);
                
                // Convert input-uv to "tile-space"
                float2 local_uv = frac(input.uv * _GridSize.xy);

                // Convert tile index to 4x4 grid (0 is bottom-left)
                float col = fmod(tile_index, 4.0);
                float row = floor(tile_index / 4.0);
                
                // Each tile is 0.25 (1/4) of the sheet
                float2 tile_offset = float2(col, row) * 0.25;
                float2 final_uv = tile_offset + local_uv * 0.25;

                return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, final_uv);
            }
            
            ENDHLSL
        }
    }
}