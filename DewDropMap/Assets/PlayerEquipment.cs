using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public float getItemRadius;
    public LayerMask item;
    public enum EquipmentType
    {
        Weapon,
        Armor,
        Shoes,
        Helmet
    }

    [SerializeField]
    [Tooltip("Index 0: Weapon | Index 1: Armor | Index 2: Shoes | Index 3: Helmet")]
    private List<Item> listEquipment = new List<Item>(4);

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            listEquipment.Add(null);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Collider[] hitItems = Physics.OverlapSphere(transform.position, getItemRadius, item, QueryTriggerInteraction.Collide);
            if (hitItems != null)
            {
                Item targetItem = hitItems[0].GetComponent<Item>();

                if (targetItem != null)
                {
                    EquipmentType itemType = EquipmentType.Helmet;
                    switch (targetItem.itemType)
                    {
                        case "Weapon":
                            itemType = EquipmentType.Weapon;
                            break;
                        case "Armor":
                            itemType = EquipmentType.Armor;
                            break;
                        case "Shoes":
                            itemType = EquipmentType.Shoes;
                            break;
                    }
                    SetEquipment(targetItem, itemType);
                }
            }
        }
    }

    public void SetEquipment(Item targetItem, EquipmentType typeOfItem)
    {
        if (listEquipment[(int)typeOfItem] != null)
            PopEquipment((int)typeOfItem);

        listEquipment[(int)typeOfItem] = targetItem;
        targetItem.transform.position = new Vector3(transform.position.x, targetItem.transform.position.y, transform.position.z);
        targetItem.transform.parent = transform;
        targetItem.SetItem();
        targetItem.gameObject.SetActive(false);
    }

    private void PopEquipment(int index)
    {
        Item targetItem = listEquipment[index];
        listEquipment[index] = null;

        targetItem.transform.parent = null;
        targetItem.gameObject.SetActive(true);
        targetItem.PopItem();
    }
}
