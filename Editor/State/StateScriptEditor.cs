using UnityEngine;
using UnityEditor;
using GameFoundation.State;

namespace GameFoundation.Editor
{
    [CustomEditor(typeof(StateScript<,>), true)]
    public class StateScriptEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.TextField("State ID", target.GetType().GetProperty("ID").GetValue(target, null).ToString());
        }
    }
}