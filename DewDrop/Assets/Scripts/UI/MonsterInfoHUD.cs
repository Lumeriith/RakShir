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

    private Text text_statusEffects;
    private Image image_statusEffectsBackdrop;

    private RectTransform rectTransform;

    public float minimumWidth = 200f;
    public float maximumWidth = 1000f;

    public Color statusEffectsBackdropColorA;
    public Color statusEffectsBackdropColorB;

    public float sineTimeMultiplier = 5f;

    private void Awake()
    {
        text_name = transform.Find("Name").GetComponent<Text>();
        tmpu_health = transform.Find("Health/Text").GetComponent<TextMeshProUGUI>();
        uhb = GetComponentInChildren<UniversalHealthbar>();
        group = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        text_statusEffects = transform.Find("StatusEffects").GetComponent<Text>();
        image_statusEffectsBackdrop = transform.Find("StatusEffects Backdrop").GetComponent<Image>();
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
            tmpu_health.text = string.Format("{0:n0}/{1:n0}", (int)target.currentHealth, (int)target.maximumHealth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Clamp(target.maximumHealth, minimumWidth, maximumWidth));
            string statuses = StatusEffect.GetImportantStatusEffectNames(target);
            if(statuses == "")
            {
                image_statusEffectsBackdrop.enabled = false;
                text_statusEffects.text = "";
            }
            else
            {
                image_statusEffectsBackdrop.enabled = true;
                text_statusEffects.text = statuses;
                image_statusEffectsBackdrop.color = Color.Lerp(statusEffectsBackdropColorA, statusEffectsBackdropColorB, Mathf.Sin(Time.time * sineTimeMultiplier) / 2f + 0.5f);
            }
        }

        tmpu_health.enabled = text_statusEffects.text == "";
    }


    private void SetTarget(InfoDamage info)
    {
        if(info.to != info.from) target = info.to;
    }

}
