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

    

    private Text text_name;
    private UniversalHealthbar universalHealthbar;
    private Image image_mana_fill;


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

        canvasGroup = GetComponent<CanvasGroup>();

    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        if (target.IsDead())
        {
            canvasGroup.alpha = 0;
        }
        else
        {
            canvasGroup.alpha = 1;
        }

        text_name.text = target.name;
        image_mana_fill.fillAmount = target.stat.currentMana / target.stat.finalMaximumMana;

        universalHealthbar.SetTarget(target);


        transform.position = Camera.main.WorldToScreenPoint(target.transform.position + renderer.bounds.size.y * Vector3.up + worldOffset) + UIOffset;


    }
}
