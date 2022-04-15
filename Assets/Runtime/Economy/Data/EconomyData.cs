using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace GameFoundation.Economy
{
    [CreateAssetMenu(fileName = "EconomyData", menuName = "Game Foundation/Economy/Data")]
    public class EconomyData : ScriptableObject
    {
        [HideInInspector] public CurrencyCatalog currencyCatalog;
        [HideInInspector] public ItemCatalog itemCatalog;
        [HideInInspector] public TransactionCatalog transactionCatalog;
        [HideInInspector] public StoreCatalog storeCatalog;
    }
}
