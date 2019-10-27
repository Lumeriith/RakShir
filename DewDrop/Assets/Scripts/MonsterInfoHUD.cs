using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterInfoHUD : MonoBehaviour
{
    private Text text_name;
    private TextMeshProUGUI tmpu_health;
    private UniversalHealthbar uhb;
    private CanvasGroup group;

    private LivingThing player;
    private LivingThing target;

    private RectTransform rectTransform;

    public float minimumWidth = 200f;
    public float maximumWidth = 1000f;

    private void Awake()
    {
        text_name = transform.Find("Name").GetComponent<Text>();
        tmpu_health = transform.Find("Health/Text").GetComponent<TextMeshProUGUI>();
        uhb = GetComponentInChildren<UniversalHealthbar>();
        group = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(player == null && GameManager.instance.localPlayer != null)
        {
            player = GameManager.instance.localPlayer;
            player.OnDealDamage += SetTarget;
        }

        if (target != null && target.IsDead()) target = null;


        if(target == null)
        {
            group.alpha = 0f;
        }
        else
        {
            group.alpha = 1f;
            uhb.SetTarget(target);
            text_name.text = target.readableName;
            tmpu_health.text = string.Format("{0}/{1}", (int)target.currentHealth, (int)target.maximumHealth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Clamp(target.maximumHealth, minimumWidth, maximumWidth));
        }
    }


    private void SetTarget(InfoDamage info)
    {
        if(info.to != info.from) target = info.to;
    }

}
