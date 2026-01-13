using UnityEditor;
using UnityEngine;

namespace DualGrid.Editor
{
    public static class DualGridCreator
    {
        private const string MenuPath = "GameObject/DualGrid/DualGrid";
        private const string PrefabPath = "Packages/com.miikaelv.dualgrid-procgen/Core/DualGridPrefab.prefab";

        [MenuItem(MenuPath, false, 10)]
        private static void Create(MenuCommand menuCommand)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[dualgrid-procgen] Could not find prefab at: {PrefabPath}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    
            instance.name = "DualGrid";

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

            Selection.activeObject = instance;
        }
    }
}