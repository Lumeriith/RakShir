using UnityEngine;

public class InfoTextCanvas : MonoBehaviour
{
    public InfoText consumableInfoText;
    public InfoText equipmentInfoText;
    public InfoText obeliskInfoText;
    public InfoText moonPortalInfoText;
    public InfoText gemInfoText;

    private void Start()
    {
        GameManager.instance.OnActivatableInstantiate += (Activatable activatable) =>
        {
            Consumable cons = activatable as Consumable;
            Equipment equip = activatable as Equipment;
            Gem gem = activatable as Gem;
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
            else if (gem != null)
            {
                InfoText text = Instantiate(gemInfoText, transform).GetComponent<InfoText>();
                text.text = gem.itemName;
                text.follow = gem.transform;
                text.OnClick += () =>
                {
                    UnitControlManager.instance.selectedUnit.control.CommandActivate(gem, Input.GetKey(UnitControlManager.instance.reservationModifier));
                    Instantiate(UnitControlManager.instance.commandMarkerInterest, gem.transform.position, Quaternion.identity, equip.transform);
                };
            }
            else if (activatable as MoonPortal != null)
            {
                InfoText text = Instantiate(moonPortalInfoText, transform).GetComponent<InfoText>();
                text.follow = activatable.transform;
                text.OnClick += () =>
                {
                    UnitControlManager.instance.selectedUnit.control.CommandActivate(activatable, Input.GetKey(UnitControlManager.instance.reservationModifier));
                    Instantiate(UnitControlManager.instance.commandMarkerInterest, activatable.transform.position, Quaternion.identity, activatable.transform);
                };
            }
            else if (activatable as Portal != null)
            {
                InfoText text = Instantiate(obeliskInfoText, transform).GetComponent<InfoText>();
                text.follow = activatable.transform;
                text.OnClick += () =>
                {
                    UnitControlManager.instance.selectedUnit.control.CommandActivate(activatable, Input.GetKey(UnitControlManager.instance.reservationModifier));
                    Instantiate(UnitControlManager.instance.commandMarkerInterest, activatable.transform.position, Quaternion.identity, activatable.transform);
                };
            }
        };
    }
}
