using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfobar : MonoBehaviour, IInfobar
{
    private LivingThing target;

    private new Renderer renderer
    {
        get
        {
            if (_renderer == null) _renderer = target.transform.Find("Model").GetComponentInChildren<SkinnedMeshRenderer>();
            return _renderer;
        }
    }
    private Renderer _renderer;

    public Vector3 worldOffset;
    public Vector3 UIOffset;

    public Color nameColor = Color.white;
    public Color statusEffectColor = Color.yellow;

    private Text text_name;
    private UniversalHealthbar universalHealthbar;
    private Image image_mana_fill;
    private Image image_status_effect_backdrop;


    private CanvasGroup canvasGroup;
    public void SetTarget(LivingThing target)
    {
        this.target = target;
    }

    private void Awake()
    {
        text_name = transform.Find("Name").GetComponent<Text>();
        universalHealthbar = GetComponentInChildren<UniversalHealthbar>();
        image_mana_fill = transform.Find("Mana/Fill").GetComponent<Image>();
        image_status_effect_backdrop = transform.Find("Status Effect Backdrop").GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        if (target.IsDead() || !renderer.isVisible)
        {
            canvasGroup.alpha = 0;
            universalHealthbar.enabled = false;
            return;
        }
        else
        {
            canvasGroup.alpha = 1;
            universalHealthbar.enabled = true;
        }
        string statusEffectName = StatusEffect.GetImportantStatusEffectName(target);
        if(statusEffectName == "")
        {
            text_name.text = target.readableName;
            text_name.color = nameColor;
            image_status_effect_backdrop.enabled = false;
        }
        else
        {
            text_name.text = statusEffectName;
            text_name.color = statusEffectColor;
            image_status_effect_backdrop.enabled = true;
        }

        image_mana_fill.fillAmount = target.stat.currentMana / target.stat.finalMaximumMana;

        universalHealthbar.SetTarget(target);


        transform.position = Camera.main.WorldToScreenPoint(target.transform.position + renderer.bounds.size.y * Vector3.up + worldOffset) + UIOffset;


    }
}
