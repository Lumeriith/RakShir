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


    public FloatingText magicDamageFloatingText;
    public FloatingText physicalDamageFloatingText;

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
        player.OnDoBasicAttackHit = (InfoBasicAttackHit info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();
            GameObject floatingText = Instantiate(physicalDamageFloatingText.gameObject, worldPos, Quaternion.identity);
            floatingText.SetActive(true);
            floatingText.GetComponent<FloatingText>().text = Mathf.Ceil(info.damage).ToString();

        };

        player.OnDealMagicDamage = (InfoMagicDamage info) =>
        {
            Vector3 worldPos = info.to.transform.position + info.to.GetRandomOffset();
            GameObject floatingText = Instantiate(magicDamageFloatingText.gameObject, worldPos, Quaternion.identity);
            floatingText.SetActive(true);
            floatingText.GetComponent<FloatingText>().text = Mathf.Ceil(info.finalDamage).ToString();

        };
    }

    public void SpawnLocalPlayer(PlayerType type, Vector3 location)
    {
        LivingThing localPlayer;
        List<Activatable> startItems = new List<Activatable>();
        if(type == PlayerType.Elemental)
        {
            localPlayer = PhotonNetwork.Instantiate("player_Elemental", location, Quaternion.identity).GetComponent<LivingThing>();
            AvatarManager.instance.SetAvatar(PlayerType.Elemental);
        }
        else
        {
            localPlayer = PhotonNetwork.Instantiate("player_Reptile", location, Quaternion.identity).GetComponent<LivingThing>();
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
        magicDamageFloatingText.gameObject.SetActive(false);
        physicalDamageFloatingText.gameObject.SetActive(false);
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
