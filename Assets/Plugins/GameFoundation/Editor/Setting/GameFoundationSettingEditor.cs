using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using GameFoundation;
using UnityEditor.Compilation;
using GameFoundation.Economy;
using static GameFoundation.Data.DataLayer;

namespace GameFoundation.Editor
{
    public class GameFoundationSettingEditor : EditorWindow
    {
        private GameFoundationSetting setting = null;
        private static SerializedObject serializedObject;
        private List<BuildTargetGroup> allPlatformGroup = new List<BuildTargetGroup>()
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.WebGL,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
        };
        private List<BuildTargetGroup> mobileGroup = new List<BuildTargetGroup>()
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
        };

        private bool featureFoldout = false;
        private Vector2 scrollPos = Vector2.zero;

        [MenuItem("Game Foundation/Setting", false, 0)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(GameFoundationSettingEditor));
            window.titleContent = new GUIContent("Game Foundation Setting");
            window.Show();
        }

        private void OnGUI()
        {
            serializedObject?.ApplyModifiedProperties();
            serializedObject?.Dispose();
            serializedObject = null;

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
                serializedObject = new SerializedObject(setting);
                serializedObject.Update();
                Draw();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(setting);
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void Draw()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginVertical("GroupBox");
            DrawFeature();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("GroupBox");
            DrawSettings();
            EditorGUILayout.EndVertical();

            GUILayout.EndScrollView();
        }
        private void DrawFeature()
        {
            featureFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(featureFoldout, "Features");
            if (featureFoldout)
            {
                EditorGUILayout.LabelField("Advertise", EditorStyles.boldLabel);

                var value = EditorGUILayout.Toggle("Enable Admob", setting.enableAds);

                if (value != setting.enableAds)
                {
                    if (value)
                    {
                        AddSymbol("GF_ADS", mobileGroup);
                    }
                    else
                    {
                        RemoveSymbol("GF_ADS", mobileGroup);
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
                        AddSymbol("GF_FIREBASE", mobileGroup);
                    }
                    else
                    {
                        RemoveSymbol("GF_FIREBASE", mobileGroup);
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
                        AddSymbol("GF_IAP", allPlatformGroup);
                    }
                    else
                    {
                        RemoveSymbol("GF_IAP", allPlatformGroup);
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
                        AddSymbol("UNITASK_DOTWEEN_SUPPORT", allPlatformGroup);
                    }
                    else
                    {
                        RemoveSymbol("UNITASK_DOTWEEN_SUPPORT", allPlatformGroup);
                    }
                    setting.enableDotween = value;
                }

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                if (GUILayout.Button("Apply Change"))
                {
                    CompilationPipeline.RequestScriptCompilation();
                }
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

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
            setting.startSceneIndex = EditorGUILayout.Popup("Master Scene", setting.startSceneIndex, EditorBuildSettings.scenes.Select(scene => scene.path).ToArray());

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Mobile Optimize", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resolution", GUILayout.Width(150));
            setting.designResolution = EditorGUILayout.Vector2IntField(GUIContent.none, setting.designResolution);
            EditorGUILayout.EndHorizontal();
            setting.fps = EditorGUILayout.IntField("FPS", setting.fps);
            setting.multiTouch = EditorGUILayout.Toggle("Multi Touch", setting.multiTouch);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Advertise", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("adConfig"), true);
            setting.useAdFakeOnEditor = EditorGUILayout.Toggle("Use Ad Fake On Editor", setting.useAdFakeOnEditor);
            setting.adFakeAvailable = EditorGUILayout.Toggle("Ad Fake Available", setting.adFakeAvailable);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Local Notification", EditorStyles.boldLabel);
            setting.autoScheduleNotification = EditorGUILayout.Toggle("Auto Schedule Notification", setting.autoScheduleNotification);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("notifications"), true);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Economy", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            setting.economyData = EditorGUILayout.ObjectField("Economy Data", setting.economyData, typeof(EconomyData), false) as EconomyData;
            if (setting.economyData == null && GUILayout.Button("Create", GUILayout.Width(60)))
            {
                EditorApplication.ExecuteMenuItem("Game Foundation/Economy");
                EditorCoroutineUtility.StartCoroutine(LoadEconomyData(), this);
            }
            else if (setting.economyData != null && GUILayout.Button("Edit", GUILayout.Width(60)))
            {
                EditorApplication.ExecuteMenuItem("Game Foundation/Economy");
            }
            EditorGUILayout.EndHorizontal();
            setting.dataLayerType = (DataLayerType)EditorGUILayout.EnumPopup("Data Layer", setting.dataLayerType);
            setting.saveOnLostFocus = EditorGUILayout.Toggle("Save On Lost Focus", setting.saveOnLostFocus);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Sprite Atlas", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteAtlasLabels"), true);
        }

        private IEnumerator LoadEconomyData()
        {
            yield return new EditorWaitForSeconds(0.1f);
            var guids = AssetDatabase.FindAssets("t:EconomyData");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                setting.economyData = AssetDatabase.LoadAssetAtPath<EconomyData>(path);
            }
            Repaint();
        }
    }
}
