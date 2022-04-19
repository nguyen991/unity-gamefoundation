using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;
using UnityEditorInternal;

namespace GameFoundation.Editor.Economy
{
    public class StoreEditor : CatalogEditor<Store>
    {
        private float layoutWidth = 200f;

        private string searchTransaction = "";

        private ReorderableList transactionList;

        private SerializedObject serializedObject;

        public StoreEditor() : base("Store")
        {
        }

        public override void Init(EconomyData data, Catalog<Store> catalog)
        {
            base.Init(data, catalog);
        }

        protected override void DrawCustomItemData()
        {
            var rect = EditorGUILayout.BeginHorizontal();
            layoutWidth = rect.width > 0 ? rect.width / 2f : layoutWidth;

            GUILayout.BeginVertical("Store", "window", GUILayout.Width(layoutWidth), GUILayout.MinWidth(200));
            DrawStore();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Transactions", "window", GUILayout.MaxWidth(layoutWidth), GUILayout.MinWidth(200));
            DrawTransactions();
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedItem);
            }
        }

        protected override void OnSelectItem(Store item)
        {
            base.OnSelectItem(item);

            if (item == null)
            {
                serializedObject = null;
                transactionList = null;
            }
            else
            {
                serializedObject = new SerializedObject(selectedItem);
                transactionList = new ReorderableList(serializedObject, serializedObject.FindProperty("transactions"), true, false, false, true);
                transactionList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var item = selectedItem.Transactions[index];
                    EditorGUI.LabelField(rect, item.display);
                };
            }
        }

        public void DrawStore()
        {
            GUILayout.Space(23f);

            if (serializedObject != null && transactionList != null)
            {
                serializedObject.Update();
                transactionList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void DrawTransactions()
        {
            // search box
            GUILayout.BeginHorizontal();
            searchTransaction = GUILayout.TextField(searchTransaction, GUI.skin.FindStyle("ToolbarSeachTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                searchTransaction = "";
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);

            // list
            economyData.transactionCatalog.Items
                .Where(item => item.key.Contains(searchTransaction) && !selectedItem.Transactions.Exists(t => t.key == item.key))
                .ToList()
                .ForEach(item =>
            {
                GUILayout.BeginHorizontal("HelpBox");
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_tab_prev"), GUILayout.Width(30f)))
                {
                    selectedItem.Transactions.Add(item);
                }
                GUILayout.Label(item.display);
                GUILayout.EndHorizontal();
            });
        }
    }
}
