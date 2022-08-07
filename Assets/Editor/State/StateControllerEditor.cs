using UnityEngine;
using UnityEditor;
using GameFoundation.State;
using System.Collections.Generic;

namespace GameFoundation.Editor
{
    [CustomEditor(typeof(StateController<,>), true)]
    public class StateControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // validate states
            var states = serializedObject.FindProperty("states");
            var ids = new List<string>();
            for (var i = 0; i < states.arraySize; i++)
            {
                var element = states.GetArrayElementAtIndex(i).objectReferenceValue;
                if (element == null)
                {
                    // draw error box
                    EditorGUILayout.HelpBox($"States have a null value.", MessageType.Error);
                    break;
                }

                var id = element.GetType().GetProperty("ID").GetValue(element, null).ToString();
                if (ids.Contains(id))
                {
                    // draw error box
                    EditorGUILayout.HelpBox($"State \"{id}\" is duplicated.", MessageType.Error);
                    break;
                }
                else
                {
                    ids.Add(id);
                }
            }

            EditorGUILayout.TextField("Current State", target.GetType().GetProperty("currentState").GetValue(target, null).ToString());
        }
    }
}