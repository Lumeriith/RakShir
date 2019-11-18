using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewChangeSFXManager : MonoBehaviour
{
    private IngameNodeType lastType = IngameNodeType.Unknown;
    private void Update()
    {
        if (lastType == GameManager.cachedCurrentNodeType) return;

        if (lastType == IngameNodeType.Inventory)
            SFXManager.CreateSFXInstance("si_local_InventoryClose", SFXManager.instance.transform.position, true);
        else if (lastType == IngameNodeType.Map)
            SFXManager.CreateSFXInstance("si_local_MapClose", SFXManager.instance.transform.position, true);
        else if (lastType == IngameNodeType.MapObelisk)
            SFXManager.CreateSFXInstance("si_local_MapObeliskClose", SFXManager.instance.transform.position, true);
        else if (lastType == IngameNodeType.Shop)
            SFXManager.CreateSFXInstance("si_local_ShopClose", SFXManager.instance.transform.position, true);


        if (GameManager.cachedCurrentNodeType == IngameNodeType.Inventory)
            SFXManager.CreateSFXInstance("si_local_InventoryOpen", SFXManager.instance.transform.position, true);
        else if (GameManager.cachedCurrentNodeType == IngameNodeType.Map)
            SFXManager.CreateSFXInstance("si_local_MapOpen", SFXManager.instance.transform.position, true);
        else if (GameManager.cachedCurrentNodeType == IngameNodeType.MapObelisk)
            SFXManager.CreateSFXInstance("si_local_MapObeliskOpen", SFXManager.instance.transform.position, true);
        else if (GameManager.cachedCurrentNodeType == IngameNodeType.Shop)
            SFXManager.CreateSFXInstance("si_local_ShopOpen", SFXManager.instance.transform.position, true);

        lastType = GameManager.cachedCurrentNodeType;

    }
}
