using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class TooltipAbilityGroup : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Required References")]
    private Image _abilityIcon;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _abilityName;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _abilitySubtitle;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _abilityDescription;
    [SerializeField, FoldoutGroup("Required References")]
    private GameObject _gemInfoPrefab;
    [SerializeField, FoldoutGroup("Required References")]
    private Transform _gemInfoParent;

    public void Setup(int index)
    {
        if(GameManager.instance.localPlayer == null || GameManager.instance.localPlayer.control.skillSet[index] == null)
        {
            _abilityIcon.enabled = false;
            _abilityName.text = "비어있는 기술";
            _abilitySubtitle.text = "장비를 장착하여 기술을 습득할 수 있습니다.";
            _abilityDescription.text = "";
            _abilityDescription.gameObject.SetActive(false);
            _gemInfoParent.gameObject.SetActive(false);

            foreach(Transform t in _gemInfoParent)
            {
                Destroy(t);
            }
        }
        else
        {
            AbilityTrigger trigger = GameManager.instance.localPlayer.control.skillSet[index];
            _abilityIcon.enabled = true;
            _abilityIcon.sprite = trigger.abilityIcon;
            _abilityName.text = trigger.abilityName;
            
            if(trigger.cooldownTime > 0 && trigger.manaCost > 0)
            {
                _abilitySubtitle.text = string.Format("재사용 대기시간 {0}초, 마나 소모량 {1}", trigger.cooldownTime, trigger.manaCost);
            }
            else if (trigger.cooldownTime > 0)
            {
                _abilitySubtitle.text = string.Format("재사용 대기시간 {0}초", trigger.cooldownTime);
            }
            else if (trigger.manaCost > 0)
            {
                _abilitySubtitle.text = string.Format("마나 소모량 {0}", trigger.manaCost);
            }
            else
            {
                _abilitySubtitle.text = "";
            }

            _abilityDescription.text = DescriptionSyntax.Decode(trigger.abilityDescription);
            _abilityDescription.gameObject.SetActive(true);

            foreach (Transform t in _gemInfoParent)
            {
                Destroy(t.gameObject);
            }

            _gemInfoParent.gameObject.SetActive(trigger.connectedGems.Count > 0);

            for (int i = 0; i < trigger.connectedGems.Count; i++)
            {
                Instantiate(_gemInfoPrefab, _gemInfoParent).GetComponent<TooltipAbilityGroupGemInfo>().Setup(trigger.connectedGems[i]);
            }
        }
    }

}
