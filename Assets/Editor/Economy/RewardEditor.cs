using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameFoundation.Economy;

namespace GameFoundation.Editor.Economy
{
    public class RewardEditor : CatalogEditor<Reward>
    {
        public RewardEditor() : base("Reward")
        {
        }

        protected override void DrawCustomItemData()
        {
        }
    }
}
