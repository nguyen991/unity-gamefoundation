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
        private List<BuildTargetGroup> targetGroup = new List<BuildTargetGroup>()
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.WebGL,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
        };

        private bool featureFoldout = true;

        [MenuItem("Game Foundation/Setting", false, 0)]
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
            EditorGUILayout.BeginVertical("GroupBox");
            DrawFeature();
            EditorGUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(setting);
                AssetDatabase.SaveAssetIfDirty(setting);
            }
        }
        private void DrawFeature()
        {
            featureFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(featureFoldout, "Features");
            if (featureFoldout)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.LabelField("Advertise", EditorStyles.boldLabel);

                var value = EditorGUILayout.Toggle("Enable Admob", setting.enableAds);

                if (value != setting.enableAds)
                {
                    if (value)
                    {
                        AddSymbol("GF_ADS", targetGroup);
                    }
                    else
                    {
                        RemoveSymbol("GF_ADS", targetGroup);
                    }
                    setting.enableAds = value;
                }

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

                EditorGUILayout.LabelField("Analytics", EditorStyles.boldLabel);
                value = EditorGUILayout.Toggle("Enable Firebase", setting.enableFirebase);
                if (value != setting.enableFirebase)
                {
                    if (value)
                    {
                        AddSymbol("GF_FIREBASE", targetGroup);
                    }
                    else
                    {
                        RemoveSymbol("GF_FIREBASE", targetGroup);
                    }
                    setting.enableFirebase = value;
                }

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

                EditorGUILayout.LabelField("In-App Purchasing", EditorStyles.boldLabel);
                value = EditorGUILayout.Toggle("Enable IAP", setting.enableIap);
                if (value != setting.enableIap)
                {
                    if (value)
                    {
                        AddSymbol("GF_IAP", targetGroup);
                    }
                    else
                    {
                        RemoveSymbol("GF_IAP", targetGroup);
                    }
                    setting.enableIap = value;
                }

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

                EditorGUILayout.LabelField("Dotween", EditorStyles.boldLabel);
                value = EditorGUILayout.Toggle("Enable Dotween", setting.enableDotween);
                if (value != setting.enableDotween)
                {
                    if (value)
                    {
                        AddSymbol("UNITASK_DOTWEEN_SUPPORT", targetGroup);
                    }
                    else
                    {
                        RemoveSymbol("UNITASK_DOTWEEN_SUPPORT", targetGroup);
                    }
                    setting.enableDotween = value;
                }

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                if (GUILayout.Button("Apply Change"))
                {
                    CompilationPipeline.RequestScriptCompilation();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndFoldoutHeaderGroup();
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
