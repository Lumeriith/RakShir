using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using Doozy.Engine;
using Doozy.Engine.Nody;
using Doozy.Engine.Nody.Nodes;
public enum PlayerType { Elemental, Reptile }
public enum IngameNodeType { Unknown, Ingame, Menu, Inventory, Shop, Map, MapObelisk, Moving }
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
    public System.Action<LivingThing> OnLivingThingDestroy = (LivingThing _) => { };

    public System.Action<Activatable> OnActivatableInstantiate = (Activatable _) => { };
    public System.Action<LivingThing> OnLivingThingRoomEnter = (LivingThing _) => { };

    [HideInInspector]
    public List<LivingThing> everyLivingThings = new List<LivingThing>();

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

    public static GraphController nodyGraphController
    {
        get
        {
            if (_nodyGraphController == null) _nodyGraphController = FindObjectOfType<GraphController>();
            return _nodyGraphController;
        }
    }

    private static GraphController _nodyGraphController = null;

    public static IngameNodeType cachedCurrentNodeType { get; private set; }
    private void FixedUpdate()
    {
        cachedCurrentNodeType = GetCurrentNode();
    }

    public static IngameNodeType GetCurrentNode() // Probably would be a good idea to cache this? or not?
    {
        if (nodyGraphController.Graph.ActiveNode != null && nodyGraphController.Graph.ActiveNode.NodeType == Doozy.Engine.Nody.Models.NodeType.SubGraph)
        {
            string nodeName = ((SubGraphNode)nodyGraphController.Graph.ActiveNode).SubGraph.ActiveNode.Name;
            if (nodeName == "Ingame") return IngameNodeType.Ingame;
            else if (nodeName == "Menu") return IngameNodeType.Menu;
            else if (nodeName == "Inventory") return IngameNodeType.Inventory;
            else if (nodeName == "Shop") return IngameNodeType.Shop;
            else if (nodeName == "Map") return IngameNodeType.Map;
            else if (nodeName == "Map Obelisk") return IngameNodeType.MapObelisk;
            else if (nodeName == "Moving") return IngameNodeType.Moving;
        }
        return IngameNodeType.Unknown;
    }

    public static void SetVisionMultiplier(float multiplier)
    {
        PlayerViewCamera.instance.visionMultiplier = multiplier;
    }

    public static LivingThing SpawnLivingThing(string livingThingName, Vector3 location, Quaternion rotation = new Quaternion())
    {
        return PhotonNetwork.Instantiate("LivingThings/" + livingThingName, location, rotation).GetComponent<LivingThing>();
    }

    public static Item SpawnItem(string itemName, Vector3 location, Quaternion rotation = new Quaternion())
    {
        return PhotonNetwork.Instantiate("Items/" + itemName, location, rotation).GetComponent<Item>();
    }

    public static LivingThing SpawnLocalPlayer(PlayerType type, Vector3 location)
    {
        LivingThing localPlayer;
        List<Activatable> startItems = new List<Activatable>();
        if (type == PlayerType.Elemental)
        {
            localPlayer = SpawnLivingThing("player_Elemental", location);
            startItems.Add(SpawnItem("equip_Armor_ElementalIntegrity", location));
            startItems.Add(SpawnItem("equip_Boots_ElementalDetermination", location));
            startItems.Add(SpawnItem("equip_Weapon_ElementalJustice", location));

            startItems.Add(SpawnItem("cons_HealingPotionMedium", location));
            startItems.Add(SpawnItem("cons_HealingPotionMedium", location));
            startItems.Add(SpawnItem("cons_ManaPotionMedium", location));


            AvatarManager.instance.SetAvatar(PlayerType.Elemental);
        }
        else
        {
            localPlayer = SpawnLivingThing("player_Reptile", location);
            startItems.Add(SpawnItem("equip_Armor_ReptileSkin", location));
            startItems.Add(SpawnItem("equip_Boots_ReptileFeet", location));
            startItems.Add(SpawnItem("equip_Weapon_ReptileClaw", location));

            startItems.Add(SpawnItem("cons_HealingPotionMedium", location));
            startItems.Add(SpawnItem("cons_HealingPotionMedium", location));
            startItems.Add(SpawnItem("cons_ManaPotionMedium", location));

            AvatarManager.instance.SetAvatar(PlayerType.Reptile);
        }

        foreach (Activatable item in startItems)
        {
            localPlayer.ActivateImmediately(item);
        }

        instance.localPlayer = localPlayer;
        localPlayer.SetReadableName(PlayerPrefs.GetString("characterName", "이름없는 영웅"));
        return localPlayer;
    }
    private void Start()
    {
        OnLivingThingInstantiate += (LivingThing thing) =>
        {
            everyLivingThings.Add(thing);
        };

        OnLivingThingDestroy += (LivingThing thing) =>
        {
            everyLivingThings.Remove(thing);
        };
    }
    public static void DropLoot(string name, Vector3 position)
    {
        GameObject gobj = SpawnItem(name, position + Vector3.up * 2f + Random.insideUnitSphere * 0.8f, Random.rotation).gameObject;
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

    public static void DoObeliskTeleportation(Room room)
    {
        instance.StartCoroutine(CoroutineMove(room));
        GameEventMessage.SendEvent("Move Started");
    }


    private static IEnumerator CoroutineMove(Room room)
    {
        instance.localPlayer.ApplyStatusEffect(StatusEffect.Protected(instance.localPlayer, 4f));
        instance.localPlayer.ApplyStatusEffect(StatusEffect.HealOverTime(instance.localPlayer, 4f, (instance.localPlayer.maximumHealth - instance.localPlayer.currentHealth), true));
        instance.localPlayer.DoManaHeal(instance.localPlayer.stat.finalMaximumMana / 2f, instance.localPlayer, true);
        for (int i = 0; i < 20; i++)
        {
            instance.localPlayer.RpcFlashForDuration(1, 1, 1, 1, 0.2f, 0.8f - 0.03f * i);
            yield return new WaitForSeconds(0.03f);
        }
        instance.localPlayer.RpcScaleForDuration(0f, 0.5f);
        if(instance.localPlayer.team == Team.Red && room.redCustomEntryPoint != null)
        {
            instance.localPlayer.Teleport(room.redCustomEntryPoint.position);
        }
        else if (instance.localPlayer.team == Team.Blue && room.blueCustomEntryPoint != null)
        {
            instance.localPlayer.Teleport(room.blueCustomEntryPoint.position);
        }
        else
        {
            instance.localPlayer.Teleport(room.entryPoint.position);
        }
        
        Instantiate(GladiatorGameManager.instance.monsterSpawnEffect, instance.localPlayer.transform.position, Quaternion.identity);
        instance.localPlayer.SetCurrentRoom(room);
        GameEventMessage.SendEvent("Move Finished");
        OverlayCanvas.Blink();
        instance.localPlayer.ApplyStatusEffect(StatusEffect.Stun(instance.localPlayer, 0.5f));
        instance.localPlayer.ApplyStatusEffect(StatusEffect.Speed(instance.localPlayer, 3.5f, 30f));


        yield return new WaitForSeconds(.45f);
        for (float t = 0.5f; t < 1f; t += 0.05f)
        {
            instance.localPlayer.RpcFlashForDuration(1f, 1f, 1f, 1f, 0.225f, t);
        }
    }



}
