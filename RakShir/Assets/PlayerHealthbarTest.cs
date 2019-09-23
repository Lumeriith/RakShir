using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHealthbarTest : MonoBehaviour
{
    Image fill;
    public LivingThing target;
    Text text;

    public Vector3 offset;

    public void SetTarget(LivingThing target)
    {
        this.target = target;
    }

    void Start()
    {
        fill = transform.Find("Fill").GetComponent<Image>();
        text = transform.Find("Text").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;
        fill.fillAmount = target.currentHealth / target.maximumHealth;
        text.text = string.Format("{0}/{1}", (int)target.currentHealth, (int)target.maximumHealth);
        Vector3 targetPosition = target.transform.position + Vector3.up;
        transform.position = Camera.main.WorldToScreenPoint(targetPosition) + offset;
    }
}
