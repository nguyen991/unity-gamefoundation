using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;

namespace GameFoundation.Editor.Economy
{
    public class EconomyEditor : EditorWindow
    {
        private readonly string[] Tabs = new string[] { "Currency", "Item", "Transaction", "Store", "Reward" };
        private int selectedTab = 0;
        private EconomyData economyData = null;
        private CurrencyEditor currencyEditor = new CurrencyEditor();
        private ItemEditor itemEditor = new ItemEditor();
        private TransactionEditor transactionEditor = new TransactionEditor();
        private StoreEditor storeEditor = new StoreEditor();
        private RewardEditor rewardEditor = new RewardEditor();

        [MenuItem("Game Foundation/Economy", false, 1)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(EconomyEditor));
            window.titleContent = new GUIContent("Economy Editor");
            window.Show();
        }

        private void OnGUI()
        {
            economyData = EditorGUILayout.ObjectField("Economy Data", economyData, typeof(EconomyData), false) as EconomyData;
            if (economyData == null)
            {
                // search in resource folder
                var guids = AssetDatabase.FindAssets("t:EconomyData");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    economyData = AssetDatabase.LoadAssetAtPath<EconomyData>(path);
                }
                else
                {
                    // create Resources folder
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    // create new asset
                    economyData = ScriptableObject.CreateInstance<EconomyData>();
                    AssetDatabase.CreateAsset(economyData, "Assets/Resources/EconomyData.asset");
                    AssetDatabase.SaveAssets();
                }
            }

            selectedTab = GUILayout.Toolbar(selectedTab, Tabs);
            switch (selectedTab)
            {
                case 0:
                    DrawCatalog(currencyEditor, economyData, economyData.currencyCatalog);
                    break;
                case 1:
                    DrawCatalog(itemEditor, economyData, economyData.itemCatalog);
                    break;
                case 2:
                    DrawCatalog(transactionEditor, economyData, economyData.transactionCatalog);
                    break;
                case 3:
                    DrawCatalog(storeEditor, economyData, economyData.storeCatalog);
                    break;
                case 4:
                    DrawCatalog(rewardEditor, economyData, economyData.rewardCatalog);
                    break;
                default:
                    break;
            }
        }

        private void DrawCatalog<T>(CatalogEditor<T> editor, EconomyData data, Catalog<T> catalog) where T : CatalogItem
        {
            editor.Init(data, catalog);
            editor.Draw();
        }
    }
}
