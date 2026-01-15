# DualGrid for Unity (URP)

**DualGrid** is a high-performance, GPU-accelerated tiling system for Unity’s Universal Render Pipeline. It implements
the Dual Grid technique to automate tile transitions, allowing you to create organic-looking maps using a simple boolean
data grid (e.g. solid vs. empty) and a single 16-tile sprite sheet. Made with run-time performance and procedural
generation in mind.

## Features

* **GPU-Powered:** Renders the entire map on a single quad using a custom HLSL shader.
* **Compute Shader Support:** Offload tile rule calculation to the GPU for massive maps.
* **Zero-Boilerplate Autotiling:** Only requires a 16-tile sprite sheet to handle all corner transitions.
* **Editor Integration:** Quick-create system via the `GameObject` menu.
* **Sample Included:** Includes optional sample scene in Samples with noise map generation to get you started quickly.

## Installation

### Via Unity Package Manager (Git)

1. Open the **Package Manager** (`Window > Package Manager`).
2. Click the **+** icon and select **Add package from git URL...**.
3. Paste the following:
   `https://github.com/miikaelv/dualgrid-procgen.git`

## Getting Started

### Importing the SampleScene

To quickly try out the package, you can import the provided `SampleScene`:

1. Open **Window → Package Manager** in Unity.
2. Select this package from the list.
3. Click the **Samples** button in the package details.
4. Click **Import** next to `Map Generation Sample`.

### How to use

### 1. Create the DualGrid Object

Right-click in the **Hierarchy** or use the top menu:  
`GameObject > DualGrid > DualGrid`

This instantiates the `DualGridPrefab`, which comes pre-configured with the `DualGridRenderer` and the necessary mesh
components.

### 2. Configure Your Material

1. Create a new **Material** and set the shader to `DualGrid/DualGridBuilderShader`.
2. Assign your **4x4 Sprite Sheet** to the `Base Map` slot.
3. Ensure your Sprite Sheet import settings are:
    * **Filter Mode:** Point (no filter)
    * **Compression:** None (or High Quality)
    * **Wrap Mode:** Clamp

### 3. Usage via Code

Pass a 2D boolean array (your data map) to the `DualGridMap` component:

```csharp
using DualGrid.Core;
using DualGrid.Core.Utility;
using UnityEngine;

public class MyLevelGenerator : MonoBehaviour
{
    [SerializeField] private DualGridMap dualGrid;
    [SerializeField] private Material dualGridMaterial;
    [SerializeField] private ComputeShader NoiseShader;

    void GenerateGrid()
    {
        // 1000x1000 logical grid
        bool[,] map = new bool[1000, 1000];
        
        // Fill map with logic (true = solid, false = empty)
        // ...
        
        dualGrid.DrawRenderMap(dualGridMaterial, map);
    }
    
    // For faster results, use a 1D int array, where all values are set as either 1 or 0
    void GenerateGridOptimized()
    {
        var width = 1000;
        var height = 1000;
        
        int[] map = new int[width * height];
        
        // Fill map with logic (1 = solid, 0 = empty)
        // ...
        
        // Wrap the map into the optional DataMap class for some helper tools.
        var dataMap = new DataMap(map, width, height);
        
        // Use either DrawRenderMap for CPU or DrawRenderMapWithComputeShader for GPU performance.
        dualGrid.DrawRenderMap(dualGridMaterial, dataMap, width, height);
        dualGrid.DrawMapWithComputeShader(dualGridMaterial, dataMap, width, height);
    }
    
    // For the fastest results, use a compute shader to generate the dataMap and pass it in to render.
    // See NoiseComputeShader.compute in Map Generation Sample for specifics.
    void GenerateGridWithShaderComputedNoise()
    {
        var width = 1000;
        var height = 1000;
        
        var buffer = new ComputeBuffer(width * height, sizeof(int));
        var kernel = NoiseShader.FindKernel("CSMain");
        NoiseShader.SetBuffer(kernel, "Result", buffer);
        NoiseShader.SetInt("Width", 1000);
        NoiseShader.SetInt("Height", 1000);
        NoiseShader.SetFloat("Scale", 0.05f);
        NoiseShader.SetFloat("Threshold", 0.5f);

        NoiseShader.Dispatch(kernel, Mathf.CeilToInt(width / 8.0f),
                Mathf.CeilToInt(height / 8.0f), 1);
        
        dualGrid.DrawMapWithComputeShader(dualGridMaterial, buffer, width, height);
    }
}