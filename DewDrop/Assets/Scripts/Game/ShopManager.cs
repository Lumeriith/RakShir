using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    // 7개
    // 힘 지능 민첩 장비 장비 포션 포션 포션

    public List<Item> itemsAlwaysInStock;

    public List<Item> sellableEquipments;
    public int equipmentsInStockPerShop = 1;

    public List<Item> sellableConsumables;
    public int consumablesInStockPerShop = 3;

    public float itemSellingValueModifier = 0.5f;

    public List<Item> currentStock = new List<Item>();
    public List<int> currentStockNumber = new List<int>();
    public static ShopManager instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<ShopManager>();
            return _instance;
        }
    }
    private static ShopManager _instance;

    private void Awake()
    {
        RerollShop();
    }

    public void RerollShop()
    {
        currentStock.Clear();
        currentStockNumber.Clear();
        currentStock.AddRange(itemsAlwaysInStock);
        for (int i = 0; i < currentStock.Count; i++) currentStockNumber.Add(99);
        for (int i = 0; i < equipmentsInStockPerShop; i++)
        {
            currentStock.Add(sellableEquipments[Random.Range(0, sellableEquipments.Count)]);
            currentStockNumber.Add(1);

        }

        for(int i = 0; i < consumablesInStockPerShop; i++)
        {
            currentStock.Add(sellableConsumables[Random.Range(0, sellableConsumables.Count)]);
            currentStockNumber.Add(1);
        }
    }

    public void SellItem(Item item)
    {
        SFXManager.CreateSFXInstance("si_local_ShopSell", GameManager.instance.localPlayer.transform.position, true);
        GameManager.instance.localPlayer.EarnGold(item.value * itemSellingValueModifier);
        item.DestroySelf();
    }

    public void TryBuyItem(int index)
    {
        if (currentStock.Count <= index) return;
        if (currentStockNumber[index] <= 0) return;

        if (GameManager.instance.localPlayer.HasGold(currentStock[index].value))
        {
            if (GameManager.instance.localPlayer.GetComponent<PlayerInventory>().HasSpaceFor(currentStock[index]))
            {
                SFXManager.CreateSFXInstance("si_local_ShopBuy", GameManager.instance.localPlayer.transform.position, true);
                GameManager.instance.localPlayer.SpendGold(currentStock[index].value);
                GameManager.instance.localPlayer.ActivateImmediately(GameManager.SpawnItem(currentStock[index].name, GameManager.instance.localPlayer.transform.position));
                currentStockNumber[index]--;
            }
            else
            {
                // 공간이 없습니다!
            }
        }
        else
        {
            // 돈이 없습니다!
        }

    }
}
