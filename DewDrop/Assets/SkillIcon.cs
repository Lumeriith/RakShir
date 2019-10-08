using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SkillIcon : MonoBehaviour
{
    public int skillIndex = 1;

    private Image image_icon;
    private Image image_disabled;
    private Image image_highlighted;
    private TextMeshProUGUI tmpu_cooldown;

    private void Awake()
    {
        image_icon = transform.Find("Mask/Icon Image").GetComponent<Image>();
        image_disabled = transform.Find("Mask/Disabled Image").GetComponent<Image>();
        image_highlighted = transform.Find("Mask/Highlighted Image").GetComponent<Image>();
        tmpu_cooldown = transform.Find("Mask/Cooldown Text").GetComponent<TextMeshProUGUI>();
    }
    private void Update()
    {
        LivingThing target = UnitControlManager.instance.selectedUnit;
        if (target == null || !target.photonView.IsMine || target.control.skillSet[skillIndex] == null)
        {
            image_icon.sprite = null;
            image_disabled.enabled = true;
            image_highlighted.enabled = false;
            tmpu_cooldown.text = "";
            return;
        }

        image_icon.sprite = target.control.skillSet[skillIndex].abilityIcon ?? null;
        image_disabled.enabled = !target.control.skillSet[skillIndex].isCooledDown;
        image_highlighted.enabled = false;
        tmpu_cooldown.text = target.control.skillSet[skillIndex].remainingCooldownTime == 0 ? "" : target.control.skillSet[skillIndex].remainingCooldownTime.ToString("F1");


    }
}
