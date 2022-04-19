using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using GameFoundation.Economy;

#if GF_IAP
using UnityEngine.Purchasing;
#endif

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

            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedItem);
            }
        }

        private void DrawCost()
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;
            switch (selectedItem.transactionType)
            {
                case Transaction.TransactionType.Ads:
                    DrawAdsCost();
                    break;
                case Transaction.TransactionType.IAP:
                    DrawIAPCost();
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
                selectedItem.Cost.currencies,
                economyData.currencyCatalog.Items,
                (item) =>
                {
                    selectedItem.Cost.currencies.Add(new TransactionItem<Currency>() { item = item, amount = 1 });
                }
            );
            GUILayout.Space(10f);
            DrawTransactionItems<Item>(
                "Items:",
                selectedItem.Cost.items,
                economyData.itemCatalog.Items,
                (item) =>
                {
                    selectedItem.Cost.items.Add(new TransactionItem<Item>() { item = item, amount = 1 });
                }
            );
        }

        private void DrawAdsCost()
        {
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Rewarded Ad");
        }

        private void DrawIAPCost()
        {
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            selectedItem.Cost.productId = EditorGUILayout.TextField("Product Id:", selectedItem.Cost.productId);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);

#if GF_IAP
            // find product in catalog
            var catalog = ProductCatalog.LoadDefaultCatalog().allValidProducts.FirstOrDefault(c => c.id == selectedItem.Cost.productId);
            if (catalog != null)
            {
                GUILayout.Label("Product Detail:", EditorStyles.boldLabel);
                GUILayout.Label($"{catalog.id}");
                GUILayout.Label($"{catalog.type.ToString()}");
            }
            else
            {
                GUILayout.Label("Invalid Product Id", EditorStyles.boldLabel);
            }
#else
            GUILayout.Label("IAP feature not yet enabled", EditorStyles.boldLabel);
#endif
        }

        private void DrawReward()
        {
            DrawTransactionItems<Currency>(
                "Currencies:",
                selectedItem.Reward.currencies,
                economyData.currencyCatalog.Items,
                (item) =>
                {
                    selectedItem.Reward.currencies.Add(new TransactionItem<Currency>() { item = item, amount = 1 });
                }
            );
            GUILayout.Space(10f);
            DrawTransactionItems<Item>(
                "Items:",
                selectedItem.Reward.items,
                economyData.itemCatalog.Items,
                (item) =>
                {
                    selectedItem.Reward.items.Add(new TransactionItem<Item>() { item = item, amount = 1 });
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
                    (T value) =>
                    {
                        onSelectItem.Invoke(value);
                        SaveAsset();
                    })
                );
            }
            EditorGUILayout.EndHorizontal();

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
