using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerInfoHUD : MonoBehaviour
{


    private Image image_health_fill;
    private Image image_health_delta;
    private Image image_mana_fill;

    private Image image_health_HoT;
    private Image image_health_DoT;

    private TextMeshProUGUI tmpu_health_text;
    private TextMeshProUGUI tmpu_mana_text;



    private void Awake()
    {
        image_health_fill = transform.Find("Health/Fill").GetComponent<Image>();
        image_health_delta = transform.Find("Health/Delta").GetComponent<Image>();
        image_mana_fill = transform.Find("Mana/Fill").GetComponent<Image>();
        image_health_HoT = transform.Find("Health/HoT").GetComponent<Image>();
        image_health_DoT = transform.Find("Health/DoT").GetComponent<Image>();

        tmpu_health_text = transform.Find("Health/Text").GetComponent<TextMeshProUGUI>();
        tmpu_mana_text = transform.Find("Mana/Text").GetComponent<TextMeshProUGUI>();

    }

    void LateUpdate()
    {
        LivingThing target = UnitControlManager.instance.selectedUnit;
        if (target == null) return;

        image_health_fill.fillAmount = target.currentHealth / target.maximumHealth;
        image_mana_fill.fillAmount = target.stat.currentMana / target.stat.finalMaximumMana;

        if (image_health_fill.fillAmount > image_health_delta.fillAmount) image_health_delta.fillAmount = image_health_fill.fillAmount;
        image_health_delta.fillAmount = Mathf.MoveTowards(image_health_delta.fillAmount, image_health_fill.fillAmount, 0.3f * Time.deltaTime);

        if (target.statusEffect.totalDamageOverTimeAmount > target.statusEffect.totalHealOverTimeAmount)
        {
            image_health_fill.fillAmount -= (target.statusEffect.totalDamageOverTimeAmount - target.statusEffect.totalHealOverTimeAmount) / target.maximumHealth;
            image_health_DoT.fillAmount = Mathf.Clamp(image_health_fill.fillAmount + (target.statusEffect.totalDamageOverTimeAmount - target.statusEffect.totalHealOverTimeAmount) / target.maximumHealth, 0f, target.currentHealth / target.maximumHealth);
            image_health_HoT.fillAmount = 0f;
        }
        else
        {
            image_health_DoT.fillAmount = 0f;
            image_health_HoT.fillAmount = image_health_fill.fillAmount + (target.statusEffect.totalHealOverTimeAmount - target.statusEffect.totalDamageOverTimeAmount) / target.maximumHealth;
        }

        tmpu_health_text.text = string.Format("{0}/{1}", (int)target.currentHealth, (int)target.maximumHealth);
        tmpu_mana_text.text = string.Format("{0}/{1}", (int)target.stat.currentMana, (int)target.stat.finalMaximumMana);
    }
}
