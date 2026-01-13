# DualGrid for Unity (URP)

**DualGrid** is a high-performance, GPU-accelerated tiling system for Unity’s Universal Render Pipeline. It implements the Dual Grid technique to automate tile transitions, allowing you to create organic-looking maps using a simple boolean data grid (e.g. solid vs. empty) and a single 16-tile sprite sheet.



## Features

* **GPU-Powered:** Renders the entire map on a single quad using a custom HLSL shader.
* **Zero-Boilerplate Autotiling:** Only requires a 16-tile sprite sheet to handle all corner transitions.
* **Editor Integration:** Quick-create system via the `GameObject` menu.
* **Optimized Data Flow:** Uses an `R8` texture format to pass tile indices to the GPU, ensuring a tiny memory footprint and high performance for dynamic updates.

---

## Installation

### Via Unity Package Manager (Git)
1. Open the **Package Manager** (`Window > Package Manager`).
2. Click the **+** icon and select **Add package from git URL...**.
3. Paste the following:
   `https://github.com/yourusername/dualgrid-unityshader.git`

---

## Getting Started

### 1. Create the DualGrid Object
Right-click in the **Hierarchy** or use the top menu:  
`GameObject > DualGrid > DualGrid`

This instantiates the `DualGridPrefab`, which comes pre-configured with the `DualGridRenderer` and the necessary mesh components.

### 2. Configure Your Material
1. Create a new **Material** and set the shader to `DualGrid/DualGridBuilderShader`.
2. Assign your **4x4 Sprite Sheet** to the `Base Map` slot.
3. Ensure your Sprite Sheet import settings are:
    * **Filter Mode:** Point (no filter)
    * **Compression:** None (or High Quality)
    * **Wrap Mode:** Clamp

### 3. Usage via Code
Pass a 2D boolean array (your logical map) to the `DualGridMap` component:

```csharp
using DualGrid.Core;
using UnityEngine;

public class MyLevelGenerator : MonoBehaviour
{
    [SerializeField] private DualGridMap dualGrid;
    [SerializeField] private Material dualGridMaterial;

    void Start()
    {
        // 50x50 logical grid
        bool[,] map = new bool[50, 50];
        
        // Fill map with logic (true = solid, false = empty)
        // ...
        
        dualGrid.DrawRenderMap(dualGridMaterial, map);
    }
}