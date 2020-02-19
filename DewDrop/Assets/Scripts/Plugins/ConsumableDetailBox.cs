using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableDetailBox : MonoBehaviour
{
    private ConsumableIcon parent;
    private new Text name;
    private Text description;

    private PlayerInventory belt;

    private void Awake()
    {
        parent = GetComponentInParent<ConsumableIcon>();
        name = transform.Find("Name").GetComponent<Text>();
        description = transform.Find("Description").GetComponent<Text>();
    }

    void Update()
    {
        UpdateText();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (UnitControlManager.instance.selectedUnit == null) return;
        if(belt ==  null) belt = UnitControlManager.instance.selectedUnit.GetComponent<PlayerInventory>();

        if (belt == null || !UnitControlManager.instance.selectedUnit.photonView.IsMine || belt.consumableBelt[parent.consumableIndex] == null)
        {
            gameObject.SetActive(false);
            return;
        }

        name.text = belt.consumableBelt[parent.consumableIndex].itemName;
        description.text = DescriptionSyntax.Decode(belt.consumableBelt[parent.consumableIndex].itemDescription);
    }
}
