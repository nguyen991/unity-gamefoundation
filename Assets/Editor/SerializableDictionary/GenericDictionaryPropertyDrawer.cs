using System;
using UnityEditor;
using UnityEngine;

namespace GameFoundation.Editor
{
    /// <summary>
    /// Draws the generic dictionary a bit nicer than Unity would natively (not as many expand-arrows
    /// and better spacing between KeyValue pairs). Also renders a warning-box if there are duplicate
    /// keys in the dictionary.
    /// </summary>
    [CustomPropertyDrawer(typeof(GenericDictionary<,>))]
    public class GenericDictionaryPropertyDrawer : PropertyDrawer
    {
        static float lineHeight = EditorGUIUtility.singleLineHeight;
        static float vertSpace = EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, property);

            // Draw list.
            var list = property.FindPropertyRelative("list");
            string fieldName = ObjectNames.NicifyVariableName(fieldInfo.Name);
            var currentPos = new Rect(pos.x, pos.y, pos.width, lineHeight);
            EditorGUI.PropertyField(currentPos, list, new GUIContent(fieldName), true);

            // Draw key collision warning.
            var keyCollision = property.FindPropertyRelative("keyCollision").boolValue;
            if (keyCollision)
            {
                currentPos.y += EditorGUI.GetPropertyHeight(list, true) + vertSpace;
                var entryPos = new Rect(currentPos.x, currentPos.y, currentPos.width, lineHeight * 2f);
                EditorGUI.HelpBox(entryPos, "Duplicate keys will not be serialized.", MessageType.Error);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totHeight = 0f;

            // Height of KeyValue list.
            var listProp = property.FindPropertyRelative("list");
            totHeight += EditorGUI.GetPropertyHeight(listProp, true);

            // Height of key collision warning.
            bool keyCollision = property.FindPropertyRelative("keyCollision").boolValue;
            if (keyCollision)
            {
                totHeight += lineHeight * 2f + vertSpace;
            }

            return totHeight;
        }
    }
}
