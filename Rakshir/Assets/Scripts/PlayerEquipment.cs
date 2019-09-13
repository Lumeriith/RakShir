using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public enum EquipmentType
    {
        Weapon,
        Armor,
        Shoes,
        Helmet
    }

    [SerializeField]
    private List<IItem> listEquipment = new List<IItem>(4);

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            listEquipment.Add(null);
        }
    }

    public void SetEquipment(IItem item, EquipmentType type)
    {
        if (listEquipment[(int)type] != null)
            PopEquipment((int)type);

        listEquipment[(int)type] = item;
    }

    private void PopEquipment(int index)
    {
        // Drop item (listEquipment[index])
    }
}
