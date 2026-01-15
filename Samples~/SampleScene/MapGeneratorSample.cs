using DualGrid.Core;
using DualGrid.Core.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DualGrid.Samples.SampleScene
{
    public class MapGeneratorSample : MonoBehaviour
    {
        public enum MapGeneratorMode
        {
            BoolMap,
            IntMap,
            ComputeShaderIntMap,
            PureComputeShader,
        }

        private static readonly int Width = Shader.PropertyToID("Width");
        private static readonly int Height = Shader.PropertyToID("Height");
        private static readonly int ScaleProperty = Shader.PropertyToID("Scale");
        private static readonly int Threshold = Shader.PropertyToID("Threshold");
        private static readonly int ResultProperty = Shader.PropertyToID("Result");

        [SerializeField] private Material TilemapMaterial;
        [SerializeField] private DualGridMap Map;
        [SerializeField] private DualGridRenderer GridRenderer;
        [SerializeField] private Camera MainCamera;
        [SerializeField] private ComputeShader NoiseShader;

        [Header("Settings")] [SerializeField] private MapGeneratorMode Mode = MapGeneratorMode.BoolMap;
        [SerializeField, Range(0, 16383)] private int MapWidth;
        [SerializeField, Range(0, 16383)] private int MapHeight;
        [SerializeField, Range(0f, 1f)] private float LandGenerationThreshold;
        [SerializeField, Range(0.005f, 0.05f)] private float Scale;

        public void GenerateMap()
        {
            if (Map == null || TilemapMaterial == null)
            {
                Debug.LogError($"Missing references in ${nameof(MapGeneratorSample)}");

                return;
            }

            switch (Mode)
            {
                case MapGeneratorMode.BoolMap:
                    var boolMap = GenerateDataMapBool(MapWidth, MapHeight, Scale, LandGenerationThreshold);
                    Map.DrawRenderMap(TilemapMaterial, boolMap);

                    break;
                case MapGeneratorMode.IntMap:
                    var dataMap = GenerateDataMap1D(MapWidth, MapHeight, Scale, LandGenerationThreshold);
                    Map.DrawRenderMap(TilemapMaterial, dataMap, MapWidth, MapHeight);

                    break;
                case MapGeneratorMode.ComputeShaderIntMap:
                    var dataMap2 = GenerateDataMap1D(MapWidth, MapHeight, Scale, LandGenerationThreshold);
                    Map.DrawMapWithComputeShader(TilemapMaterial, dataMap2, MapWidth, MapHeight);

                    break;
                case MapGeneratorMode.PureComputeShader:
                    var dataBuffer = CreateNoiseBuffer(MapWidth, MapHeight, Scale, LandGenerationThreshold);
                    Map.DrawMapWithComputeShader(TilemapMaterial, dataBuffer, MapWidth, MapHeight);

                    break;
            }

            // Set camera to look at the generated map
            MainCamera.orthographic = true;
            MainCamera.orthographicSize = MapHeight / 2f;
            MainCamera.transform.position =
                new Vector3(GridRenderer.transform.position.x, GridRenderer.transform.position.y, -10f);
            MainCamera.transform.LookAt(GridRenderer.transform);
        }

        private static DataMap GenerateDataMap1D(int width, int height, float scale, float landThreshold)
        {
            var map = new int[width * height];

            for (var y = 0; y < height; y++)
            {
                var yArrayOffset = y * width;

                for (var x = 0; x < width; x++)
                {
                    var sampleX = x * scale;
                    var sampleY = y * scale;

                    var noiseValue = Mathf.PerlinNoise(sampleX, sampleY);

                    map[yArrayOffset + x] = noiseValue > landThreshold ? 1 : 0;
                }
            }

            return new DataMap(map, width, height);
        }

        private static bool[,] GenerateDataMapBool(int width, int height, float scale, float landThreshold)
        {
            var map = new bool[width, height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var sampleX = x * scale;
                    var sampleY = y * scale;

                    var noiseValue = Mathf.PerlinNoise(sampleX, sampleY);

                    map[x, y] = noiseValue > landThreshold;
                }
            }

            return map;
        }

        private ComputeBuffer CreateNoiseBuffer(int width, int height, float scale, float threshold)
        {
            var buffer = new ComputeBuffer(MapWidth * MapHeight, sizeof(int));
            var kernel = NoiseShader.FindKernel("CSMain");
            NoiseShader.SetBuffer(kernel, ResultProperty, buffer);
            NoiseShader.SetInt(Width, width);
            NoiseShader.SetInt(Height, height);
            NoiseShader.SetFloat(ScaleProperty, scale);
            NoiseShader.SetFloat(Threshold, threshold);

            NoiseShader.Dispatch(kernel, Mathf.CeilToInt(width / 8.0f),
                Mathf.CeilToInt(height / 8.0f), 1);
            return buffer;
        }
    }
}