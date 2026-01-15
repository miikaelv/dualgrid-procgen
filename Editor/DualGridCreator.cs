using DualGrid.Core;
using UnityEditor;
using UnityEngine;

namespace DualGrid.Editor
{
    public static class DualGridCreator
    {
        private const string MenuPath = "GameObject/DualGrid/DualGrid";
        private const string PackagePath = "Packages/com.miikaelv.dualgrid-procgen/";
        private static readonly string PrefabPath = $"{PackagePath}Core/Assets/DualGridPrefab.prefab";
        private static readonly string
            ComputeShaderPath = $"{PackagePath}Core/Assets/DualGridRuleComputeShader.compute";

        [MenuItem(MenuPath, false, 10)]
        private static void CreateDualGrid(MenuCommand menuCommand)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[dualgrid-procgen] Could not find prefab at: {PrefabPath}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(instance, "Create DualGrid Object");

            if (PrefabUtility.IsPartOfPrefabInstance(instance))
            {
                PrefabUtility.UnpackPrefabInstance(
                    instance,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction
                );
            }
            
            instance.name = "DualGrid";
            instance.GetComponent<DualGridMap>().DualGridComputeShader =
                AssetDatabase.LoadAssetAtPath<ComputeShader>(ComputeShaderPath);

            Selection.activeObject = instance;
        }
    }
}