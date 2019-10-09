using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SkillDetailBox : MonoBehaviour
{
    private SkillIcon parent;
    private new Text name;
    private Text description;

    private void Awake()
    {
        parent = GetComponentInParent<SkillIcon>();
        name = transform.Find("Name").GetComponent<Text>();
        description = transform.Find("Description").GetComponent<Text>();
    }

    void Update()
    {
        UpdateText();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        LivingThing target = UnitControlManager.instance.selectedUnit;
        if (target == null || !target.photonView.IsMine || target.control.skillSet[parent.skillIndex] == null)
        {
            gameObject.SetActive(false);
            return;
        }

        name.text = target.control.skillSet[parent.skillIndex].abilityName;
        description.text = DescriptionSyntax.Decode(target.control.skillSet[parent.skillIndex].abilityDescription);
    }
}
