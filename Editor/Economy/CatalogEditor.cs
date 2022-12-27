using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;
using UnityEditorInternal;

namespace GameFoundation.Editor.Economy
{
    public class CatalogEditor<T> where T : CatalogItem
    {
        protected EconomyData economyData;
        protected Catalog<T> catalog;

        protected T selectedItem = null;
        protected SerializedObject serializedObject;
        protected T deleteItem = null;
        protected string newKey = "";
        protected string searchString = "";

        protected Vector2 listScrollPos = Vector2.zero;
        protected Vector2 infoScrollPos = Vector2.zero;

        protected string title = "";

        private ReorderableList catalogListReorder;
        private ReorderableList tagListReorder;

        public CatalogEditor(string title)
        {
            this.title = title;
        }

        public virtual void Init(EconomyData data, Catalog<T> catalog)
        {
            if (this.catalog != catalog)
            {
                this.catalogListReorder = new ReorderableList(catalog.Items, typeof(T), true, false, false, false);
                this.catalogListReorder.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var item = catalog.Items[index];
                    EditorGUI.LabelField(rect, item.display);
                    if (GUI.Button(
                            new Rect(rect.x + rect.width - 30, rect.y, 30, rect.height),
                            EditorGUIUtility.IconContent("d_TreeEditor.Trash")) &&
                        EditorUtility.DisplayDialog("Delete", $"Are you sure you want to delete {item.key}?", "Yes", "No"))
                    {
                        deleteItem = item;
                        OnSelectItem(null);
                    }
                };
                this.catalogListReorder.onReorderCallbackWithDetails = (ReorderableList list, int oldIndex, int newIndex) =>
                {
                    EditorUtility.SetDirty(data);
                    AssetDatabase.SaveAssets();
                };
                this.catalogListReorder.onSelectCallback = (ReorderableList l) => OnSelectItem(catalog.Items[l.index]);
            }

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
            if (!string.IsNullOrEmpty(searchString))
            {
                catalog.Items
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
            }
            else
            {
                this.catalogListReorder.DoLayoutList();
            }

            GUILayout.EndScrollView();
            if (GUILayout.Button("+"))
            {
                OnSelectItem(null);
            }

            // delete item
            if (deleteItem)
            {
                catalog.Items.Remove(deleteItem);
                AssetDatabase.RemoveObjectFromAsset(deleteItem);
                Undo.DestroyObjectImmediate(deleteItem);
                AssetDatabase.SaveAssets();
                deleteItem = null;
            }
        }

        protected virtual void OnSelectItem(T item)
        {
            selectedItem = item;
            serializedObject?.Dispose();
            serializedObject = item ? new SerializedObject(item) : null;

            if (serializedObject != null)
            {
                // create tag list
                var tagProperty = serializedObject.FindProperty("tags");
                tagListReorder = new ReorderableList(serializedObject, tagProperty, true, true, true, true);
                tagListReorder.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Add New"), false, AddTag, null);
                    menu.AddItem(new GUIContent("----------"), false, null);
                    Tags().Where(tag => !selectedItem.IsHaveTag(tag)).ToList().ForEach(tag => menu.AddItem(new GUIContent(tag), false, AddTag, tag));
                    menu.ShowAsContext();
                };
                tagListReorder.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Tags");
                tagListReorder.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = tagProperty.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    rect.y += 2;
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                };
            }
            else
            {
                tagListReorder  = null;
            }
        }

        protected void AddTag(object value)
        {
            var tag = value != null ? value as string : "";
            if (!selectedItem.IsHaveTag(tag))
                selectedItem.tags.Add(tag);
        }

        private void DrawItem()
        {
            serializedObject?.Update();

            GUILayout.BeginVertical("HelpBox");
            infoScrollPos = GUILayout.BeginScrollView(infoScrollPos);

            // basic info
            GUILayout.Label("Basic", EditorStyles.boldLabel);
            GUILayout.BeginVertical("GroupBox");
            EditorGUILayout.TextField("Key", selectedItem.key);
            selectedItem.display = EditorGUILayout.TextField("Display", selectedItem.display);

            EditorGUILayout.Space();
            tagListReorder?.DoLayoutList();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("properties"), true);
            
            GUILayout.EndVertical();

            // item custom data
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.BeginVertical("GroupBox");
            DrawCustomItemData();
            GUILayout.EndVertical(); // Vertical - GroupBox

            // draw debug mode
            if (Application.isPlaying)
            {
                GUILayout.Label("Debug", EditorStyles.boldLabel);
                GUILayout.BeginVertical("GroupBox");
                DrawInPlayMode();
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView(); // ScrollView
            GUILayout.EndVertical(); // Vertical - HelpBox

            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedItem);
            }
            serializedObject?.ApplyModifiedProperties();
        }

        private IEnumerable<string> Tags()
        {
            var tags = new List<string>();
            economyData.currencyCatalog.Items.ForEach(it => { if(tags != null) tags.AddRange(it.tags); });
            economyData.itemCatalog.Items.ForEach(it => { tags.AddRange(it.tags);  });
            economyData.transactionCatalog.Items.ForEach(it => { tags.AddRange(it.tags);  });
            economyData.rewardCatalog.Items.ForEach(it => { tags.AddRange(it.tags);  });
            return tags.Distinct().Where(it => !string.IsNullOrEmpty(it));
        }
       
        protected virtual void DrawCustomItemData()
        {
        }

        protected virtual void DrawInPlayMode()
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
                item.display = ObjectNames.NicifyVariableName(newKey);
                catalog.Items.Add(item);

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
