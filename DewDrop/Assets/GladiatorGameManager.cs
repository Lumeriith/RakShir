using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using NaughtyAttributes;
using System.Text.RegularExpressions;

public enum GladiatorGamePhase { Prepare, PvE, PvP, End };

public class GladiatorGameManager : MonoBehaviourPunCallbacks
{
    public bool debugSoloPlayMode = true;
    
    public GladiatorGamePhase phase = GladiatorGamePhase.Prepare;
    public List<GameObject> epicLootPool, rareLootPool, commonLootPool, consumableLootPool;

    public float goldModifier = 1.0f;
    public float goldRandomness = 0.1f;

    private List<KeyValuePair<Photon.Realtime.Player, LivingThing>> gamePlayers;

    private int[] redEpicLootDeck;
    private int[] redRareLootDeck;
    private int[] redCommonLootDeck;

    private int[] blueEpicLootDeck;
    private int[] blueRareLootDeck;
    private int[] blueCommonLootDeck;

    private int redNextEpicLoot = 0;
    private int redNextRareLoot = 0;
    private int redNextCommonLoot = 0;

    private int blueNextEpicLoot = 0;
    private int blueNextRareLoot = 0;
    private int blueNextCommonLoot = 0;
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





    private Room firstRoom;

    private List<E> ShuffleList<E>(List<E> inputList) // http://www.vcskicks.com/randomize_array.php
    {
        List<E> randomList = new List<E>();

        System.Random r = new System.Random();
        int randomIndex = 0;
        while (inputList.Count > 0)
        {
            randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
            randomList.Add(inputList[randomIndex]); //add it to the new, random list
            inputList.RemoveAt(randomIndex); //remove to avoid duplicates
        }
        return randomList; //return the new random list
    }

    private void SetupGame()
    {
        
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Only MasterClient can setup Gladiator game!");
            return;
        }
        CreateMap();
        BuildLootDecks();
        RegisterKillDropLootEvent();
        RegisterKillRewardGoldEvent();
        SpawnPlayers();
    }
    private void SpawnPlayers()
    {
        bool nextPlayerIsRedTeam = Random.value < .5f;
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        PlayerType type;

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            type = nextPlayerIsRedTeam ? PlayerType.Reptile : PlayerType.Elemental;
            photonView.RPC("RpcCreateLocalPlayer", players[i], (int)type, firstRoom.entryPoint.position);
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
            thing.OnDeath += (InfoDeath info) =>
            {
                HandlePlayerDeath(info.victim);
            };
        };
    }
    
    private void HandlePlayerDeath(LivingThing player)
    {
        if(phase == GladiatorGamePhase.PvE)
        {
            StartCoroutine(CoroutinePlayerRevival(player));
        }
        else if (phase == GladiatorGamePhase.PvP)
        {
            throw new System.NotImplementedException();
        }
    }

    private IEnumerator CoroutinePlayerRevival(LivingThing player)
    {

        yield return new WaitForSeconds()
    }

    private void RpcDropLoot(string name, Vector3 position)
    {
        GameManager.instance.DropLoot(name, position);
    }

    private void DropLootOnContext(LivingThing killer, LivingThing victim)
    {
        float randomValue = Random.value;
        float epic = 0f, rare = 0f, common = 0f, consumable = 0f;

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
        }

        victim.GiveGold(victim.droppedGold * goldModifier * (1 + goldRandomness * (Random.value * 2f - 1f)), rewardTarget);
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

        for(int i = 0; i < rareLootPool.Count; i++)
        {
            numberMap.Add(i);
        }
        redRareLootDeck = ShuffleList(numberMap).ToArray();
        blueRareLootDeck = ShuffleList(numberMap).ToArray();

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

    private void RpcSyncLootDecks(int[] a, int[] b, int[] c, int[] d, int[] e, int[] f)
    {
        redEpicLootDeck = a;
        redRareLootDeck = b;
        redCommonLootDeck = c;

        blueEpicLootDeck = d;
        blueRareLootDeck = e;
        blueCommonLootDeck = f;
    }

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("Gladiator Game cannot operate if not in room, returning to Main Menu...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
            return;
        }

        if (debugSoloPlayMode) SetupGame();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (phase == GladiatorGamePhase.Prepare && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            SetupGame();
        }
    }


    private void RpcCreateLocalPlayer(int type, Vector3 pos)
    {
        LivingThing thing = GameManager.instance.SpawnLocalPlayer((PlayerType)type, pos);
        photonView.RPC("RpcRegisterLivingThingOnGamePlayers", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, thing.photonView.ViewID);
    }

    private void RpcRegisterLivingThingOnGamePlayers(int actorNumber, int viewId)
    {
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if(players[i].ActorNumber == actorNumber)
            {
                gamePlayers.Add(new KeyValuePair<Photon.Realtime.Player, LivingThing>(players[i], PhotonNetwork.GetPhotonView(viewId).GetComponent<LivingThing>()));
                return;
            }
        }
    }
    private void CreateMap()
    {
        // firstRoom = ~~~~~

    }

}
