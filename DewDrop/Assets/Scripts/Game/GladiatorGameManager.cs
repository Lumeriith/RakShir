using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Text.RegularExpressions;
using DecalSystem;
using Doozy.Engine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
public enum GladiatorGamePhase { Waiting, PvE, PvP, End };
[System.Serializable]
public class RoomListWrapper
{
    public List<Room> rooms;
}


public class GladiatorGameManager : MonoBehaviourPunCallbacks
{
    public static GladiatorGameManager instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<GladiatorGameManager>();
            return _instance;
        }
    }
    private static GladiatorGameManager _instance;

    public Toggle readyCheckbox;
    public bool debugSoloPlayMode = true;
    [Header("General Settings")]
    public GladiatorGamePhase phase = GladiatorGamePhase.Waiting;
    public List<GameObject> epicLootPool, rareLootPool, commonLootPool, consumableLootPool;

    public float goldModifier = 1.0f;
    public float goldRandomness = 0.1f;
    public float lostGoldMultiplierOnDeath = 0.3f;
    public GameObject monsterSpawnEffect;

    public float defeatGold = 1500f;
    public float victoryGold = 1500f;

    public int roundTime = 60 * 5;

    [Header("Map Generation Settings")]
    public List<MapPlaceholderNode> nodes;
    public List<RoomListWrapper> nodeRoomsMap;
    public int redStartRoomIndex;
    public int blueStartRoomIndex;


    [Header("Loot Drop Chance Settings")]
    public float lesserEpicDropChance = 0.00f;
    public float lesserRareDropChance = 0.00f;
    public float lesserCommonDropChance = 0.00f;
    public float lesserConsumableDropChance = 0.15f;

    public float normalEpicDropChance = 0.02f;
    public float normalRareDropChance = 0.07f;
    public float normalCommonDropChance = 0.15f;
    public float normalConsumableDropChance = 0.05f;

    public float eliteEpicDropChance = 0.35f;
    public float eliteRareDropChance = 0.15f;
    public float eliteCommonDropChance = 0.05f;
    public float eliteConsumableDropChance = 0.05f;

    public float bossEpicDropChance = 0.60f;
    public float bossRareDropChance = 0.30f;
    public float bossCommonDropChance = 0.05f;
    public float bossConsumableDropChance = 0.05f;

    [Header("Current Loot Decks")]

    [SerializeField]
    [ReadOnly]
    private int[] redEpicLootDeck;
    [SerializeField]
    [ReadOnly]
    private int[] redRareLootDeck;
    [SerializeField]
    [ReadOnly]
    private int[] redCommonLootDeck;

    [SerializeField]
    [ReadOnly]
    private int[] blueEpicLootDeck;
    [SerializeField]
    [ReadOnly]
    private int[] blueRareLootDeck;
    [SerializeField]
    [ReadOnly]
    private int[] blueCommonLootDeck;

    public Dictionary<Photon.Realtime.Player, LivingThing> gamePlayers = new Dictionary<Photon.Realtime.Player, LivingThing>();
    private Dictionary<Photon.Realtime.Player, bool> readyStatuses = new Dictionary<Photon.Realtime.Player, bool>();


    private int redNextEpicLoot = 0;
    private int redNextRareLoot = 0;
    private int redNextCommonLoot = 0;

    private int blueNextEpicLoot = 0;
    private int blueNextRareLoot = 0;
    private int blueNextCommonLoot = 0;
    private List<Room> createdRooms = new List<Room>();

    private int redScore = 0;
    private int blueScore = 0;

    public int roundIndex = 0;
    public bool[] didRedTeamWin = new bool[6];

    private CanvasGroup canvasGroup_Victory;
    private CanvasGroup canvasGroup_Defeat;

    private Canvas canvas_PvP;

    private Text text_Time;

    private Coroutine roundTimeTicker;
    [SerializeField]
    private int remainingRoundTime = 0;

    public static void DoObeliskTeleportation(Room room)
    {
        instance.StartCoroutine(CoroutineMove(room));
        GameEventMessage.SendEvent("Move Started");
    }


    private static IEnumerator CoroutineMove(Room room)
    {
        GameManager.instance.localPlayer.ApplyStatusEffect(StatusEffect.Protected(SourceInfo.Empty(), 4f));
        GameManager.instance.localPlayer.ApplyStatusEffect(StatusEffect.HealOverTime(SourceInfo.Empty(), 4f, (GameManager.instance.localPlayer.maximumHealth - GameManager.instance.localPlayer.currentHealth), true));
        GameManager.instance.localPlayer.DoManaHeal(GameManager.instance.localPlayer.stat.finalMaximumMana / 2f, GameManager.instance.localPlayer, true, new SourceInfo());
        for (int i = 0; i < 20; i++)
        {
            GameManager.instance.localPlayer.RpcFlashForDuration(1, 1, 1, 1, 0.2f, 0.8f - 0.03f * i);
            yield return new WaitForSeconds(0.03f);
        }
        GameManager.instance.localPlayer.RpcScaleForDuration(0f, 0.5f);
        if (GameManager.instance.localPlayer.team == Team.Red && room.redCustomEntryPoint != null)
        {
            GameManager.instance.localPlayer.Teleport(room.redCustomEntryPoint.position);
        }
        else if (GameManager.instance.localPlayer.team == Team.Blue && room.blueCustomEntryPoint != null)
        {
            GameManager.instance.localPlayer.Teleport(room.blueCustomEntryPoint.position);
        }
        else
        {
            GameManager.instance.localPlayer.Teleport(room.entryPoint.position);
        }

        Instantiate(GladiatorGameManager.instance.monsterSpawnEffect, GameManager.instance.localPlayer.transform.position, Quaternion.identity);
        GameManager.instance.localPlayer.SetCurrentRoom(room);
        GameEventMessage.SendEvent("Move Finished");
        OverlayCanvas.Blink();
        GameManager.instance.localPlayer.ApplyStatusEffect(StatusEffect.Stun(SourceInfo.Empty(), 0.5f));
        GameManager.instance.localPlayer.ApplyStatusEffect(StatusEffect.Speed(SourceInfo.Empty(), 3.5f, 30f));


        yield return new WaitForSeconds(.45f);
        for (float t = 0.5f; t < 1f; t += 0.05f)
        {
            GameManager.instance.localPlayer.RpcFlashForDuration(1f, 1f, 1f, 1f, 0.225f, t);
        }
    }



    private IEnumerator CoroutineTickTime()
    {
        remainingRoundTime = roundTime;
        while (remainingRoundTime-- > 0)
        {
            text_Time.text = (remainingRoundTime / 60) + ":" + (remainingRoundTime % 60).ToString("00");
            yield return new WaitForSeconds(1f);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            float damage = 1f;
            while (true)
            {
                foreach(LivingThing thing in gamePlayers.Values)
                {
                    if(thing.currentHealth > damage)
                    {
                        thing.DoPureDamage(damage, thing, new SourceInfo());
                    }
                    else if (thing.currentHealth > 1f)
                    {
                        thing.DoPureDamage(thing.currentHealth - 1f, thing, new SourceInfo());
                    }
                }
                damage += 1f;
                yield return new WaitForSeconds(1f);
            }
        }
    }

    [PunRPC]
    private void RpcStartRoundTimer()
    {
        text_Time.enabled = true;
        roundTimeTicker = StartCoroutine(CoroutineTickTime());
    }

    [PunRPC]
    private void RpcStopRoundTimer()
    {
        text_Time.enabled = false;
        if (roundTimeTicker != null) StopCoroutine(roundTimeTicker);
        roundTimeTicker = null;

    }

    private void Awake()
    {
        canvasGroup_Victory = transform.Find("PvP Canvas/Victory").GetComponent<CanvasGroup>();
        canvasGroup_Defeat = transform.Find("PvP Canvas/Defeat").GetComponent<CanvasGroup>();
        text_Time = transform.Find("PvP Canvas/Time Text").GetComponent<Text>();
        canvas_PvP = transform.Find("PvP Canvas").GetComponent<Canvas>();
        text_Time = transform.Find("PvP Canvas/Time Text").GetComponent<Text>();
        canvas_PvP.gameObject.SetActive(false);
        text_Time.enabled = false;
    }

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("Gladiator Game cannot operate if not in room, returning to Main Menu...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
            return;
        }

        if (debugSoloPlayMode) StartCoroutine(CoroutineDelayedSetupGame(1f));
        readyStatuses.Add(PhotonNetwork.LocalPlayer, false);
    }



    [Button("Force Setup Game")]
    public void SetupGame()
    {
        if (phase != GladiatorGamePhase.Waiting)
        {
            Debug.LogWarning("Game has already started!");
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Only MasterClient can setup Gladiator game!");
            return;
        }
        PhotonNetwork.CurrentRoom.IsOpen = false;
        CreateMap();
        photonView.RPC("RpcUpdateDecalUtils", RpcTarget.All);
        BuildLootDecks();
        RegisterKillDropLootEvent();
        RegisterKillRewardGoldEvent();
        RegisterPlayerDeathEvent();
        
        photonView.RPC("RpcRegisterSpawnEffectEvent", RpcTarget.All);
        SpawnPlayers();
        SetPhase(GladiatorGamePhase.PvE);
        photonView.RPC("RpcSendGameStartedEvent", RpcTarget.All);
        RegisterPlayerRoomEnterEvent();
    }

    [PunRPC]
    private void RpcUpdateDecalUtils()
    {
        DecalUtils.UpdateAffectedObjects();
    }

    [PunRPC]
    private void RpcSendGameStartedEvent()
    {
        GameEventMessage.SendEvent("Game Started");
    }

    [PunRPC]
    private void RpcRegisterSpawnEffectEvent()
    {
        GameManager.instance.OnLivingThingInstantiate += (LivingThing thing) =>
        {
            if (thing.photonView.IsMine) thing.statusEffect.ApplyStatusEffect(StatusEffect.Stasis(SourceInfo.Empty(), .5f));
            Instantiate(monsterSpawnEffect, thing.transform.position, Quaternion.identity);
            thing.RpcScaleForDuration(0f, 0.5f);
            for (float t = 0.5f; t < 1f; t += 0.05f)
            {
                thing.RpcFlashForDuration(1f, 1f, 1f, 1f, 0.2f, t);
            }
        };
    }


    private void SpawnPlayers()
    {
        bool nextPlayerIsRedTeam = Random.value < .5f;
        Vector3 spawnPoint;
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        PlayerType type;

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            type = nextPlayerIsRedTeam ? PlayerType.Reptile : PlayerType.Elemental;
            if (nextPlayerIsRedTeam && createdRooms[nextPlayerIsRedTeam ? redStartRoomIndex : blueStartRoomIndex].redCustomEntryPoint != null)
            {
                spawnPoint = createdRooms[nextPlayerIsRedTeam ? redStartRoomIndex : blueStartRoomIndex].redCustomEntryPoint.position;
            }
            else if (!nextPlayerIsRedTeam && createdRooms[nextPlayerIsRedTeam ? redStartRoomIndex : blueStartRoomIndex].blueCustomEntryPoint != null)
            {
                spawnPoint = createdRooms[nextPlayerIsRedTeam ? redStartRoomIndex : blueStartRoomIndex].blueCustomEntryPoint.position;
            }
            else
            {
                spawnPoint = createdRooms[nextPlayerIsRedTeam ? redStartRoomIndex : blueStartRoomIndex].entryPoint.position;
            }
            photonView.RPC("RpcCreateLocalPlayer", players[i], (int)type, spawnPoint, createdRooms[nextPlayerIsRedTeam ? redStartRoomIndex : blueStartRoomIndex].photonView.ViewID);

            nextPlayerIsRedTeam = !nextPlayerIsRedTeam;
        }
    }

    private void RegisterKillDropLootEvent()
    {
        GameManager.instance.OnLivingThingInstantiate += (LivingThing thing) =>
        {
            thing.OnDeath += (InfoDeath info) =>
            {
                DropLootOnContext(info.killer, info.victim);
            };
        };
    }

    private void RegisterKillRewardGoldEvent()
    {
        GameManager.instance.OnLivingThingInstantiate += (LivingThing thing) =>
        {
            thing.OnDeath += (InfoDeath info) =>
            {
                RewardGoldOnContext(info.killer, info.victim);
            };
        };
    }

    private void RegisterPlayerDeathEvent()
    {
        GameManager.instance.OnLivingThingInstantiate += (LivingThing thing) =>
        {
            if (thing.type == LivingThingType.Player)
            {
                thing.OnDeath += (InfoDeath info) =>
                {
                    HandlePlayerDeath(info);
                };
            }

        };
    }

    private void RegisterPlayerRoomEnterEvent()
    {
        GameManager.instance.OnLivingThingRoomEnter += (LivingThing thing) =>
        {
            if (thing.type == LivingThingType.Player)
            {
                HandlePlayerRoomEnter(thing);
            }
        };
    }

    private void HandlePlayerDeath(InfoDeath info)
    {
        if (phase == GladiatorGamePhase.PvE)
        {
            StartCoroutine(CoroutinePlayerRevival(info.victim));
        }
        else if (phase == GladiatorGamePhase.PvP)
        {

            didRedTeamWin[roundIndex] = info.victim.team == Team.Blue;
            roundIndex++;
            photonView.RPC("RpcSyncRoundResult", RpcTarget.Others, didRedTeamWin, roundIndex);
            info.killer.ApplyStatusEffect(StatusEffect.Invulnerable(SourceInfo.Empty(), 5f));
            info.killer.ApplyStatusEffect(StatusEffect.HealOverTime(SourceInfo.Empty(), 5f, info.killer.maximumHealth - info.killer.currentHealth, true));
            photonView.RPC("RpcShowDefeatEffect", info.victim.photonView.Owner);
            photonView.RPC("RpcShowVictoryEffect", info.killer.photonView.Owner);
            photonView.RPC("RpcStopRoundTimer", RpcTarget.All);
            
            int redWins = 0, blueWins = 0;

            for (int i = 0; i < roundIndex; i++)
            {
                if (didRedTeamWin[i]) redWins++;
                else blueWins++;
            }

            if (redWins >= 3 || blueWins >= 3)
            {
                photonView.RPC("RpcGameEnd", RpcTarget.All);
            }
            else
            {
                info.victim.EarnGold(defeatGold);
                info.killer.EarnGold(victoryGold);
                StartCoroutine(CoroutinePlayerRevival(info.victim));
                photonView.RPC("RpcStartRoundTimer", RpcTarget.All);
            }



        }
    }

    [PunRPC]
    private void RpcShowDefeatEffect()
    {
        StartCoroutine(CoroutineShowDefeatEffect());

    }

    [PunRPC]
    private void RpcShowVictoryEffect()
    {
        StartCoroutine(CoroutineShowVictoryEffect());

    }



    private IEnumerator CoroutineShowDefeatEffect()
    {
        canvasGroup_Defeat.DOFade(1f, 0.5f);
        yield return new WaitForSeconds(3f);
        canvasGroup_Defeat.DOFade(0f, 1f);
    }

    private IEnumerator CoroutineShowVictoryEffect()
    {
        canvasGroup_Victory.DOFade(1f, 0.5f);
        yield return new WaitForSeconds(3f);
        canvasGroup_Victory.DOFade(0f, 1f);
    }




    [PunRPC]
    private void RpcGameEnd()
    {
        phase = GladiatorGamePhase.End;
    }


    [PunRPC]
    private void RpcSyncRoundResult(bool[] didRedTeamWin, int roundIndex)
    {
        this.roundIndex = roundIndex;
        this.didRedTeamWin = didRedTeamWin;
    }

    private void HandlePlayerRoomEnter(LivingThing player)
    {
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        for(int i = 0; i < players.Length; i++)
        {
            if (!gamePlayers[players[i]].currentRoom.name.Contains("Nether")) return;
        }
        Transform nethergos = transform.Find("/monster_BossNethergos(Clone)");
        if (nethergos != null) nethergos.GetComponent<BossNethergosBehaviour>().Disappear();
        SetPhase(GladiatorGamePhase.PvP);
        
        for (int i = 0; i < players.Length; i++)
        {
            gamePlayers[players[i]].ApplyStatusEffect(StatusEffect.Invulnerable(SourceInfo.Empty(), 5f));
            gamePlayers[players[i]].ApplyStatusEffect(StatusEffect.HealOverTime(SourceInfo.Empty(), 5f, gamePlayers[players[i]].maximumHealth - gamePlayers[players[i]].currentHealth, true));
        }

        photonView.RPC("RpcStartRoundTimer", RpcTarget.All);
    }



    private IEnumerator CoroutinePlayerRevival(LivingThing player)
    {
        yield return new WaitForSeconds(4f);
        player.SpendGold(player.stat.currentGold * lostGoldMultiplierOnDeath);
        if(player.team == Team.Blue && player.currentRoom.blueCustomEntryPoint != null)
        {
            player.Teleport(player.currentRoom.blueCustomEntryPoint.position);
        }
        else if (player.team == Team.Red && player.currentRoom.redCustomEntryPoint != null)
        {
            player.Teleport(player.currentRoom.redCustomEntryPoint.position);
        }
        else
        {
            player.Teleport(player.currentRoom.entryPoint.position);
        }
        
        player.Revive();
        player.DoHeal(player.maximumHealth, player, true, new SourceInfo());
        player.DoManaHeal(player.stat.finalMaximumMana, player, true, new SourceInfo());
        player.statusEffect.ApplyStatusEffect(StatusEffect.Protected(SourceInfo.Empty(), 5f));
    }

    private void SetPhase(GladiatorGamePhase phase)
    {
        photonView.RPC("RpcSetPhase", RpcTarget.All, (int)phase);
    }

    [PunRPC]
    private void RpcSetPhase(int phase)
    {
        this.phase = (GladiatorGamePhase)phase;
        if((GladiatorGamePhase)phase == GladiatorGamePhase.PvP) canvas_PvP.gameObject.SetActive(true);
    }

    [PunRPC]
    private void RpcDropLoot(string name, Vector3 position)
    {
        GameManager.DropLoot(name, position);
    }

    private void DropLootOnContext(LivingThing killer, LivingThing victim)
    {
        float randomValue = Random.value;
        float epic = 0f, rare = 0f, common = 0f, consumable = 0f;

        if (victim.type != LivingThingType.Monster) return;

        if(victim.tier == LivingThingTier.Lesser)
        {
            epic = lesserEpicDropChance;
            rare = lesserRareDropChance;
            common = lesserCommonDropChance;
            consumable = lesserConsumableDropChance;
        }
        else if (victim.tier == LivingThingTier.Normal)
        {
            epic = normalEpicDropChance;
            rare = normalRareDropChance;
            common = normalCommonDropChance;
            consumable = normalConsumableDropChance;
        }
        else if (victim.tier == LivingThingTier.Elite)
        {
            epic = eliteEpicDropChance;
            rare = eliteRareDropChance;
            common = eliteCommonDropChance;
            consumable = eliteConsumableDropChance;
        }
        else if (victim.tier == LivingThingTier.Boss)
        {
            epic = bossEpicDropChance;
            rare = bossRareDropChance;
            common = bossCommonDropChance;
            consumable = bossConsumableDropChance;
        }

        if (randomValue < epic)
        {
            if (killer.team == Team.Red)
            {
                photonView.RPC("RpcDropLoot", victim.photonView.Owner, epicLootPool[redEpicLootDeck[redNextEpicLoot]].name, victim.transform.position + victim.GetCenterOffset());
                redNextEpicLoot = (redNextEpicLoot + 1) % redEpicLootDeck.Length;
            }
            else if (killer.team == Team.Blue)
            {
                photonView.RPC("RpcDropLoot", victim.photonView.Owner, epicLootPool[blueEpicLootDeck[blueNextEpicLoot]].name, victim.transform.position + victim.GetCenterOffset());
                blueNextEpicLoot = (blueNextEpicLoot + 1) % blueEpicLootDeck.Length;
            }
        }
        else if (randomValue < epic + rare)
        {
            if (killer.team == Team.Red)
            {
                photonView.RPC("RpcDropLoot", victim.photonView.Owner, rareLootPool[redRareLootDeck[redNextRareLoot]].name, victim.transform.position + victim.GetCenterOffset());
                redNextRareLoot = (redNextRareLoot + 1) % redRareLootDeck.Length;
            }
            else if (killer.team == Team.Blue)
            {
                photonView.RPC("RpcDropLoot", victim.photonView.Owner, rareLootPool[blueRareLootDeck[blueNextRareLoot]].name, victim.transform.position + victim.GetCenterOffset());
                blueNextRareLoot = (blueNextRareLoot + 1) % blueRareLootDeck.Length;
            }
        }
        else if (randomValue < epic + rare + common)
        {
            if (killer.team == Team.Red)
            {
                photonView.RPC("RpcDropLoot", victim.photonView.Owner, commonLootPool[redCommonLootDeck[redNextCommonLoot]].name, victim.transform.position + victim.GetCenterOffset());
                redNextCommonLoot = (redNextCommonLoot + 1) % redCommonLootDeck.Length;
            }
            else if (killer.team == Team.Blue)
            {
                photonView.RPC("RpcDropLoot", victim.photonView.Owner, commonLootPool[blueCommonLootDeck[blueNextCommonLoot]].name, victim.transform.position + victim.GetCenterOffset());
                blueNextCommonLoot = (blueNextCommonLoot + 1) % blueCommonLootDeck.Length;
            }
        }
        else if (randomValue < epic + rare + common + consumable)
        {
            photonView.RPC("RpcDropLoot", victim.photonView.Owner, consumableLootPool[Random.Range(0, consumableLootPool.Count)].name, victim.transform.position + victim.GetCenterOffset());
        }
    }

    private void RewardGoldOnContext(LivingThing killer, LivingThing victim)
    {
        LivingThing rewardTarget = null;
        if(victim.type == LivingThingType.Monster)
        {
            if(killer.type == LivingThingType.Player)
            {
                rewardTarget = killer;
            }
            else if (killer.type == LivingThingType.Summon && killer.summoner.type == LivingThingType.Player)
            {
                rewardTarget = killer.summoner;
            }
            if (rewardTarget == null) return;
            victim.GiveGold(victim.droppedGold * goldModifier * (1 + goldRandomness * (Random.value * 2f - 1f)), rewardTarget);
            SFXManager.CreateSFXInstance("si_local_EarnMoney", victim.transform.position, rewardTarget.photonView.Owner);
        }

        
    }



    private void BuildLootDecks()
    {
        
        #region Build Epic Loot Deck
        List<List<int>> epicClumps = new List<List<int>>();
        List<string> epicClumpSetNames = new List<string>();
        for (int i = 0; i < epicLootPool.Count; i++)
        {
            string setName = epicLootPool[i].name.Split('_')[2];
            int found = -1;
            // Find exact clump for the specific set.
            for (int j = 0; j < epicClumpSetNames.Count; j++)
            {
                if (epicClumpSetNames[j].Contains("|" + setName + "|"))
                {
                    found = j;
                    break;
                }
            }

            if (found != -1)
            {
                epicClumps[found].Add(i);
            }
            else
            {
                found = -1;
                // Find vacant clump.
                for (int j = 0; j < epicClumpSetNames.Count; j++)
                {
                    if (Regex.Matches(epicClumpSetNames[j], Regex.Escape("|")).Count == 2)
                    {
                        found = j;
                        break;
                    }
                }

                if (found != -1)
                {
                    epicClumpSetNames[found] += "|" + setName + "|";
                    epicClumps[found].Add(i);
                }
                else
                {
                    epicClumpSetNames.Add("|" + setName + "|");
                    epicClumps.Add(new List<int> { i });
                }
            }
        }

        int index = 0;

        redEpicLootDeck = new int[epicLootPool.Count];
        blueEpicLootDeck = new int[epicLootPool.Count];

        for (int i = 0; i < epicClumps.Count; i++)
        {
            epicClumps[i] = ShuffleList(epicClumps[i]);
        }
        epicClumps = ShuffleList(epicClumps);

        for (int i = 0; i < epicClumps.Count; i++)
        {
            for (int j = 0; j < epicClumps[i].Count; j++)
            {
                redEpicLootDeck[index++] = epicClumps[i][j];
            }
        }

        index = 0;
        for (int i = 0; i < epicClumps.Count; i++)
        {
            epicClumps[i] = ShuffleList(epicClumps[i]);
        }
        epicClumps = ShuffleList(epicClumps);

        for (int i = 0; i < epicClumps.Count; i++)
        {
            for (int j = 0; j < epicClumps[i].Count; j++)
            {
                blueEpicLootDeck[index++] = epicClumps[i][j];
            }
        }
        #endregion Build Epic Loot Deck

        List<int> numberMap = new List<int>();
        List<int> shuffledMap;
        List<int> lootDeck;

        for(int i = 0; i < rareLootPool.Count; i++)
        {
            numberMap.Add(i);
        }

        // Make Red Rare Loot Deck
        shuffledMap = ShuffleList(numberMap);
        lootDeck = new List<int>();
        List<EquipmentType> types = new List<EquipmentType> { EquipmentType.Armor, EquipmentType.Boots, EquipmentType.Helmet, EquipmentType.Ring };
        types = ShuffleList(types);
        types.Insert(0, EquipmentType.Weapon);

        for (int i = 0; i < types.Count; i++)
        {
            for (int j = 0; j < shuffledMap.Count; j++)
            {
                if (rareLootPool[shuffledMap[j]].GetComponent<Equipment>().type == types[i])
                {
                    lootDeck.Add(shuffledMap[j]);
                    shuffledMap.RemoveAt(j);
                    break;
                }
            }
        }

        lootDeck.AddRange(shuffledMap);
        redRareLootDeck = lootDeck.ToArray();

        // Make Blue Rare Loot Deck
        for (int i = 0; i < rareLootPool.Count; i++)
        {
            numberMap.Add(i);
        }
        shuffledMap = ShuffleList(numberMap);
        lootDeck = new List<int>();
        types = new List<EquipmentType> { EquipmentType.Armor, EquipmentType.Boots, EquipmentType.Helmet, EquipmentType.Ring };
        types = ShuffleList(types);
        types.Insert(0, EquipmentType.Weapon);
        
        for (int i = 0; i < types.Count; i++)
        {
            for (int j = 0; j < shuffledMap.Count; j++)
            {
                if (rareLootPool[shuffledMap[j]].GetComponent<Equipment>().type == types[i])
                {
                    lootDeck.Add(shuffledMap[j]);
                    shuffledMap.RemoveAt(j);
                    break;
                }
            }
        }
        lootDeck.AddRange(shuffledMap);
        blueRareLootDeck = lootDeck.ToArray();



        numberMap.Clear();
        for (int i = 0; i < commonLootPool.Count; i++)
        {
            numberMap.Add(i);
        }
        redCommonLootDeck = ShuffleList(numberMap).ToArray();
        blueCommonLootDeck = ShuffleList(numberMap).ToArray();

        numberMap.Clear();
        for (int i = 0; i < consumableLootPool.Count; i++)
        {
            numberMap.Add(i);
        }

        photonView.RPC("RpcSyncLootDecks", RpcTarget.Others,
            redEpicLootDeck, redRareLootDeck, redCommonLootDeck,
            blueEpicLootDeck, blueRareLootDeck, blueCommonLootDeck);
    }

    [PunRPC]
    private void RpcSyncLootDecks(int[] a, int[] b, int[] c, int[] d, int[] e, int[] f)
    {
        redEpicLootDeck = a;
        redRareLootDeck = b;
        redCommonLootDeck = c;

        blueEpicLootDeck = d;
        blueRareLootDeck = e;
        blueCommonLootDeck = f;
    }



    private void Update()
    {
        //readyCheckbox.gameObject.SetActive(readyStatuses.Count == 2);
        if (phase == GladiatorGamePhase.Waiting && PhotonNetwork.IsMasterClient && readyStatuses.Count == 2)
        {
            foreach(bool value in readyStatuses.Values)
            {
                if (!value) return;
            }
            SetupGame();
        }
    }

    public void UpdateReady()
    {
        photonView.RPC("RpcUpdateReady", RpcTarget.All, readyCheckbox.isOn);
    }

    [PunRPC]
    private void RpcUpdateReady(bool isReady, PhotonMessageInfo info)
    {
        readyStatuses[info.Sender] = isReady;
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if(phase == GladiatorGamePhase.Waiting) readyStatuses.Add(newPlayer, false);
    }

    IEnumerator CoroutineDelayedSetupGame(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        SetupGame();
    }

    [PunRPC]
    private void RpcCreateLocalPlayer(int type, Vector3 pos, int roomViewId)
    {
        LivingThing thing = GameManager.SpawnLocalPlayer((PlayerType)type, pos);
        Room room = PhotonNetwork.GetPhotonView(roomViewId).GetComponent<Room>();
        thing.SetCurrentRoom(room);
        photonView.RPC("RpcRegisterLivingThingOnGamePlayers", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, thing.photonView.ViewID);
    }

    [PunRPC]
    private void RpcRegisterLivingThingOnGamePlayers(int actorNumber, int viewId)
    {
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if(players[i].ActorNumber == actorNumber)
            {
                gamePlayers.Add(players[i], PhotonNetwork.GetPhotonView(viewId).GetComponent<LivingThing>());
                return;
            }
        }
    }
    private void CreateMap()
    {
        string roomName;

        for(int i = 0; i < nodes.Count; i++)
        {
            roomName = nodeRoomsMap[i].rooms[Random.Range(0, nodeRoomsMap[i].rooms.Count)].gameObject.name;
            createdRooms.Add(PhotonNetwork.InstantiateSceneObject("Rooms/" + roomName, nodes[i].transform.position, nodes[i].transform.rotation).GetComponent<Room>());
        }

        for(int i = 0; i < createdRooms.Count; i++)
        {
            int[] nextRoomViewIDs = new int[nodes[i].nextNodes.Count];
            for (int j = 0; j < nodes[i].nextNodes.Count; j++)
            {
                nextRoomViewIDs[j] = createdRooms[nodes.IndexOf(nodes[i].nextNodes[j])].photonView.ViewID;
            }
            photonView.RPC("RpcSetNextRooms", RpcTarget.All, createdRooms[i].photonView.ViewID, nextRoomViewIDs);
        }

        photonView.RPC("RpcDisablePlaceholderNodes", RpcTarget.All);
    }

    [PunRPC]
    private void RpcSetNextRooms(int roomViewID, int[] nextRoomViewIDs)
    {
        Room room = PhotonNetwork.GetPhotonView(roomViewID).GetComponent<Room>();
        for(int i = 0; i<nextRoomViewIDs.Length; i++)
        {
            room.nextRooms.Add(PhotonNetwork.GetPhotonView(nextRoomViewIDs[i]).GetComponent<Room>());
        }
    }

    [PunRPC]
    private void RpcDisablePlaceholderNodes()
    {
        for(int i = 0; i < nodes.Count; i++)
        {
            nodes[i].gameObject.SetActive(false);
        }
    }

    private List<E> ShuffleList<E>(List<E> inputList)
    {
        List<E> randomList = new List<E>();
        System.DateTime 내생일ㅎㅎ = new System.DateTime(1999, 9, 17, 0, 0, 0, System.DateTimeKind.Utc);
        System.Random r = new System.Random((int)(System.DateTime.UtcNow - 내생일ㅎㅎ).TotalSeconds);
        int randomIndex = 0;
        while (inputList.Count > 0)
        {
            randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
            randomList.Add(inputList[randomIndex]); //add it to the new, random list
            inputList.RemoveAt(randomIndex); //remove to avoid duplicates
        }
        return randomList; //return the new random list
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Disconnected! " + cause.ToString());
    }


    [Button("Autocomplete Pools")]
    private void AutocompletePools()
    {
        epicLootPool = new List<GameObject>();
        rareLootPool = new List<GameObject>();
        commonLootPool = new List<GameObject>();
        consumableLootPool = new List<GameObject>();

        object[] resources = Resources.LoadAll("");
        foreach (object obj in resources)
        {
            GameObject gobj = obj as GameObject;
            if (gobj == null) continue;
            if (gobj.name.StartsWith("cons_"))
            {
                consumableLootPool.Add(gobj);
            }
            else if (gobj.name.StartsWith("equip_"))
            {
                Equipment equip = gobj.GetComponent<Equipment>();
                if (equip == null) continue;
                if (equip.itemTier == ItemTier.Common) commonLootPool.Add(gobj);
                if (equip.itemTier == ItemTier.Rare) rareLootPool.Add(gobj);
                if (equip.itemTier == ItemTier.Epic) epicLootPool.Add(gobj);

            }
        }
    }

}
