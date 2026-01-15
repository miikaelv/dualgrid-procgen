using System.Diagnostics;
using DualGrid.Core.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DualGrid.Core
{
    public class DualGridMap : MonoBehaviour
    {
        public bool LogPerformance = true;

        private static readonly TileRuleLookup TileRuleLookup = new();
        [SerializeField] private DualGridRenderer GridRenderer;
        public ComputeShader DualGridComputeShader;

        /// <summary>
        /// Safest, but slowest. bool ensures all values are valid, 2DArray easier to handle outside rendering logic.
        /// </summary>
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

            if (!LogPerformance) return;

            Debug.Log(
                $"DualGrid tilemap {dataWidth}x{dataHeight} auto-tiling data created in {dataStopwatch.Elapsed.TotalMilliseconds:F2}ms");
            Debug.Log(
                $"DualGrid tilemap {dataWidth}x{dataHeight} rendered in {renderStopwatch.Elapsed.TotalMilliseconds:F2}ms");
        }

        /// <summary>
        /// Faster, but still CPU bottlenecked. Must make sure all values in DataMap are either 1 or 0.
        /// </summary>
        public void DrawRenderMap(Material tilemapMaterial, DataMap dataMap, int dataWidth, int dataHeight)
        {
            var dataStopwatch = Stopwatch.StartNew();

            var renderWidth = dataWidth + 1;
            var renderHeight = dataHeight + 1;
            var tileIndices = new int[renderWidth * renderHeight];

            // Loop through the entire RenderMap and assign tileIndex based on neighbouring data tiles
            var renderIndex = 0;
            for (uint y = 0; y < renderHeight; y++)
            {
                // Pre-calculate needed variables for Y
                var yMinus1 = y - 1;
                var yOffsetValid = yMinus1 < dataHeight;
                var yValid = y < dataHeight;
                var yArrayOffset = y * dataWidth;
                var yMinus1ArrayOffset = yMinus1 * dataWidth;

                for (uint x = 0; x < renderWidth; x++)
                {
                    var xMinus1 = x - 1;
                    var xOffsetValid = xMinus1 < dataWidth;
                    var xValid = x < dataWidth;

                    // Check tile dataMap neighbour bounds and get value from dataMap.
                    // Uses ternary operators and non-short-circuit AND to reduce conditional branching.
                    // uint to both check negative and upperBounds in one check
                    var topLeft = xOffsetValid & yValid ? dataMap.InternalArray[yArrayOffset + xMinus1] : 0;
                    var topRight = xValid & yValid ? dataMap.InternalArray[yArrayOffset + x] : 0;
                    var bottomLeft = xOffsetValid & yOffsetValid
                        ? dataMap.InternalArray[yMinus1ArrayOffset + xMinus1]
                        : 0;
                    var bottomRight = xValid & yOffsetValid ? dataMap.InternalArray[yMinus1ArrayOffset + x] : 0;

                    // Bitwise OR operation to add bits to the bitmask, each 1 or 0 value shifted left to their correct position.
                    var bitmask = topLeft | (topRight << 1) | (bottomLeft << 2) | (bottomRight << 3);

                    tileIndices[renderIndex++] = TileRuleLookup.GetRenderTileIndexNoBoundsCheck(bitmask);
                }
            }

            dataStopwatch.Stop();
            var renderStopwatch = Stopwatch.StartNew();

            GridRenderer.DrawMapToTexture(tilemapMaterial, tileIndices, renderWidth, renderHeight);

            renderStopwatch.Stop();

            if (!LogPerformance) return;

            Debug.Log(
                $"DualGrid tilemap {dataWidth}x{dataHeight} auto-tiling data created in {dataStopwatch.Elapsed.TotalMilliseconds:F2}ms");
            Debug.Log(
                $"DualGrid tilemap {dataWidth}x{dataHeight} rendered in {renderStopwatch.Elapsed.TotalMilliseconds:F2}ms");
        }

        private static readonly int DataBufferProperty = Shader.PropertyToID("DataBuffer");
        private static readonly int TileRuleLookupProperty = Shader.PropertyToID("TileRuleLookup");
        private static readonly int TileIndicesProperty = Shader.PropertyToID("TileIndices");
        private static readonly int DataWidthProperty = Shader.PropertyToID("DataWidth");
        private static readonly int DataHeightProperty = Shader.PropertyToID("DataHeight");
        private static readonly int RenderWidthProperty = Shader.PropertyToID("RenderWidth");
        private static readonly int RenderHeightProperty = Shader.PropertyToID("RenderHeight");

        /// <summary>
        /// Fast. Must make sure all values in DataMap are either 1 or 0.
        /// </summary>
        public void DrawMapWithComputeShader(Material tilemapMaterial, DataMap dataMap, int dataWidth, int dataHeight)
        {
            var dataBuffer = ShaderUtility.CreateStructuredBuffer(dataMap);
            DrawMapWithComputeShader(tilemapMaterial, dataBuffer, dataWidth, dataHeight);
        }

        /// <summary>
        /// Fastest, use a compute shader to generate your dataMap directly into a dataBuffer and pass it here to render.
        /// Trade off is that getting the dataMap out of the dataBuffer is slow.
        /// </summary>
        public void DrawMapWithComputeShader(Material tilemapMaterial, ComputeBuffer dataBuffer, int dataWidth,
            int dataHeight)
        {
            if (DualGridComputeShader == null)
            {
                Debug.LogError($"{nameof(DualGridComputeShader)} is not assigned. Releasing dataBuffer...");
                dataBuffer.Release();
                return;
            }

            var dataStopwatch = Stopwatch.StartNew();

            var renderWidth = dataWidth + 1;
            var renderHeight = dataHeight + 1;

            var ruleLookupDataBuffer = ShaderUtility.CreateStructuredBuffer(TileRuleLookup.TileIndicesByBitmask);

            var tileIndexTexture = new RenderTexture(renderWidth, renderHeight, 0, RenderTextureFormat.RFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                useMipMap = false,
                autoGenerateMips = false,
            };
            tileIndexTexture.Create();

            var kernel = DualGridComputeShader.FindKernel("CSMain");
            DualGridComputeShader.SetBuffer(kernel, DataBufferProperty, dataBuffer);
            DualGridComputeShader.SetBuffer(kernel, TileRuleLookupProperty, ruleLookupDataBuffer);
            DualGridComputeShader.SetTexture(kernel, TileIndicesProperty, tileIndexTexture);

            DualGridComputeShader.SetInt(DataWidthProperty, dataWidth);
            DualGridComputeShader.SetInt(DataHeightProperty, dataHeight);
            DualGridComputeShader.SetInt(RenderWidthProperty, renderWidth);
            DualGridComputeShader.SetInt(RenderHeightProperty, renderHeight);

            DualGridComputeShader.Dispatch(kernel, Mathf.CeilToInt(dataWidth / 8.0f),
                Mathf.CeilToInt(dataHeight / 8.0f), 1);

            dataBuffer.Release();
            ruleLookupDataBuffer.Release();

            dataStopwatch.Stop();
            var renderStopwatch = Stopwatch.StartNew();

            GridRenderer.DrawMapToTexture(tilemapMaterial, tileIndexTexture, renderWidth, renderHeight);

            renderStopwatch.Stop();

            if (!LogPerformance) return;

            Debug.Log(
                $"DualGrid tilemap {dataWidth}x{dataHeight} auto-tiling data created in {dataStopwatch.Elapsed.TotalMilliseconds:F2}ms");
            Debug.Log(
                $"DualGrid tilemap {dataWidth}x{dataHeight} rendered in {renderStopwatch.Elapsed.TotalMilliseconds:F2}ms");
        }

        public void ClearGrid() => GridRenderer.ClearGrid();
    }
}