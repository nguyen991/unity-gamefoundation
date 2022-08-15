using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;
using UnityEngine.Events;
using System.Linq;
using UnityEditorInternal;

namespace GameFoundation.Editor.Economy
{
    public class RewardEditor : CatalogEditor<Reward>
    {
        private ReorderableList rewardTable = null;        

        private float itemHeight = 22f;

        public RewardEditor() : base("Reward")
        {
        }

        protected override void OnSelectItem(Reward item)
        {
            base.OnSelectItem(item);
            if (item == null)
            {
                rewardTable = null;
            }
            else
            {
                rewardTable = new ReorderableList(serializedObject, serializedObject.FindProperty("rewardTable"), true, false, true, true);
                rewardTable.elementHeightCallback = (index) =>
                {
                    var maxItem = Mathf.Max(selectedItem.RewardTable[index].currencies.Count, selectedItem.RewardTable[index].items.Count);
                    return (maxItem + 2) * itemHeight;
                };
                rewardTable.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.HelpBox(rect, "", MessageType.None);

                    var rewardItem = selectedItem.RewardTable[index];

                    // Percentage or Progressive
                    if (selectedItem.type == Reward.RewardType.Randomized)
                    {
                        EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, 100, itemHeight), "Percent", EditorStyles.boldLabel);
                        var percentRect = new Rect(rect.x + 5, rect.y + itemHeight, 70, EditorGUIUtility.singleLineHeight);
                        rewardItem.percent = EditorGUI.FloatField(percentRect, rewardItem.percent);
                        rewardItem.percent = Mathf.Clamp(rewardItem.percent, 0f, 1f);
                    }
                    else
                    {
                        EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, 100, itemHeight), "Step --> " + (index + 1), EditorStyles.boldLabel);
                    }

                    var padding = 50f;
                    var width = (rect.width - 140 - padding) / 2f;
                    rect = new Rect(rect.x + 135 + padding, rect.y, width - padding / 2f, rect.height);
                    DrawRewardTable<Currency>
                        (rect,
                        "Currencies",
                        rewardItem.currencies,
                        economyData.currencyCatalog.Items,
                        (item) =>
                        {
                            rewardItem.currencies.Add(new TransactionItem<Currency>() { item = item, amount = 1 });
                            OnSelectItem(selectedItem);
                        }
                    );
                    DrawRewardTable<Item>
                        (new Rect(rect.x + width + padding / 2f, rect.y, width - padding / 2f, rect.height),
                        "Items:",
                        rewardItem.items,
                        economyData.itemCatalog.Items,
                        (item) =>
                        {
                            rewardItem.items.Add(new TransactionItem<Item>() { item = item, amount = 1 });
                            OnSelectItem(selectedItem);
                        }
                    );
                };
            }
        }

        protected override void DrawCustomItemData()
        {
            EditorGUILayout.LabelField("Information:", EditorStyles.boldLabel);

            selectedItem.type = (Reward.RewardType)EditorGUILayout.EnumPopup("Type", selectedItem.type);
            if (serializedObject != null)
            {
                EditorGUILayout.BeginHorizontal();
                selectedItem.limit = EditorGUILayout.IntField("Limit", selectedItem.limit, GUILayout.Width(232));
                EditorGUILayout.LabelField(" / ", GUILayout.Width(15));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("limitTime"), GUIContent.none);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldown"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("expire"), true);
            }

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            // check if percentage is validate
            if (selectedItem.type == Reward.RewardType.Randomized && selectedItem.RewardTable?.Sum(it => it.percent) != 1f)
            {
                EditorGUILayout.HelpBox("Summary of percentage is not equal to 1", MessageType.Warning);
            }

            EditorGUILayout.LabelField("Reward Table:", EditorStyles.boldLabel);
            rewardTable?.DoLayoutList();
        }

        private void DrawRewardTable<T>(Rect rect, string title, List<TransactionItem<T>> items, List<T> selections, UnityAction<T> onSelectItem) where T : CatalogItem
        {
            TransactionItem<T> deleteItem = null;

            var padding = Mathf.Abs((EditorGUIUtility.singleLineHeight - itemHeight) / 2f);

            rect = new Rect(rect.x + 5, rect.y, rect.width - 5, itemHeight);
            EditorGUI.LabelField(rect, title, EditorStyles.boldLabel);
            items.ForEach(item =>
            {
                rect.y += itemHeight;
                EditorGUI.HelpBox(rect, "", MessageType.None);
                EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, rect.width, rect.height), item.item.key);
                item.amount = EditorGUI.LongField(new Rect(rect.x + rect.width - 100 - padding * 2f, rect.y + padding, 70, EditorGUIUtility.singleLineHeight), item.amount);
                if (GUI.Button(new Rect(rect.x + rect.width - 30 - padding, rect.y + padding, 30, EditorGUIUtility.singleLineHeight), EditorGUIUtility.IconContent("d_TreeEditor.Trash")))
                {
                    deleteItem = item;
                }
            });

            rect.y += itemHeight;
            var buttonRect = new Rect(rect.x + rect.width - 30 - padding, rect.y + padding, 30, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, "+"))
            {
                PopupWindow.Show(buttonRect, new SelectItemPopup<T>(
                    selections.Where(s => !items.Exists(it => it.item.key == s.key)).ToList(),
                    (T value) =>
                    {
                        onSelectItem.Invoke(value);
                        SaveAsset();
                    })
                );
            }

            // delete item
            if (deleteItem != null)
            {
                items.Remove(deleteItem);
                SaveAsset();
            }
        }

        private void SaveAsset()
        {
            if (selectedItem != null)
            {
                EditorUtility.SetDirty(selectedItem);
                AssetDatabase.SaveAssets();
            }
        }

        protected override void DrawInPlayMode()
        {
            EditorGUILayout.LabelField("Is Claimable", EconomyManager.Instance.Reward.IsClaimable(selectedItem.key).ToString());
            EditorGUILayout.LabelField("Remain", EconomyManager.Instance.Reward.Remain(selectedItem.key).ToString());
            EditorGUILayout.LabelField("Next Claim", EconomyManager.Instance.Reward.UntilNextClaim(selectedItem.key).ToString("hh\\:mm\\:ss"));
            if (selectedItem.type == Reward.RewardType.Progressive)
            {
                EditorGUILayout.LabelField("Current Step", EconomyManager.Instance.Reward.Progressive(selectedItem.key).ToString());
            }
            if (GUILayout.Button("Claim"))
            {
                EconomyManager.Instance.Reward.Claim(selectedItem.key, true);
            }
            if (GUILayout.Button("Reset"))
            {
                EconomyManager.Instance.Reward.Reset(selectedItem.key);
            }
        }
    }
}
