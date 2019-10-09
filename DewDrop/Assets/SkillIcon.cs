using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class SkillIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int skillIndex = 1;

    private Image image_icon;
    private Image image_disabled;
    private Image image_highlighted;
    private TextMeshProUGUI tmpu_cooldown;

    private SkillDetailBox detailBox;

    public void OnPointerEnter(PointerEventData data)
    {
        detailBox.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData data)
    {
        detailBox.gameObject.SetActive(false);
    }

    private void Awake()
    {
        image_icon = transform.Find("Mask/Icon Image").GetComponent<Image>();
        image_disabled = transform.Find("Mask/Disabled Image").GetComponent<Image>();
        image_highlighted = transform.Find("Mask/Highlighted Image").GetComponent<Image>();
        tmpu_cooldown = transform.Find("Mask/Cooldown Text").GetComponent<TextMeshProUGUI>();
        detailBox = transform.Find("Detail Box").GetComponent<SkillDetailBox>();
        detailBox.gameObject.SetActive(false);
    }
    private void Update()
    {
        LivingThing target = UnitControlManager.instance.selectedUnit;

        if (target == null || !target.photonView.IsMine)
        {
            image_icon.sprite = null;
            image_disabled.enabled = true;
            image_highlighted.enabled = false;
            tmpu_cooldown.text = "";
            return;
        }

        if(target.control.skillSet[skillIndex] == null)
        {
            image_icon.sprite = null;
            image_disabled.enabled = true;
            image_highlighted.enabled = false;
        }
        else
        {
            image_icon.sprite = target.control.skillSet[skillIndex].abilityIcon ?? null;
            image_disabled.enabled = !target.control.skillSet[skillIndex].isCooledDown;
            image_highlighted.enabled = false;
        }

        tmpu_cooldown.text = target.control.cooldownTime[skillIndex] == 0f ? "" : target.control.cooldownTime[skillIndex].ToString("F1");


    }
}
