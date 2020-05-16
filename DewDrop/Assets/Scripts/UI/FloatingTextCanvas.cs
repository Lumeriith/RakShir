using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class FloatingTextCanvas : MonoBehaviour
{
    [Title("Floating Text Prefab Assignments")]
    [SerializeField]
    private GameObject _magicDamage;
    [SerializeField]
    private GameObject _physicalDamage;
    [SerializeField]
    private GameObject _heal;
    [SerializeField]
    private GameObject _manaHeal;
    [SerializeField]
    private GameObject _gold;
    [SerializeField]
    private GameObject _pureDamage;
    [SerializeField]
    private GameObject _dodge;
    [SerializeField]
    private GameObject _miss;


    public static FloatingTextCanvas instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<FloatingTextCanvas>();
            return _instance;
        }
    }
    private static FloatingTextCanvas _instance;

    private void Awake()
    {
        GameManager.instance.OnLivingThingInstantiate += (Entity entity) => { if (entity.type == LivingThingType.Player && entity.photonView.IsMine) RegisterFloatingTextEvents(entity); };
    }

    public void SpawnFloatingTextAtEntity(GameObject original, string text, Entity entity)
    {
        Vector3 worldPos = entity.transform.position + entity.GetCenterOffset();
        SpawnFloatingText(original, text, worldPos);
    }

    public void SpawnFloatingTextAtEntity(GameObject original, Entity entity)
    {
        Vector3 worldPos = entity.transform.position + entity.GetCenterOffset();
        SpawnFloatingText(original, worldPos);
    }



    public void SpawnFloatingText(GameObject original, string text, Vector3 worldPosition)
    {
        FloatingText newText = PoolManager.Spawn(original, Vector3.zero, Quaternion.identity, transform).GetComponent<FloatingText>();
        newText.Setup(worldPosition, text);
    }

    public void SpawnFloatingText(GameObject original, Vector3 worldPosition)
    {
        FloatingText newText = PoolManager.Spawn(original, Vector3.zero, Quaternion.identity, transform).GetComponent<FloatingText>();
        newText.Setup(worldPosition);
    }



    public void RegisterFloatingTextEvents(Entity player)
    {
        player.OnDoBasicAttackHit += (InfoBasicAttackHit info) =>
        {
            SpawnFloatingTextAtEntity(_physicalDamage, Mathf.Ceil(info.damage).ToString(), info.to);
        };

        player.OnTakeBasicAttackHit += (InfoBasicAttackHit info) =>
        {
            if (info.from == info.to) return;
            SpawnFloatingTextAtEntity(_physicalDamage, Mathf.Ceil(info.damage).ToString(), info.to);
        };

        player.OnDealMagicDamage += (InfoMagicDamage info) =>
        {
            SpawnFloatingTextAtEntity(_magicDamage, Mathf.Ceil(info.finalDamage).ToString(), info.to);
        };

        player.OnTakeMagicDamage += (InfoMagicDamage info) =>
        {
            if (info.from == info.to) return;
            SpawnFloatingTextAtEntity(_magicDamage, Mathf.Ceil(info.finalDamage).ToString(), info.to);
        };

        player.OnDealPureDamage += (InfoDamage info) =>
        {
            SpawnFloatingTextAtEntity(_pureDamage, Mathf.Ceil(info.damage).ToString(), info.to);
        };

        player.OnTakePureDamage += (InfoDamage info) =>
        {
            if (info.from == info.to) return;
            SpawnFloatingTextAtEntity(_pureDamage, Mathf.Ceil(info.damage).ToString(), info.to);
        };

        player.OnDoHeal += (InfoHeal info) =>
        {
            SpawnFloatingTextAtEntity(_heal, Mathf.Ceil(info.finalHeal).ToString(), info.to) ;
        };

        player.OnTakeHeal += (InfoHeal info) =>
        {
            if (info.from == info.to) return;
            SpawnFloatingTextAtEntity(_heal, Mathf.Ceil(info.finalHeal).ToString(), info.to);
        };

        player.OnDoManaHeal += (InfoManaHeal info) =>
        {
            SpawnFloatingTextAtEntity(_manaHeal, Mathf.Ceil(info.finalManaHeal).ToString(), info.to);
        };

        player.OnTakeManaHeal += (InfoManaHeal info) =>
        {
            if (info.from == info.to) return;
            SpawnFloatingTextAtEntity(_manaHeal, Mathf.Ceil(info.finalManaHeal).ToString(), info.to);
        };

        player.OnTakeGold += (InfoGold info) =>
        {
            SpawnFloatingTextAtEntity(_gold, Mathf.Ceil(info.amount).ToString(), info.from);
        };

        player.OnDodge += (InfoMiss info) =>
        {
            SpawnFloatingTextAtEntity(_dodge, info.to);
        };

        player.OnMiss += (InfoMiss info) =>
        {
            SpawnFloatingTextAtEntity(_miss, info.to);
        };





    }
}
