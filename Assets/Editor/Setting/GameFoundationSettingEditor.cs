using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameFoundation;
using UnityEditor.Compilation;

namespace GameFoundation.Editor
{
    public class GameFoundationSettingEditor : EditorWindow
    {
        private GameFoundationSetting setting = null;
        private List<BuildTargetGroup> targetGroups = new List<BuildTargetGroup>()
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
        };

        [MenuItem("Game Foundation/Setting")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(GameFoundationSettingEditor));
            window.titleContent = new GUIContent("Game Foundation Setting");
            window.Show();
        }

        private void OnGUI()
        {
            // find setting asset
            if (setting == null)
            {
                // search in resource folder
                var guids = AssetDatabase.FindAssets("t:GameFoundationSetting");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    setting = AssetDatabase.LoadAssetAtPath<GameFoundationSetting>(path);
                }
                else
                {
                    // create new asset
                    setting = ScriptableObject.CreateInstance<GameFoundationSetting>();
                    AssetDatabase.CreateAsset(setting, "Assets/Resources/GameFoundationSetting.asset");
                    AssetDatabase.SaveAssets();
                    Debug.Log("Create GameFoundationSetting asset in Resources folder.");
                }
            }

            // draw setting
            if (setting != null)
            {
                Draw();
            }
        }

        private void Draw()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Advertise", EditorStyles.boldLabel);

            var modified = false;
            var value = EditorGUILayout.Toggle("Enable Admob", setting.enableAds);

            if (value != setting.enableAds)
            {
                if (value)
                {
                    AddSymbol("GF_ADS", targetGroups);
                }
                else
                {
                    RemoveSymbol("GF_ADS", targetGroups);
                }
                modified = true;
                setting.enableAds = value;
            }

            EditorGUILayout.Space(10f);

            EditorGUILayout.LabelField("Firebase Analytics", EditorStyles.boldLabel);
            value = EditorGUILayout.Toggle("Enable Analytics", setting.enableAnalytics);
            if (value != setting.enableAnalytics)
            {
                if (value)
                {
                    AddSymbol("GF_ANALYTICS", targetGroups);
                }
                else
                {
                    RemoveSymbol("GF_ANALYTICS", targetGroups);
                }
                modified = true;
                setting.enableAnalytics = value;
            }

            EditorGUILayout.Space(10f);

            EditorGUILayout.LabelField("In-App Purchasing", EditorStyles.boldLabel);
            value = EditorGUILayout.Toggle("Enable IAP", setting.enableIap);
            if (value != setting.enableIap)
            {
                if (value)
                {
                    AddSymbol("GF_IAP", targetGroups);
                }
                else
                {
                    RemoveSymbol("GF_IAP", targetGroups);
                }
                modified = true;
                setting.enableIap = value;
            }

            EditorGUILayout.Space(10f);

            EditorGUILayout.LabelField("Dotween", EditorStyles.boldLabel);
            value = EditorGUILayout.Toggle("Enable Dotween", setting.enableDotween);
            if (value != setting.enableDotween)
            {
                if (value)
                {
                    AddSymbol("UNITASK_DOTWEEN_SUPPORT ", targetGroups);
                }
                else
                {
                    RemoveSymbol("UNITASK_DOTWEEN_SUPPORT ", targetGroups);
                }
                modified = true;
                setting.enableDotween = value;
            }

            EditorGUILayout.Space(15f);
            if (GUILayout.Button("Apply Change"))
            {
                CompilationPipeline.RequestScriptCompilation();
            }

            if (modified)
            {
                EditorUtility.SetDirty(setting);
                AssetDatabase.SaveAssetIfDirty(setting);
            }

            EditorGUILayout.EndVertical();
        }

        private void AddSymbol(string symbol, List<BuildTargetGroup> groups)
        {
            groups.ForEach(target =>
            {
                if (!PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Contains(symbol))
                {
                    Debug.Log("Add define " + symbol + " to " + target);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, PlayerSettings.GetScriptingDefineSymbolsForGroup(target) + ";" + symbol);
                }
            });
        }

        private void RemoveSymbol(string symbol, List<BuildTargetGroup> groups)
        {
            groups.ForEach(target =>
            {
                if (PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Contains(symbol))
                {
                    Debug.Log("Remove define " + symbol + " from " + target);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Replace(symbol, ""));
                }
            });
        }
    }
}
