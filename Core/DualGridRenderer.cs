using DualGrid.Core.Utility;
using UnityEngine;

namespace DualGrid.Core
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class DualGridRenderer : MonoBehaviour
    {
        [SerializeField] private MeshRenderer MeshRenderer;
        [SerializeField] private MeshFilter MeshFilter;

        public bool SetGridScaleAutomatically = true;
        public bool SetGridPositionAutomatically = true;

        private static readonly int DataMapProperty = Shader.PropertyToID("_DataMap");
        private static readonly int GridSizeProperty = Shader.PropertyToID("_GridSize");

        private RenderTexture TileIndicesTexture;

        /// <summary>
        /// Sets the DataMap of the shader to null, so all tiles are drawn as false (empty).
        /// </summary>
        public void ClearGrid()
        {
            if (TileIndicesTexture != null)
                TileIndicesTexture.Release();
            
            if (MeshRenderer.material != null)
            {
                MeshRenderer.material.SetTexture(DataMapProperty, null);
            }
        }

        public void DrawMapToTexture(Material dualGridMaterial, RenderTexture tileIndicesTexture, int width, int height)
        {
            ClearGrid();
            
            TileIndicesTexture = tileIndicesTexture;
            // Set grid scale to match grid size, results in tile size 1
            if (SetGridScaleAutomatically)
                transform.localScale = new Vector3(width, height, 1f);
            
            // Set grid "pivot" to parent at bottom left corner and offset by -0.5f to match with data grid 
            if (SetGridPositionAutomatically)
                transform.localPosition = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, 0);

            // Draw to material texture
            MeshRenderer.material = dualGridMaterial;
            MeshRenderer.material.SetTexture(DataMapProperty, tileIndicesTexture);
            MeshRenderer.material.SetVector(GridSizeProperty, new Vector3(width, height));
        }
        
        public void DrawMapToTexture(Material dualGridMaterial, int[] tileIndices, int width, int height)
        {
            ClearGrid();
            
            if (dualGridMaterial == null || !dualGridMaterial.HasProperty(DataMapProperty))
            {
                Debug.LogError(
                    $"Passed in invalid material {dualGridMaterial.name} to ${nameof(DualGridRenderer)}. Use shader DualGridBuilderShader in the material.");

                return;
            }
            
            if (TileIndicesTexture != null)
                TileIndicesTexture.Release();

            // Set grid scale to match grid size, results in tile size 1
            if (SetGridScaleAutomatically)
                transform.localScale = new Vector3(width, height, 1f);
            
            // Set grid "pivot" to parent at bottom left corner and offset by -0.5f to match with data grid 
            if (SetGridPositionAutomatically)
                transform.localPosition = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, 0);

            // Draw to material texture
            var dataTexture = ShaderUtility.CreateDataTexture(tileIndices, width, height);
            MeshRenderer.material = dualGridMaterial;
            MeshRenderer.material.SetTexture(DataMapProperty, dataTexture);
            MeshRenderer.material.SetVector(GridSizeProperty, new Vector3(width, height));
        }
    }
}