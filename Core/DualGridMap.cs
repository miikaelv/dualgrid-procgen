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
            var dataStopwatch = Stopwatch.StartNew();

            var dataWidth = (uint)dataMap.GetLength(0);
            var dataHeight = (uint)dataMap.GetLength(1);
            var renderWidth = dataWidth + 1;
            var renderHeight = dataHeight + 1;
            var tileIndices = new int[renderWidth * renderHeight];

            // Loop through the entire RenderMap and assign tileIndex based on neighbouring data tiles
            // Height first to ease conversion to 1D Array
            for (uint y = 0; y < renderHeight; y++)
            {
                // Pre-calculate needed variables for Y
                var yMinus1 = y - 1;
                var yOffsetValid = yMinus1 < dataHeight;
                var yValid = y < dataHeight;
                var yArrayOffset = y * renderWidth;
                
                for (uint x = 0; x < renderWidth; x++)
                {
                    var xMinus1 = x - 1;
                    var xOffsetValid = xMinus1 < dataWidth;
                    var xValid = x < dataWidth;

                    // Check tile dataMap neighbour bounds and get value from dataMap.
                    // Uses ternary operators and non-short-circuit AND to reduce conditional branching.
                    // uint to both check negative and upperBounds in one check
                    var topLeft = xOffsetValid & yValid ? dataMap[xMinus1, y] ? 1 : 0 : 0;
                    var topRight = xValid & yValid ? dataMap[x, y] ? 1 : 0 : 0;
                    var bottomLeft = xOffsetValid & yOffsetValid ? dataMap[xMinus1, yMinus1] ? 1 : 0 : 0;
                    var bottomRight = xValid & yOffsetValid ? dataMap[x, yMinus1] ? 1 : 0 : 0;

                    // Bitwise OR operation to add bits to the bitmask, each 1 or 0 value shifted left to their correct position.
                    var bitmask = topLeft | (topRight << 1) | (bottomLeft << 2) | (bottomRight << 3);

                    tileIndices[yArrayOffset + x] = TileRuleLookup.GetRenderTileIndexNoBoundsCheck(bitmask);
                }
            }

            dataStopwatch.Stop();
            var renderStopwatch = Stopwatch.StartNew();

            GridRenderer.DrawMapToTexture(tilemapMaterial, tileIndices, (int)renderWidth, (int)renderHeight);

            renderStopwatch.Stop();

            Debug.Log(
                $"DualGrid tilemap {dataWidth}x{dataHeight} auto-tiling data created in {dataStopwatch.Elapsed.TotalMilliseconds:F2}ms");
            Debug.Log(
                $"DualGrid tilemap {dataWidth}x{dataHeight} rendered in {renderStopwatch.Elapsed.TotalMilliseconds:F2}ms");
        }

        public void ClearGrid() => GridRenderer.ClearGrid();
    }
}