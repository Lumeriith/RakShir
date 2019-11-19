using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoTextCanvas : MonoBehaviour
{
    public InfoText consumableInfoText;
    public InfoText equipmentInfoText;
    public InfoText obeliskInfoText;
    public InfoText moonPortalInfoText;

    private void Start()
    {
        GameManager.instance.OnActivatableInstantiate += (Activatable activatable) =>
        {
            Consumable cons = activatable as Consumable;
            Equipment equip = activatable as Equipment;
            if (cons != null)
            {
                InfoText text = Instantiate(consumableInfoText, transform).GetComponent<InfoText>();
                text.text = cons.itemName;
                text.follow = cons.transform;
                text.OnClick += () =>
                {
                    UnitControlManager.instance.selectedUnit.control.CommandActivate(cons, Input.GetKey(UnitControlManager.instance.reservationModifier));
                    Instantiate(UnitControlManager.instance.commandMarkerInterest, cons.transform.position, Quaternion.identity, cons.transform);
                };
            }
            else if (equip != null)
            {
                InfoText text = Instantiate(equipmentInfoText, transform).GetComponent<InfoText>();
                text.text = equip.itemName;
                text.follow = equip.transform;
                text.OnClick += () =>
                {
                    UnitControlManager.instance.selectedUnit.control.CommandActivate(equip, Input.GetKey(UnitControlManager.instance.reservationModifier));
                    Instantiate(UnitControlManager.instance.commandMarkerInterest, equip.transform.position, Quaternion.identity, equip.transform);
                };
            }
            else if (activatable as MoonPortal != null)
            {
                InfoText text = Instantiate(moonPortalInfoText, transform).GetComponent<InfoText>();
                text.follow = activatable.transform;
                text.OnClick += () =>
                {
                    UnitControlManager.instance.selectedUnit.control.CommandActivate(activatable, Input.GetKey(UnitControlManager.instance.reservationModifier));
                    Instantiate(UnitControlManager.instance.commandMarkerInterest, activatable.transform.position, Quaternion.identity, equip.transform);
                };
            }
            else if (activatable as Portal != null)
            {
                InfoText text = Instantiate(obeliskInfoText, transform).GetComponent<InfoText>();
                text.follow = activatable.transform;
                text.OnClick += () =>
                {
                    UnitControlManager.instance.selectedUnit.control.CommandActivate(activatable, Input.GetKey(UnitControlManager.instance.reservationModifier));
                    Instantiate(UnitControlManager.instance.commandMarkerInterest, activatable.transform.position, Quaternion.identity, equip.transform);
                };
            }
        };
    }
}
