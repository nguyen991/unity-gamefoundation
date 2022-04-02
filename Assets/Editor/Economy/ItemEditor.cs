using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;

namespace GameFoundation.Editor.Economy
{
    public class ItemEditor : CatalogEditor<Item>
    {
        public ItemEditor() : base("Item")
        {
        }

        protected override void DrawCustomItemData()
        {
            selectedItem.persistent = EditorGUILayout.Toggle("Persistent", selectedItem.persistent);
            if (!selectedItem.persistent)
            {
                selectedItem.initBalance = EditorGUILayout.LongField("Init Balance", selectedItem.initBalance);
                selectedItem.maxBalance = EditorGUILayout.LongField("Max Balance", selectedItem.maxBalance);
                selectedItem.stackable = EditorGUILayout.Toggle("Stackable", selectedItem.stackable);
                if (selectedItem.stackable)
                {
                    selectedItem.itemPerStack = EditorGUILayout.LongField("Items per Stack", selectedItem.itemPerStack);
                }
            }
        }
    }
}

