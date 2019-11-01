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

    public System.Action<LivingThing> OnLivingThingInstantiate = (LivingThing _) => { };
    public System.Action<Activatable> OnActivatableInstantiate = (Activatable _) => { };
    public System.Action<LivingThing> OnLivingThingRoomEnter = (LivingThing _) => { }; 

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

    public LivingThing SpawnLocalPlayer(PlayerType type, Vector3 location)
    {
        LivingThing localPlayer;
        List<Activatable> startItems = new List<Activatable>();
        if (type == PlayerType.Elemental)
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

        foreach (Activatable item in startItems)
        {
            localPlayer.ActivateImmediately(item);
        }

        this.localPlayer = localPlayer;

        return localPlayer;
    }

    public void DropLoot(string name, Vector3 position)
    {
        GameObject gobj = PhotonNetwork.Instantiate(name, position + Vector3.up + Random.insideUnitSphere, Random.rotation);
        Rigidbody rb = gobj.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.velocity = (Vector3.up * 2 + Random.insideUnitSphere) * 3f;
            rb.angularVelocity = Random.onUnitSphere * 360f;
        }
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
