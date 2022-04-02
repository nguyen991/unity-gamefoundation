using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;

namespace GameFoundation.Editor.Economy
{
    public class CurrencyEditor : CatalogEditor<Currency>
    {
        public CurrencyEditor() : base("Currency")
        {
        }

        protected override void DrawCustomItemData()
        {
            selectedItem.initBalance = EditorGUILayout.LongField("Init Balance", selectedItem.initBalance);
            selectedItem.maxBalance = EditorGUILayout.LongField("Max Balance", selectedItem.maxBalance);
        }
    }
}
