using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace GameFoundation.Editor
{
    public class StateScriptGenerator : EditorWindow
    {
        [MenuItem("Game Foundation/Generator/States Script", false, 100)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(StateScriptGenerator));
            window.titleContent = new GUIContent("State Script Generator");
            window.Show();
        }

        private string path = "Scripts";
        private string mainName = "Game";
        private bool subFolder = true;
        private string states = "";
        
        private class Options
        {
            public string replace = "";
            public string defaultName = "";
            public string customName = null;
            public bool available = true;
            public string suffix = "";
            public bool visible = true;
            public bool toggle = true;
            public string name => string.IsNullOrEmpty(customName) ? defaultName : customName;
        }
        private Dictionary<string, Options> options = new Dictionary<string, Options>()
        {
            { "model", new Options() { replace =  "__MODEL__", suffix = "Model" } },
            { "controller", new Options() { replace =  "__CONTROLLER__", suffix = "Controller" } },
            { "stateID", new Options() { replace =  "__STATE_ID__", suffix = "StateID" } },
            { "stateFile", new Options() { replace =  "__STATE__", suffix = "State", toggle = false } },
            { "stateController", new Options() { replace =  "__STATE_CONTROLLER__", suffix = "StateController", toggle = false } },
            { "states", new Options() { replace = "__STATES__", visible = false } }
        };

        private void OnGUI()
        {
            path = EditorGUILayout.TextField("Generate path", path);
            mainName = EditorGUILayout.TextField("Name", mainName);
            subFolder = EditorGUILayout.Toggle("Sub Folder", subFolder);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generate Files:", EditorStyles.boldLabel);

            options.Keys.Where(key => options[key].visible).ToList().ForEach(key => 
            {
                var option = options[key];
                option.defaultName = mainName + option.suffix;
                option.customName = string.IsNullOrEmpty(option.customName) ? option.defaultName : option.customName;
                DrawFileOption(key);
            });
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(16, false);
            EditorGUILayout.LabelField("Generate States:", GUILayout.Width(150 - 16));
            states = EditorGUILayout.TextArea(states);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate"))
            {
                Generate();
            }
        }

        private void DrawFileOption(string key)
        {
            var option = options[key];
            EditorGUILayout.BeginHorizontal();
            if (option.toggle)
            {
                option.available = EditorGUILayout.ToggleLeft(option.defaultName, option.available, GUILayout.Width(150));
            }
            else
            {
                EditorGUILayout.Space(16, false);
                EditorGUILayout.LabelField(option.defaultName, GUILayout.Width(150 - 16));
            }
            option.customName = EditorGUILayout.TextField(option.customName);
            EditorGUILayout.EndHorizontal();
        }

        private void Generate()
        {
            // generate director
            var generateDir = Path.Join("Assets", path, mainName);
            if (!Directory.Exists(generateDir))
            {
                Directory.CreateDirectory(generateDir);
            }
            if (subFolder)
            {
                Directory.CreateDirectory(Path.Join(generateDir, "Model"));
                Directory.CreateDirectory(Path.Join(generateDir, "State"));
                Directory.CreateDirectory(Path.Join(generateDir, "Controller"));
            }

            States.customName = string.Join(",", states.Split("\n    "));

            if (ModelFile.available)
                GenerateFile("model_template", Path.Join(generateDir, (subFolder ? "Model/" : "") + ModelFile.name + ".cs"));

            if (ControllerFile.available)
                GenerateFile("controller_template", Path.Join(generateDir, (subFolder ? "Controller/" : "") + ControllerFile.name + ".cs"));

            if (StateFile.available)
            {
                GenerateFile("state_template", Path.Join(generateDir, (subFolder ? "State/" : "") + StateController.name + ".cs"));
                GenerateStateScripts(Path.Join(generateDir, (subFolder ? "State/" : "")));
            }

            AssetDatabase.Refresh();
        }

        private void GenerateStateScripts(string path)
        {
            var template = Resources.Load<TextAsset>("state_script_template");
            states.Split('\n').ToList().ForEach(stateId =>
            {
                var filePath = Path.Join(path, StateFile.name + stateId + ".cs");
                if (File.Exists(filePath))
                {
                    Debug.LogWarning($"File {filePath} already exist");
                    return;
                }

                var text = ReplaceTemplate(template.text)
                    .Replace("__STATE_SCRIPT_ID__", stateId);
                File.WriteAllText(filePath, ReplaceTemplate(text));
            });
        }

        private void GenerateFile(string template, string path)
        {
            if (File.Exists(path))
            {
                Debug.LogWarning($"File {path} already exist");
                return;
            }
            var text = Resources.Load<TextAsset>(template).text;
            File.WriteAllText(path, ReplaceTemplate(text));
        }

        private string ReplaceTemplate(string template)
        {
            return options.Values.Aggregate(template, (current, op) => current.Replace(op.replace, op.name));
        }

        private Options ModelFile => options["model"];
        private Options StateIDName => options["stateID"];
        private Options StateFile => options["stateFile"];
        private Options StateController => options["stateController"];
        private Options States => options["states"];
        private Options ControllerFile => options["controller"];
    }
}
