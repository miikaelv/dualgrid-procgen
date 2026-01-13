using System.Diagnostics;
using DualGrid.Core.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DualGrid.Core
{
    public class DualGridMap : MonoBehaviour
    {
        private static readonly TileRuleLookup TileRuleLookup = new();
        [SerializeField] private DualGridRenderer GridRenderer;

        public void DrawRenderMap(Material tilemapMaterial, bool[,] dataMap)
        {
            var stopwatch = Stopwatch.StartNew();

            var dataWidth = dataMap.GetLength(0);
            var dataHeight = dataMap.GetLength(1);
            var renderWidth = dataWidth + 1;
            var renderHeight = dataHeight + 1;

            var tileIndices = new int[renderWidth * renderHeight];
            var currentIndex = 0;

            // Loop through the entire RenderMap and assign tileIndex based on neighbouring data tiles
            // Height first to ease conversion to 1D Array
            for (var y = 0; y < renderHeight; y++)
            {
                for (var x = 0; x < renderWidth; x++)
                {
                    var topLeftNeighbour = dataMap.HasPosition(x - 1, y, dataWidth, dataHeight);
                    var topRightNeighbour = dataMap.HasPosition(x, y, dataWidth, dataHeight);
                    var bottomLeftNeighbour = dataMap.HasPosition(x - 1, y - 1, dataWidth, dataHeight);
                    var bottomRightNeighbour = dataMap.HasPosition(x, y - 1, dataWidth, dataHeight);

                    if (!TileRuleLookup.TryGetTileIndexByRules(topLeftNeighbour, topRightNeighbour, bottomLeftNeighbour,
                            bottomRightNeighbour, out var tileIndex))
                    {
                        Debug.LogError(
                            $"No rule match for {topLeftNeighbour} : {topRightNeighbour} : {bottomLeftNeighbour} : {bottomRightNeighbour}");

                        return;
                    }

                    tileIndices[currentIndex] = tileIndex;
                    currentIndex++;
                }
            }

            GridRenderer.DrawMapToTexture(tilemapMaterial, tileIndices, renderWidth, renderHeight);

            stopwatch.Stop();
            Debug.Log($"DualGrid tilemap created in {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
        }
        
        public void ClearGrid() => GridRenderer.ClearGrid();
    }
}