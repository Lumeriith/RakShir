using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OverlayCanvas : MonoBehaviour
{
    public static OverlayCanvas instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<OverlayCanvas>();
            return _instance;
        }
    }
    private static OverlayCanvas _instance;

    private Image image_pain;
    private Image image_damageOverTime;
    private Image image_silence;
    private Image image_stun;
    private Image image_blink;

    private float painAmount = 0f;
    private float damageOverTimeAmount = 0f;
    private float silenceAmount = 0f;
    private float stunAmount = 0f;
    private float blinkAmount = 0f;

    private Entity player = null;

    private void Awake()
    {
        image_pain = transform.Find("Overlay - Pain").GetComponent<Image>();
        image_damageOverTime = transform.Find("Overlay - DamageOverTime").GetComponent<Image>();
        image_silence = transform.Find("Overlay - Silence").GetComponent<Image>();
        image_stun = transform.Find("Overlay - Stun").GetComponent<Image>();
        image_blink = transform.Find("Overlay - Blink").GetComponent<Image>();
    }
    private void Update()
    {
        if (player == null && GameManager.instance.localPlayer != null)
        {
            player = GameManager.instance.localPlayer;
            player.OnTakeDamage += TookDamage;
        }
        if (player == null)
        {
            image_pain.enabled = false;
            return;
        }

        UpdateAmounts();

        image_pain.enabled = true;
        image_pain.color = new Color(1, 1, 1, painAmount);
        image_damageOverTime.color = new Color(1, 1, 1, damageOverTimeAmount);
        image_silence.color = new Color(1, 1, 1, silenceAmount);
        image_stun.color = new Color(1, 1, 1, stunAmount);
        image_blink.color = new Color(0, 0, 0, blinkAmount);
    }
    private void TookDamage(InfoDamage info)
    {
        float damagePercentage = info.damage / player.maximumHealth;
        float currentHealthPercentage = player.currentHealth / player.maximumHealth;
        float painDelta = damagePercentage / currentHealthPercentage * 1.5f;
        if (currentHealthPercentage > 0.5f && painDelta < 0.3f) return;
        painAmount += Mathf.MoveTowards(painAmount, 1f, damagePercentage / currentHealthPercentage * 2f);

    }

    public static void Blink()
    {
        instance.blinkAmount = 1f;
        instance.image_blink.color = new Color(0, 0, 0, instance.blinkAmount);
    }

    private void UpdateAmounts()
    {
        Entity player = GameManager.instance.localPlayer;
        painAmount = Mathf.MoveTowards(painAmount, 0, 0.8f * Time.deltaTime);
        painAmount = Mathf.Clamp(painAmount, Mathf.Clamp(1 - player.currentHealth / player.maximumHealth - 0.6f, 0f, 1f) * 0.65f / 0.4f, 1f);
        damageOverTimeAmount = Mathf.MoveTowards(damageOverTimeAmount, 0f, 4f * Time.deltaTime);
        silenceAmount = Mathf.MoveTowards(silenceAmount, 0f, 4f * Time.deltaTime);
        stunAmount = Mathf.MoveTowards(stunAmount, 0f, 4f * Time.deltaTime);
        blinkAmount = Mathf.MoveTowards(blinkAmount, 0f, 1.75f * Time.deltaTime);
        if (player.IsAffectedBy(StatusEffectType.DamageOverTime)) damageOverTimeAmount = 1f;
        if (player.IsAffectedBy(StatusEffectType.Silence)) silenceAmount = 1f;
        if (player.IsAffectedBy(StatusEffectType.Stun)) stunAmount = 1f;

    }
}
