using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;

namespace GameFoundation.Editor.Economy
{
    public class CurrencyEditor : CatalogEditor<Currency>
    {
        private DebugValue debugValue = null;

        public CurrencyEditor() : base("Currency")
        {
        }

        protected override void DrawCustomItemData()
        {
            selectedItem.initBalance = EditorGUILayout.LongField("Init Balance", selectedItem.initBalance);
            selectedItem.maxBalance = EditorGUILayout.LongField("Max Balance", selectedItem.maxBalance);
        }

        protected override void DrawInPlayMode()
        {
            if (debugValue == null)
            {
                debugValue = new DebugValue();
            }

            EditorGUILayout.LabelField("Balance", EconomyManager.Instance.Wallet.Get(selectedItem.key).ToString());
            GUILayout.BeginHorizontal();
            debugValue.balance = EditorGUILayout.LongField("Change", debugValue.balance);
            if (GUILayout.Button("Update", GUILayout.Width(100)))
            {
                EconomyManager.Instance.Wallet.Set(selectedItem.key, debugValue.balance);
            }
            GUILayout.EndHorizontal();
        }

        private class DebugValue
        {
            public long balance = 0;
        }
    }
}
