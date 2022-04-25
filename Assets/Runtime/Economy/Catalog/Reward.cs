using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class Reward : CatalogItem
    {
    }

    [System.Serializable]
    public class RewardCatalog : Catalog<Reward> { }
}