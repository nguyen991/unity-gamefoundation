using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using GameFoundation.Economy;

namespace GameFoundation.Editor.Economy
{
    public class TransactionEditor : CatalogEditor<Transaction>
    {
        private float layoutWidth = 0f;

        public TransactionEditor() : base("Transaction")
        {
        }

        protected override void DrawCustomItemData()
        {
            selectedItem.transactionType = (Transaction.TransactionType)EditorGUILayout.EnumPopup("Transaction Type", selectedItem.transactionType);
            GUILayout.Space(5f);

            var rect = EditorGUILayout.BeginHorizontal();
            layoutWidth = rect.width > 0 ? rect.width / 2f : layoutWidth;

            GUILayout.BeginVertical("Cost", "window", GUILayout.Width(layoutWidth), GUILayout.MinWidth(200));
            DrawCost();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Reward", "window", GUILayout.MaxWidth(layoutWidth), GUILayout.MinWidth(200));
            DrawReward();
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

        }

        private void DrawCost()
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;
            switch (selectedItem.transactionType)
            {
                case Transaction.TransactionType.Ads:
                    GUILayout.Space(EditorGUIUtility.singleLineHeight);
                    selectedItem.cost.adsId = EditorGUILayout.TextField("Ads ID:", selectedItem.cost.adsId);
                    break;
                case Transaction.TransactionType.IAP:
                    GUILayout.Space(EditorGUIUtility.singleLineHeight);
                    selectedItem.cost.productId = EditorGUILayout.TextField("Product ID:", selectedItem.cost.productId);
                    break;
                default:
                    DrawVirtualCost();
                    break;
            }
            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void DrawVirtualCost()
        {
            DrawTransactionItems<Currency>(
                "Currencies:",
                selectedItem.cost.currencies,
                economyData.currencyCatalog.items,
                (item) =>
                {
                    selectedItem.cost.currencies.Add(new TransactionItem<Currency>() { item = item, amount = 1 });
                }
            );
            GUILayout.Space(10f);
            DrawTransactionItems<Item>(
                "Items:",
                selectedItem.cost.items,
                economyData.itemCatalog.items,
                (item) =>
                {
                    selectedItem.cost.items.Add(new TransactionItem<Item>() { item = item, amount = 1 });
                }
            );
        }

        private void DrawReward()
        {
            DrawTransactionItems<Currency>(
                "Currencies:",
                selectedItem.reward.currencies,
                economyData.currencyCatalog.items,
                (item) =>
                {
                    selectedItem.reward.currencies.Add(new TransactionItem<Currency>() { item = item, amount = 1 });
                }
            );
            GUILayout.Space(10f);
            DrawTransactionItems<Item>(
                "Items:",
                selectedItem.reward.items,
                economyData.itemCatalog.items,
                (item) =>
                {
                    selectedItem.reward.items.Add(new TransactionItem<Item>() { item = item, amount = 1 });
                }
            );
        }

        private void DrawTransactionItems<T>(string title, List<TransactionItem<T>> items, List<T> selections, UnityAction<T> onSelectItem) where T : CatalogItem
        {
            TransactionItem<T> deleteItem = null;

            GUILayout.Label(title, EditorStyles.boldLabel);
            items.ForEach(item =>
            {
                GUILayout.BeginHorizontal("HelpBox", GUILayout.MaxWidth(layoutWidth));
                GUILayout.Label(item.item.key, GUILayout.MaxWidth(50));
                GUILayout.FlexibleSpace();
                item.amount = EditorGUILayout.LongField(item.amount, GUILayout.MaxWidth(100));
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), GUILayout.MaxWidth(30)))
                {
                    deleteItem = item;
                }
                GUILayout.EndHorizontal();
            });

            // add currency
            var buttonRect = EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(layoutWidth));
            if (GUILayout.Button("+"))
            {
                PopupWindow.Show(buttonRect, new SelectItemPopup<T>(
                    selections.Where(s => !items.Exists(it => it.item.key == s.key)).ToList(),
                    onSelectItem)
                );
            }
            EditorGUILayout.EndHorizontal();

            // delete item
            if (deleteItem != null)
            {
                items.Remove(deleteItem);
            }
        }
    }

    class SelectItemPopup<T> : PopupWindowContent where T : CatalogItem
    {
        private List<T> items;

        private Vector2 scrollPos = Vector2.zero;

        private UnityAction<T> onSelectItem;

        public SelectItemPopup(List<T> items, UnityAction<T> onSelectItem)
        {
            this.items = items;
            this.onSelectItem = onSelectItem;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label("Select:", EditorStyles.boldLabel);
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            this.items.ForEach(item =>
            {
                if (GUILayout.Button(item.key))
                {
                    this.editorWindow.Close();
                    this.onSelectItem.Invoke(item);
                }
            });
            GUILayout.EndScrollView();
        }
    }
}
