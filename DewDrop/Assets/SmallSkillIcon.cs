using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmallSkillIcon : MonoBehaviour
{
    public int index;

    private GameObject mask;
    private Image iconImage;

    private AbilityTrigger currentTrigger;

    private void Awake()
    {
        mask = transform.Find("Mask").gameObject;
        iconImage = transform.Find("Mask/Icon Image").GetComponent<Image>();
        mask.SetActive(false);
    }

    private void Update()
    {
        if (UnitControlManager.instance.selectedUnit == null) return;
        if (UnitControlManager.instance.selectedUnit.control.skillSet.Length <= index) currentTrigger = null;
        if (UnitControlManager.instance.selectedUnit.control.skillSet[index] != currentTrigger ||
            (UnitControlManager.instance.selectedUnit.control.skillSet[index] != null && UnitControlManager.instance.selectedUnit.control.skillSet[index].abilityIcon != iconImage.sprite))
        {
            currentTrigger = UnitControlManager.instance.selectedUnit.control.skillSet[index];
            if(currentTrigger == null)
            {
                mask.SetActive(false);
            }
            else
            {
                mask.SetActive(true);
                iconImage.sprite = currentTrigger.abilityIcon;
            }
        }
    }
}
