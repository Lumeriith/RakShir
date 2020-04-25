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



    public static LivingThing SpawnLocalPlayer(PlayerType type, Vector3 location)
    {
        LivingThing localPlayer;
        List<Activatable> startItems = new List<Activatable>();
        if (type == PlayerType.Elemental)
        {
            localPlayer = Dew.SpawnEntity("player_Elemental", location);
            startItems.Add(Dew.SpawnItem("equip_Armor_ElementalIntegrity", location));
            startItems.Add(Dew.SpawnItem("equip_Boots_ElementalDetermination", location));
            startItems.Add(Dew.SpawnItem("equip_Weapon_ElementalJustice", location));

            startItems.Add(Dew.SpawnItem("cons_HealingPotionMedium", location));
            startItems.Add(Dew.SpawnItem("cons_HealingPotionMedium", location));
            startItems.Add(Dew.SpawnItem("cons_ManaPotionMedium", location));


            AvatarManager.instance.SetAvatar(PlayerType.Elemental);
        }
        else
        {
            localPlayer = Dew.SpawnEntity("player_Reptile", location);
            startItems.Add(Dew.SpawnItem("equip_Armor_ReptileSkin", location));
            startItems.Add(Dew.SpawnItem("equip_Boots_ReptileFeet", location));
            startItems.Add(Dew.SpawnItem("equip_Weapon_ReptileClaw", location));

            startItems.Add(Dew.SpawnItem("cons_HealingPotionMedium", location));
            startItems.Add(Dew.SpawnItem("cons_HealingPotionMedium", location));
            startItems.Add(Dew.SpawnItem("cons_ManaPotionMedium", location));

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

    private void Awake()
    {
        DewResources.Initialize();
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
        GameObject gobj = Dew.SpawnItem(name, position + Vector3.up * 2f + Random.insideUnitSphere * 0.8f, Random.rotation).gameObject;
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
