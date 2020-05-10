using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class ConsumableIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int consumableIndex = 1;

    [SerializeField, FoldoutGroup("Required References")]
    private Image _iconImage;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _disabledOverlay;

    private PlayerInventory _inventory;

    private bool _isTooltipShown;

    public void OnPointerEnter(PointerEventData data)
    {
        if (_inventory.consumableBelt[consumableIndex] != null)
        {
            TooltipInfo.instance.Show(_inventory.consumableBelt[consumableIndex]);
            _isTooltipShown = true;
        }
        
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
        if (UnitControlManager.instance.selectedUnit == null) return;
        if (_inventory == null) _inventory = GameManager.instance.localPlayer.GetComponent<PlayerInventory>();
        
        if (_inventory == null || !GameManager.instance.localPlayer.photonView.IsMine)
        {
            _iconImage.sprite = null;
            _disabledOverlay.enabled = true;
            return;
        }

        if (_inventory.consumableBelt[consumableIndex] == null)
        {
            _iconImage.sprite = null;
            _iconImage.enabled = false;
            _disabledOverlay.enabled = false;
        }
        else
        {
            _iconImage.sprite = _inventory.consumableBelt[consumableIndex].itemIcon;
            _iconImage.color = new Color(1f, 1f, 1f);
            _iconImage.enabled = true;
            _disabledOverlay.enabled = !(_inventory.consumableBelt[consumableIndex].selfValidator.Evaluate(GameManager.instance.localPlayer) && _inventory.consumableBelt[consumableIndex].IsReady());
        }



    }
}
