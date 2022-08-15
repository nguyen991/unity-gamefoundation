using UnityEngine;
using UnityEditor;
using GameFoundation.State;
using System.Collections.Generic;

namespace GameFoundation.Editor
{
    public static class GameObjectPath
    {
        [MenuItem("GameObject/Print Hierarchy Path", false, -1)]
        static void PrintPath()
        {
            var selected = Selection.activeTransform;
            string path = selected.name;
            Transform parent = selected.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            Debug.Log(path);
        }

        [MenuItem("GameObject/Print Hierarchy Path", true)]
        static bool ValidatePrintPath()
        {
            return Selection.activeTransform != null && Selection.transforms.Length == 1;
        }
    }
}