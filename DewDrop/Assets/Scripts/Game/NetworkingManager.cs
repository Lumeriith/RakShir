using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public interface IDelayedDestroy
{
    bool IsReadyForDestroy();
    GameObject GetGameObject();
    Coroutine StartCoroutine(IEnumerator routine);
}

public class DewFakePrefabPool : IPunPrefabPool
{
    public DewFakePrefabPool()
    {
        PhotonNetwork.PrefabPool = this;
    }

    public void Destroy(GameObject gameObject)
    {
        IDelayedDestroy target = gameObject.GetComponent<IDelayedDestroy>();
        if (target != null) target.StartCoroutine(CoroutineDespawnWhenReady(target));
        else Object.Destroy(gameObject);
    }

    private static IEnumerator CoroutineDespawnWhenReady(IDelayedDestroy target)
    {
        while (!target.IsReadyForDestroy()) yield return null;
        Object.Destroy(target.GetGameObject());
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        string shortName = prefabId.Contains("/") ? prefabId.Split('/')[1] : prefabId;
        GameObject original;
        if (prefabId.StartsWith("AbilityInstances/")) original = DewResources.GetAbilityInstance(shortName);
        else if (prefabId.StartsWith("Items/")) original = DewResources.GetItem(shortName);
        else if (prefabId.StartsWith("Entities/")) original = DewResources.GetEntity(shortName);
        else if (prefabId.StartsWith("Rooms/")) original = DewResources.GetRoom(shortName);
        else if (prefabId.StartsWith("SFXInstances/")) original = DewResources.GetSFXInstance(shortName);
        else original = DewResources.GetGameObject(shortName);
        GameObject newObject = Object.Instantiate(original, position, rotation);
        newObject.SetActive(false);
        return newObject;
    }
}

public class NetworkingManager : MonoBehaviourPunCallbacks
{
    public static NetworkingManager instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<NetworkingManager>();
            return _instance;
        }
    }
    private static NetworkingManager _instance;


    private float lastSyncAllStats = 0;
    public float syncAllStatsInterval = 5f;

    private RaiseEventOptions raiseToAll = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    private SendOptions sendReliably = new SendOptions { Reliability = true };

    private static bool registeredCustomTypes = false;

    private void Start()
    {
        if (!registeredCustomTypes)
        {
            //PhotonPeer.RegisterType(typeof(SourceInfo), 255, SourceInfo.SerializeSourceInfo, SourceInfo.DeserializeSourceInfo);
            registeredCustomTypes = true;
        }


        PhotonNetwork.PrefabPool = new DewFakePrefabPool();
    }



    

    void Update()
    {
        if(!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;

       
        if (Time.time - lastSyncAllStats > syncAllStatsInterval)
        {
            
        }
    }


    




}
