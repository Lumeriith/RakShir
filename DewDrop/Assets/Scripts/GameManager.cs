using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

public enum PlayerType { Elemental, Reptile }

public class GameManager : MonoBehaviour
{
    public Texture2D normalCursor;
    public Vector2 normalCursorHotspot;
    public Texture2D attackCursor;
    public Vector2 attackCursorHotspot;
    
    public enum CursorShapeType { None, Normal, Attack, AbilityInstance }
    public CursorShapeType cursorShape = CursorShapeType.Normal;
    private CursorShapeType lastCursorShape;

    public LivingThing localPlayer
    {
        get
        {
            return _localPlayer;
        }
        set
        {
            _localPlayer = value;
        }
    }

    private LivingThing _localPlayer;
    private Transform floatingTextCanvas;

    public FloatingText magicDamageFloatingText;
    public FloatingText physicalDamageFloatingText;
    public FloatingText healFloatingText;
    public FloatingText manaHealFloatingText;
    public FloatingText goldFloatingText;


    public System.Action<LivingThing> OnLivingThingInstantiate = (LivingThing _) => { };
    public System.Action<Activatable> OnActivatableInstantiate = (Activatable _) => { };

    public float goldModifier = 1.0f;

    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    public void RegisterFloatingTextEvents(LivingThing player)
    {
        player.OnDoBasicAttackHit += (InfoBasicAttackHit info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(physicalDamageFloatingText, floatingTextCanvas).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.damage).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnDealMagicDamage += (InfoMagicDamage info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(magicDamageFloatingText, floatingTextCanvas).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalDamage).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnTakeHeal += (InfoHeal info) =>
        {
            if (info.to == info.from) return;
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(healFloatingText, floatingTextCanvas).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalHeal).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnDoHeal += (InfoHeal info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(healFloatingText, floatingTextCanvas).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalHeal).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnTakeManaHeal += (InfoManaHeal info) =>
        {
            if (info.to == info.from) return;
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(manaHealFloatingText, floatingTextCanvas).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalManaHeal).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnDoManaHeal += (InfoManaHeal info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();

            FloatingText floatingText = Instantiate(manaHealFloatingText, floatingTextCanvas).GetComponent<FloatingText>();

            floatingText.text = Mathf.Ceil(info.finalManaHeal).ToString();
            floatingText.worldPosition = worldPos;
        };

        player.OnTakeGold += (InfoGold info) =>
        {
            Vector3 worldPos = info.from.transform.position + info.from.GetRandomOffset();

            FloatingText floatingText = Instantiate(goldFloatingText, floatingTextCanvas).GetComponent<FloatingText>();

            floatingText.text = "+" + Mathf.Ceil(info.amount).ToString() + "G";
            floatingText.worldPosition = worldPos;
        };

    }

    public void SpawnLocalPlayer(PlayerType type, Vector3 location)
    {
        LivingThing localPlayer;
        List<Activatable> startItems = new List<Activatable>();
        if(type == PlayerType.Elemental)
        {
            localPlayer = PhotonNetwork.Instantiate("player_Elemental", location, Quaternion.identity).GetComponent<LivingThing>();
            startItems.Add(PhotonNetwork.Instantiate("equip_Armor_ElementalIntegrity", location, Quaternion.identity).GetComponent<Activatable>());
            startItems.Add(PhotonNetwork.Instantiate("equip_Boots_ElementalDetermination", location, Quaternion.identity).GetComponent<Activatable>());
            startItems.Add(PhotonNetwork.Instantiate("equip_Weapon_ElementalJustice", location, Quaternion.identity).GetComponent<Activatable>());
            AvatarManager.instance.SetAvatar(PlayerType.Elemental);
        }
        else
        {
            localPlayer = PhotonNetwork.Instantiate("player_Reptile", location, Quaternion.identity).GetComponent<LivingThing>();
            startItems.Add(PhotonNetwork.Instantiate("equip_Armor_ReptileSkin", location, Quaternion.identity).GetComponent<Activatable>());
            startItems.Add(PhotonNetwork.Instantiate("equip_Boots_ReptileFeet", location, Quaternion.identity).GetComponent<Activatable>());
            startItems.Add(PhotonNetwork.Instantiate("equip_Weapon_ReptileClaw", location, Quaternion.identity).GetComponent<Activatable>());
            AvatarManager.instance.SetAvatar(PlayerType.Reptile);
        }
        
        this.localPlayer = localPlayer;
        foreach(Activatable item in startItems)
        {
            localPlayer.ActivateImmediately(item);
        }

        UnitControlManager.instance.selectedUnit = localPlayer;
        RegisterFloatingTextEvents(localPlayer);
    }




    private void Awake()
    {
        floatingTextCanvas = transform.Find("/Common Game Logics/Floating Text Canvas");
    }

    private void Start()
    {
        OnLivingThingInstantiate += (LivingThing lv) =>
        {
            if (lv.photonView.IsMine)
            {
                lv.OnDeath += (InfoDeath info) =>
                {
                    info.victim.GiveGold(info.victim.droppedGold * GameManager.instance.goldModifier, info.killer);
                };
            }
        };
    }

    private void Update()
    {
        if(lastCursorShape != cursorShape)
        {
            SetAppropriateCursor();
            lastCursorShape = cursorShape;
        }
    }

    private void SetAppropriateCursor()
    {
        switch (cursorShape)
        {
            case CursorShapeType.None:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;
            case CursorShapeType.Normal:
                Cursor.SetCursor(normalCursor, normalCursorHotspot, CursorMode.Auto);
                break;
            case CursorShapeType.Attack:
                Cursor.SetCursor(attackCursor, attackCursorHotspot, CursorMode.Auto);
                break;
            case CursorShapeType.AbilityInstance:
                Cursor.SetCursor(attackCursor, attackCursorHotspot, CursorMode.Auto);
                break;
        }
    }

    

}
