using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ConsumableIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int consumableIndex = 1;

    private Image image_icon;
    private Image image_disabled;
    private ConsumableDetailBox detailBox;

    private PlayerItemBelt belt;

    public void OnPointerEnter(PointerEventData data)
    {
        detailBox.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData data)
    {
        detailBox.gameObject.SetActive(false);
    }

    private void Awake()
    {
        image_icon = transform.Find("Mask/Icon Image").GetComponent<Image>();
        image_disabled = transform.Find("Mask/Disabled Image").GetComponent<Image>();
        detailBox = transform.Find("Detail Box").GetComponent<ConsumableDetailBox>();
        detailBox.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (UnitControlManager.instance.selectedUnit == null) return;
        if (belt == null) belt = GameManager.instance.localPlayer.GetComponent<PlayerItemBelt>();
        
        if (belt == null || !GameManager.instance.localPlayer.photonView.IsMine)
        {
            image_icon.sprite = null;
            image_disabled.enabled = true;
            return;
        }

        if (belt.consumableBelt[consumableIndex] == null)
        {
            image_icon.sprite = null;
            image_disabled.enabled = true;
            image_icon.color = new Color(0.3f, 0.3f, 0.3f);
        }
        else
        {
            image_icon.sprite = belt.consumableBelt[consumableIndex].itemIcon;
            image_icon.color = new Color(1f, 1f, 1f);
            image_disabled.enabled = !(belt.consumableBelt[consumableIndex].selfValidator.Evaluate(GameManager.instance.localPlayer) && belt.consumableBelt[consumableIndex].IsReady());
        }



    }
}
