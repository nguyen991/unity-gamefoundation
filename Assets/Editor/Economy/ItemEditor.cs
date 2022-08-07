using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;
using UnityEditorInternal;

namespace GameFoundation.Editor.Economy
{
    public class ItemEditor : CatalogEditor<Item>
    {
        private enum PropertyType
        {
            String,
            Long,
            Double,
            Bool,
        }

        // private string searchInventory = "";
        private ReorderableList inventoryListReorder = null;
        private List<InventoryManager.ItemInstance> itemInstances = null;
        private InventoryManager.ItemInstance selectedItemInstance = null;
        private bool deleteItemInstance = false;
        private int addAmount = 1;
        private string newPropertyKey = "";
        private PropertyType newPropertyType = PropertyType.String;
        private string newPropertyValue = "";

        private const float LabelWidthSize = 80f;

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

        protected override void DrawInPlayMode()
        {
            base.DrawInPlayMode();

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = LabelWidthSize;

            var rect = EditorGUILayout.BeginHorizontal();
            var layoutWidth = rect.width > 0 ? rect.width / 2f : 400;

            GUILayout.BeginVertical("Items", "window", GUILayout.Width(layoutWidth), GUILayout.MinWidth(200));
            DrawInventory();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Properties", "window", GUILayout.MaxWidth(layoutWidth), GUILayout.MinWidth(200));
            DrawInventoryProperties();
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void DrawInventory()
        {
            // // search box
            // GUILayout.BeginHorizontal();
            // searchInventory = GUILayout.TextField(searchInventory, GUI.skin.FindStyle("ToolbarSeachTextField"));
            // if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            // {
            //     searchInventory = "";
            // }
            // GUILayout.EndHorizontal();
            // GUILayout.Space(5f);

            // list items
            itemInstances = EconomyManager.Instance.Inventory.Query(keys: new string[] { selectedItem.key }).ToList();
            if (inventoryListReorder == null)
            {
                inventoryListReorder = new ReorderableList(itemInstances, typeof(InventoryManager.ItemInstance), false, false, false, false);
                inventoryListReorder.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var item = itemInstances[index];
                    EditorGUI.LabelField(rect, $"{item.key} ({item.amount})");
                    if (GUI.Button(
                            new Rect(rect.x + rect.width - 30, rect.y, 30, rect.height),
                            EditorGUIUtility.IconContent("d_TreeEditor.Trash")) &&
                        EditorUtility.DisplayDialog("Delete", $"Are you sure you want to delete {item.key}?", "Yes", "No"))
                    {
                        deleteItemInstance = true;
                    }
                };
                inventoryListReorder.onSelectCallback = (ReorderableList l) => { selectedItemInstance = itemInstances[l.index]; };
            }
            else
            {
                inventoryListReorder.list = itemInstances;
            }
            inventoryListReorder.DoLayoutList();

            // delete item instance
            if (deleteItemInstance)
            {
                EconomyManager.Instance.Inventory.RemoveById(selectedItemInstance.id, selectedItemInstance.amount);
            }
            deleteItemInstance = false;

            // add new item
            GUILayout.BeginHorizontal();
            if (selectedItem.stackable)
            {
                EditorGUIUtility.labelWidth = LabelWidthSize / 2f;
                addAmount = EditorGUILayout.IntField("Add", addAmount, GUILayout.Width(100));
                EditorGUIUtility.labelWidth = LabelWidthSize;
            }
            else
            {
                addAmount = 1;
                EditorGUILayout.LabelField("Add 1", GUILayout.Width(50));
            }
            if (GUILayout.Button($"of \"{selectedItem.key}\""))
            {
                EconomyManager.Instance.Inventory.Create(selectedItem.key, addAmount);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawInventoryProperties()
        {
            if (selectedItemInstance == null)
                return;

            EditorGUILayout.LabelField("Id", selectedItemInstance.id);
            if (selectedItem.stackable)
            {
                selectedItemInstance.amount = EditorGUILayout.LongField("Amount", selectedItemInstance.amount);
            }
            if (selectedItemInstance.data != null)
            {
                var keys = selectedItemInstance.data.Keys.ToList();
                foreach (var key in keys)
                {
                    GUILayout.BeginHorizontal();
                    switch (selectedItemInstance.data[key])
                    {
                        case string str:
                            selectedItemInstance.data[key] = EditorGUILayout.TextField(key, str);
                            break;
                        case long lng:
                            selectedItemInstance.data[key] = EditorGUILayout.LongField(key, lng);
                            break;
                        case double dbl:
                            selectedItemInstance.data[key] = EditorGUILayout.DoubleField(key, dbl);
                            break;
                        case bool bl:
                            selectedItemInstance.data[key] = EditorGUILayout.Toggle(key, bl);
                            break;
                        case object obj:
                            EditorGUILayout.LabelField(key, obj.ToString());
                            break;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        selectedItemInstance.data.Remove(key);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            // add property
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("Add Property");
            GUILayout.BeginHorizontal();
            newPropertyKey = EditorGUILayout.TextField(newPropertyKey, GUILayout.Width(80));
            newPropertyType = (PropertyType)EditorGUILayout.EnumPopup(newPropertyType, GUILayout.Width(80));
            newPropertyValue = EditorGUILayout.TextField(newPropertyValue);
            if (GUILayout.Button("+", GUILayout.Width(20)) && !string.IsNullOrEmpty(newPropertyKey))
            {
                if (selectedItemInstance.data == null)
                {
                    selectedItemInstance.data = new Dictionary<string, object>();
                }
                if (selectedItemInstance.data.ContainsKey(newPropertyKey))
                {
                    EditorUtility.DisplayDialog("Error", $"Property {newPropertyKey} already exists.", "Ok");
                }
                else
                {
                    object value;
                    switch (newPropertyType)
                    {
                        case PropertyType.Long:
                            value = long.Parse(newPropertyValue);
                            break;
                        case PropertyType.Double:
                            value = double.Parse(newPropertyValue);
                            break;
                        case PropertyType.Bool:
                            value = bool.Parse(newPropertyValue);
                            break;
                        default:
                            value = newPropertyValue;
                            break;
                    }
                    selectedItemInstance.data[newPropertyKey] = value;
                    newPropertyKey = "";
                    newPropertyValue = "";
                }
            }
            GUILayout.EndHorizontal();
        }


    }
}

