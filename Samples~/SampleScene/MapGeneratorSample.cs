using DualGrid.Core;
using UnityEngine;

namespace DualGrid.Samples.SampleScene
{
    public class MapGeneratorSample : MonoBehaviour
    {
        [SerializeField] private Material TilemapMaterial;
        [SerializeField] private DualGridMap Map;
        [SerializeField] private DualGridRenderer GridRenderer;
        [SerializeField] private Camera MainCamera;

        [Header("Settings")] [SerializeField] private int MapWidth;
        [SerializeField] private int MapHeight;
        [SerializeField, Range(0f, 1f)] private float LandGenerationThreshold;
        [SerializeField, Range(0.005f, 0.05f)] private float Scale;
        
        public void GenerateMap()
        {
            if (Map == null || TilemapMaterial == null)
            {
                Debug.LogError($"Missing references in ${nameof(MapGeneratorSample)}");

                return;
            }

            var dataMap = GenerateDataMap(MapWidth, MapHeight, Scale, LandGenerationThreshold);
            Map.DrawRenderMap(TilemapMaterial, dataMap);

            // Set camera to look at the whole generated map
            MainCamera.orthographic = true;
            MainCamera.orthographicSize = MapHeight / 2f;
            MainCamera.transform.position =
                new Vector3(GridRenderer.transform.position.x, GridRenderer.transform.position.y, -10f);
            MainCamera.transform.LookAt(GridRenderer.transform);
        }

        private static bool[,] GenerateDataMap(int width, int height, float scale, float threshold)
        {
            var map = new bool[width, height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var sampleX = x * scale;
                    var sampleY = y * scale;

                    var noiseValue = Mathf.PerlinNoise(sampleX, sampleY);

                    map[x, y] = noiseValue > threshold;
                }
            }

            return map;
        }
    }
}