using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextCanvas : MonoBehaviour
{
    public FloatingText magicDamageFloatingText;
    public FloatingText physicalDamageFloatingText;
    public FloatingText healFloatingText;
    public FloatingText manaHealFloatingText;
    public FloatingText goldFloatingText;

    private static FloatingTextCanvas _instance;
    public static FloatingTextCanvas instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FloatingTextCanvas>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        GameManager.instance.OnLivingThingInstantiate += (LivingThing thing) =>
        {
            if (thing.type == LivingThingType.Player && thing.photonView.IsMine) RegisterFloatingTextEvents(thing);
        };
    }

    public void RegisterFloatingTextEvents(LivingThing player)
    {
        player.OnDoBasicAttackHit += (InfoBasicAttackHit info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(physicalDamageFloatingText, transform).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.damage).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnDealMagicDamage += (InfoMagicDamage info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(magicDamageFloatingText, transform).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalDamage).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnTakeHeal += (InfoHeal info) =>
        {
            if (info.to == info.from) return;
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(healFloatingText, transform).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalHeal).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnDoHeal += (InfoHeal info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(healFloatingText, transform).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalHeal).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnTakeManaHeal += (InfoManaHeal info) =>
        {
            if (info.to == info.from) return;
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(manaHealFloatingText, transform).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalManaHeal).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnDoManaHeal += (InfoManaHeal info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(manaHealFloatingText, transform).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalManaHeal).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnTakeGold += (InfoGold info) =>
        {
            Vector3 worldPos = info.from.transform.position + info.from.GetRandomOffset();

            FloatingText floatingText = Instantiate(goldFloatingText, transform).GetComponent<FloatingText>();

            floatingText.text = "+" + Mathf.Ceil(info.amount).ToString() + "G";
            floatingText.worldPosition = worldPos;
        };

    }
}
