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
            Collider[] hitItems = Physics.OverlapSphere(transform.position, getItemRadius, item);
            if (hitItems != null)
            {
                Item targetItem = hitItems[0].GetComponent<Item>();

                if (targetItem != null)
                {
                    EquipmentType typeOfItem = EquipmentType.Helmet;
                    switch (targetItem.typeOfItem)
                    {
                        case "Weapon":
                            typeOfItem = EquipmentType.Weapon;
                            break;
                        case "Armor":
                            typeOfItem = EquipmentType.Armor;
                            break;
                        case "Shoes":
                            typeOfItem = EquipmentType.Shoes;
                            break;
                    }
                    SetEquipment(targetItem, typeOfItem);
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
        targetItem.gameObject.SetActive(false);
    }

    private void PopEquipment(int index)
    {
        Item targetItem = listEquipment[index];
        listEquipment[index] = null;

        targetItem.transform.parent = null;
        targetItem.gameObject.SetActive(true);
    }
}
