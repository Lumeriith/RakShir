using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class TooltipGenericItemGroup : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Required References")]
    private Image _itemIcon;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _itemName;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _itemSubtitle;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _itemDescription;
    [SerializeField, FoldoutGroup("Required References")]
    private Transform _skillIconsParent;
    [SerializeField, FoldoutGroup("Required References")]
    private GameObject _skillIconPrefab;

    public void Setup(Item item)
    {
        foreach (Transform t in _skillIconsParent)
        {
            Destroy(t.gameObject);
        }

        _itemIcon.sprite = item.itemIcon;
        _itemName.text = item.itemName;
        if (item.itemTier == ItemTier.Common) _itemSubtitle.text = "일반 ";
        else if (item.itemTier == ItemTier.Rare) _itemSubtitle.text = "희귀 ";
        else if (item.itemTier == ItemTier.Epic) _itemSubtitle.text = "영웅 ";
        else if (item.itemTier == ItemTier.Legendary) _itemSubtitle.text = "전설 ";
        else _itemSubtitle.text = "";

        if (item is Equipment equipment)
        {
            if (equipment.type == EquipmentType.Armor) _itemSubtitle.text += "갑옷";
            else if (equipment.type == EquipmentType.Boots) _itemSubtitle.text += "신발";
            else if (equipment.type == EquipmentType.Helmet) _itemSubtitle.text += "투구";
            else if (equipment.type == EquipmentType.Ring) _itemSubtitle.text += "반지";
            else if (equipment.type == EquipmentType.Weapon) _itemSubtitle.text += "무기";
            else _itemSubtitle.text += "장비";

            for (int i = 0; i < equipment.skillSetReplacements.Length; i++)
            {
                if (equipment.skillSetReplacements[i] != null)
                {
                    Instantiate(_skillIconPrefab, _skillIconsParent).GetComponent<Image>().sprite = equipment.skillSetReplacements[i].abilityIcon;
                }
            }
        }
        else if (item is Consumable) _itemSubtitle.text += "소모품";

        _itemDescription.text = DescriptionSyntax.Decode(item.itemDescription, item);
    }
}
