using UnityEngine;

namespace DualGrid.Core
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class DualGridRenderer : MonoBehaviour
    {
        [SerializeField] private MeshRenderer MeshRenderer;
        [SerializeField] private MeshFilter MeshFilter;

        private static readonly int DataMapProperty = Shader.PropertyToID("_DataMap");
        private static readonly int GridSizeProperty = Shader.PropertyToID("_GridSize");

        public void ClearGrid()
        {
            if (MeshRenderer.material != null)
            {
                MeshRenderer.material.SetTexture(DataMapProperty, null);
            }
        }

        public void DrawMapToTexture(Material dualGridMaterial, int[] tileIndices, int width, int height)
        {
            if (dualGridMaterial == null || !dualGridMaterial.HasProperty(DataMapProperty))
            {
                Debug.LogError(
                    $"Passed in invalid material {dualGridMaterial.name} to ${nameof(DualGridRenderer)}. Use shader DualGridBuilderShader in the material.");

                return;
            }

            // Set grid scale (tile size 1), and dual grid offset to parent
            transform.localScale = new Vector3(width, height, 1f);
            transform.localPosition = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, 0);

            var dataTexture = CreateDataTexture(tileIndices, width, height);
            MeshRenderer.material = dualGridMaterial;
            MeshRenderer.material.SetTexture(DataMapProperty, dataTexture);
            MeshRenderer.material.SetVector(GridSizeProperty, new Vector3(width, height));
        }

        /// <summary>
        /// Creates a Texture2D from an array of tile indices. 
        /// Each tile index is stored as a single byte in a red channel texture.
        /// </summary>
        private static Texture2D CreateDataTexture(int[] tileIndices, int width, int height)
        {
            // TextureFormat.R8: single-channel 8-bit texture (red channel only)
            // mipChain: false (no mipmaps)
            // linear: true (use linear color space)
            var tex = new Texture2D(width, height, TextureFormat.R8, false, true)
            {
                // Set the filter mode to Point (no smoothing) since this is data, not a visual texture
                filterMode = FilterMode.Point,

                // Clamp texture coordinates outside [0,1] to the edge pixel
                wrapMode = TextureWrapMode.Clamp
            };

            // Set the data directly into the texture as bytes
            var data = tex.GetRawTextureData<byte>();
            for (var i = 0; i < tileIndices.Length; i++)
            {
                data[i] = (byte)tileIndices[i];
            }

            tex.Apply();
            return tex;
        }
    }
}