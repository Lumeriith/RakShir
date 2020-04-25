using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerInfoHUD : MonoBehaviour
{


    private UniversalHealthbar uhb;

    private Image image_mana_fill;


    private TextMeshProUGUI tmpu_health_text;
    private TextMeshProUGUI tmpu_mana_text;
    private TextMeshProUGUI tmpu_gold_text;


    private void Awake()
    {

        image_mana_fill = transform.Find("Mana/Fill").GetComponent<Image>();


        tmpu_health_text = transform.Find("Health/Text").GetComponent<TextMeshProUGUI>();
        tmpu_mana_text = transform.Find("Mana/Text").GetComponent<TextMeshProUGUI>();
        tmpu_gold_text = transform.Find("Gold Text").GetComponent<TextMeshProUGUI>();

        uhb = transform.GetComponentInChildren<UniversalHealthbar>();

    }

    void LateUpdate()
    {
        LivingThing target = UnitControlManager.instance.selectedUnit;
        if (target == null) return;

        uhb.SetTarget(target);

        image_mana_fill.fillAmount = target.stat.currentMana / target.stat.finalMaximumMana;

        tmpu_health_text.text = string.Format("{0:n0}/{1:n0}", (int)target.currentHealth + (int)target.statusEffect.status.shield, (int)target.maximumHealth);
        tmpu_mana_text.text = string.Format("{0:n0}/{1:n0}", (int)target.stat.currentMana, (int)target.stat.finalMaximumMana);
        tmpu_gold_text.text = string.Format("{0:n0}", (int)target.stat.currentGold);
    }
}
