using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;

namespace GameFoundation.Editor.Economy
{
    public class CatalogEditor<T> where T : CatalogItem
    {
        protected EconomyData economyData;
        protected Catalog<T> catalog;

        protected T selectedItem = null;
        protected T deleteItem = null;
        protected string newKey = "";
        protected string searchString = "";

        protected Vector2 listScrollPos = Vector2.zero;
        protected Vector2 infoScrollPos = Vector2.zero;

        protected string title = "";

        public CatalogEditor(string title)
        {
            this.title = title;
        }

        public virtual void Init(EconomyData data, Catalog<T> catalog)
        {
            this.economyData = data;
            this.catalog = catalog;
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal(title.ToUpper(), "window");

            GUILayout.BeginVertical("List", "window", GUILayout.Width(230));
            DrawList();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Information", "window");
            if (selectedItem)
            {
                DrawItem();
            }
            else
            {
                DrawCreateItem();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            listScrollPos = GUILayout.BeginScrollView(listScrollPos);

            // search box
            GUILayout.BeginHorizontal();
            searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                searchString = "";
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);

            // list
            catalog.items
                .Where(item => item.key.Contains(searchString))
                .ToList()
                .ForEach(item =>
            {
                GUILayout.BeginHorizontal("HelpBox");
                GUILayout.Label(item.display, EditorStyles.boldLabel, GUILayout.Width(140));
                if (GUILayout.Button(EditorGUIUtility.IconContent("_Popup")))
                {
                    OnSelectItem(item);
                }
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash")) &&
                    EditorUtility.DisplayDialog("Delete", $"Are you sure you want to delete {item.key}?", "Yes", "No")
                    )
                {
                    deleteItem = item;
                    OnSelectItem(null);
                }
                GUILayout.EndHorizontal();
            });
            GUILayout.EndScrollView();
            if (GUILayout.Button("+"))
            {
                OnSelectItem(null);
            }

            // delete item
            if (deleteItem)
            {
                catalog.items.Remove(deleteItem);
                AssetDatabase.RemoveObjectFromAsset(deleteItem);
                Undo.DestroyObjectImmediate(deleteItem);
                AssetDatabase.SaveAssets();
                deleteItem = null;
            }
        }

        protected virtual void OnSelectItem(T item)
        {
            selectedItem = item;
        }

        private void DrawItem()
        {
            GUILayout.BeginVertical("HelpBox");

            infoScrollPos = GUILayout.BeginScrollView(infoScrollPos);

            // basic info
            GUILayout.Label("Basic", EditorStyles.boldLabel);
            GUILayout.BeginVertical("GroupBox");
            EditorGUILayout.TextField("Key", selectedItem.key);
            selectedItem.display = EditorGUILayout.TextField("Display", selectedItem.display);
            EditorGUILayout.PropertyField(new SerializedObject(selectedItem).FindProperty("tags"), true);
            EditorGUILayout.PropertyField(new SerializedObject(selectedItem).FindProperty("properties"), true);
            GUILayout.EndVertical();

            // item custom data
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.BeginVertical("GroupBox");
            DrawCustomItemData();
            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        // int tags = 0;
        // private void DrawTags()
        // {
        //     string[] options = new string[] { "tag_1", "tag_2", "tag_3" };
        //     tags = EditorGUILayout.MaskField("Tags", tags, options);
        // }

        protected virtual void DrawCustomItemData()
        {
        }

        private void DrawCreateItem()
        {
            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label($"Create new {title}", EditorStyles.boldLabel);

            GUILayout.BeginVertical("GroupBox");
            newKey = EditorGUILayout.TextField("Key", newKey);
            GUILayout.Space(10);

            if (string.IsNullOrEmpty(newKey) || catalog.IsHaveKey(newKey))
            {
                EditorGUILayout.HelpBox($"Key is invalidate", MessageType.Error);
            }
            else if (GUILayout.Button("Create"))
            {
                // create new item
                var item = ScriptableObject.CreateInstance<T>();
                item.name = $"{title}_{newKey}";
                item.key = newKey;
                item.display = newKey[0].ToString().ToUpper() + newKey.Substring(1);
                catalog.items.Add(item);

                // update asset database
                AssetDatabase.AddObjectToAsset(item, economyData);
                EditorUtility.SetDirty(economyData);
                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssets();

                newKey = "";
                OnSelectItem(item);
            }

            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }
    }
}
