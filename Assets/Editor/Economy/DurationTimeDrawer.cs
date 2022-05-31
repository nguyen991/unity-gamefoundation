using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GameFoundation.Economy;

namespace GameFoundation.Editor.Economy
{
    [CustomPropertyDrawer(typeof(Reward.DurationTime))]
    public class DurationTimeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var durationRect = new Rect(position.x, position.y, 80, position.height);
            var typeRect = new Rect(position.x + 85, position.y, 80, position.height);

            // Draw fields
            EditorGUI.PropertyField(durationRect, property.FindPropertyRelative("duration"), GUIContent.none);
            EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("type"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
