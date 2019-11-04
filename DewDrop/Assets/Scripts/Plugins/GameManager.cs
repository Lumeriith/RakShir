using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

public enum PlayerType { Elemental, Reptile }

public class GameManager : MonoBehaviour
{
    private static StatusEffectType[] statusEffectsToDisplay =
    {
        StatusEffectType.Stasis,
        StatusEffectType.Invulnerable,
        StatusEffectType.Protected,
        StatusEffectType.Untargetable,
        StatusEffectType.Unstoppable,
        StatusEffectType.MindControl,
        StatusEffectType.Polymorph,
        StatusEffectType.Stun,
        StatusEffectType.Sleep,
        StatusEffectType.Charm,
        StatusEffectType.Fear,
        StatusEffectType.Silence,
        StatusEffectType.Root,
        StatusEffectType.Blind,
        StatusEffectType.Custom
    };

    private static string[] statusEffectNamesToDisplay =
    {
        "정지", "무적", "보호", "지정불가", "저지불가", "정신조종", "변이", "기절", "수면", "매혹", "공포", "침묵", "이동불가", "실명", ""
    };


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

    public void SetVisionMultiplier(float multiplier)
    {
        PlayerViewCamera.instance.visionMultiplier = multiplier;
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
        localPlayer.SetReadableName(PlayerPrefs.GetString("characterName", "이름없는 영웅"));
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

    public static string GetImportantStatusEffectName(LivingThing thing)
    {
        List<StatusEffectType> existingTypes = new List<StatusEffectType>();
        for (int i = 0; i < thing.statusEffect.statusEffects.Count; i++)
        {
            if (!existingTypes.Contains(thing.statusEffect.statusEffects[i].type)) existingTypes.Add(thing.statusEffect.statusEffects[i].type);
        }
        if (existingTypes.Count == 0) return "";
        for (int i = 0; i < statusEffectsToDisplay.Length; i++)
        {
            if (existingTypes.Contains(statusEffectsToDisplay[i]))
            {
                if (statusEffectsToDisplay[i] == StatusEffectType.Custom) return (string)thing.statusEffect.statusEffects.Find(x => x.type == StatusEffectType.Custom).parameter;
                return statusEffectNamesToDisplay[i];
            }
        }
        return "";
    }

    public static string GetImportantStatusEffectNames(LivingThing thing)
    {
        string result = "";
        List<StatusEffectType> existingTypes = new List<StatusEffectType>();
        for (int i = 0; i < thing.statusEffect.statusEffects.Count; i++)
        {
            if (!existingTypes.Contains(thing.statusEffect.statusEffects[i].type)) existingTypes.Add(thing.statusEffect.statusEffects[i].type);
        }
        if (existingTypes.Count == 0) return "";
        for (int i = 0; i < statusEffectsToDisplay.Length; i++)
        {
            if (existingTypes.Contains(statusEffectsToDisplay[i]))
            {
                if (statusEffectsToDisplay[i] == StatusEffectType.Custom) result += (string)thing.statusEffect.statusEffects.Find(x => x.type == StatusEffectType.Custom).parameter + " ";
                else result += statusEffectNamesToDisplay[i] + " ";
            }
        }
        if (result.EndsWith(" ")) result = result.Substring(0, result.Length - 1);
        return result;
    }





}
