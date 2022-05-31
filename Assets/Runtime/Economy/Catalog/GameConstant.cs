using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class GameConstant : CatalogItem
    {
    }

    [System.Serializable]
    public class GameConstantCatalog : Catalog<GameConstant> { }
}
