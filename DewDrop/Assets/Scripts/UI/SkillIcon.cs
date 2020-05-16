using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class SkillIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, FoldoutGroup("Required References")]
    private Image _icon;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _disabledOverlay;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _cooldownText;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _cooldownFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _wrappingSpecialFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _verticalSpecialFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image[] _gemImages;

    public int skillIndex = 1;

    private bool _isTooltipShown;

    public void OnPointerEnter(PointerEventData data)
    {
        TooltipInfo.instance.Show(skillIndex);
        _isTooltipShown = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (_isTooltipShown)
        {
            _isTooltipShown = false;
            TooltipInfo.instance.Hide();
        }
    }

    private void OnDisable()
    {
        if (_isTooltipShown)
        {
            _isTooltipShown = false;
            TooltipInfo.instance.Hide();
        }
    }

    private void Update()
    {
        Entity target = UnitControlManager.instance.selectedUnit;

        if (target == null || !target.photonView.IsMine)
        {
            _icon.sprite = null;
            _disabledOverlay.enabled = true;
            _cooldownText.text = "";
            _cooldownFill.fillAmount = 0f;
            for(int i = 0; i < _gemImages.Length; i++)
            {
                _gemImages[i].enabled = false;
            }
            return;
        }

        if(target.control.skillSet[skillIndex] == null)
        {
            _icon.sprite = null;
            _icon.enabled = false;
            _disabledOverlay.enabled = false;
            _wrappingSpecialFill.fillAmount = 0f;
            _verticalSpecialFill.fillAmount = 0f;
            _cooldownFill.fillAmount = 0f;
            for (int i = 0; i < _gemImages.Length; i++)
            {
                _gemImages[i].enabled = false;
            }
        }
        else
        {
            _icon.color = new Color(1f, 1f, 1f);
            _icon.enabled = true;
            _icon.sprite = target.control.skillSet[skillIndex].abilityIcon ?? null;
            _disabledOverlay.enabled = !target.control.skillSet[skillIndex].isCooledDown || !target.control.skillSet[skillIndex].IsReady() || !target.control.skillSet[skillIndex].selfValidator.Evaluate(target) || !target.HasMana(target.control.skillSet[skillIndex].manaCost);
            if (skillIndex == 0) _disabledOverlay.enabled = false;
            _wrappingSpecialFill.fillAmount = target.control.skillSet[skillIndex].GetSpecialFillAmount();
            _verticalSpecialFill.fillAmount = target.control.skillSet[skillIndex].GetSpecialFillAmount();

            if (skillIndex == 0)
            {
                _cooldownFill.fillAmount = 0f;
            }
            else
            {
                _cooldownFill.fillAmount = target.control.skillSet[skillIndex].cooldownTime == 0 ? 0 : target.control.cooldownTime[skillIndex] / target.control.skillSet[skillIndex].cooldownTime;
            }
            
            

            for (int i = 0; i < _gemImages.Length; i++)
            {
                if(target.control.skillSet[skillIndex].connectedGems.Count <= i) _gemImages[i].enabled = false;
                else
                {
                    _gemImages[i].enabled = true;
                    _gemImages[i].sprite = target.control.skillSet[skillIndex].connectedGems[i].itemIcon;
                }
            }
        }

        if(target.control.cooldownTime[skillIndex] == 0f || skillIndex == 0)
        {
            _cooldownText.text = "";
        }
        else
        {
            if(target.control.cooldownTime[skillIndex] > 1)
            {
                _cooldownText.text = ((int)target.control.cooldownTime[skillIndex] + 1).ToString();
            }
            else
            {
                _cooldownText.text = target.control.cooldownTime[skillIndex].ToString("F1");
            }
            
        }
    }
}
