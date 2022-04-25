using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GameFoundation.Editor
{
    public class DataLayerEditor
    {
        [MenuItem("Game Foundation/Data Layer/Clear Save Files", false)]
        public static void RemoveAllFile()
        {
            if (EditorUtility.DisplayDialog("Delete", $"Are you sure you want to delete all save file?", "Yes", "No"))
            {
                var files = Directory.GetFiles(Application.persistentDataPath);
                foreach (var file in files)
                {
                    if (file.Contains(".dat"))
                    {
                        Debug.Log("Delete " + file);
                        File.Delete(file);
                    }
                }
            }
        }

        [MenuItem("Game Foundation/Data Layer/Log Save Path", false)]
        public static void LogSavePath()
        {
            Debug.Log(Application.persistentDataPath);
        }
    }
}
