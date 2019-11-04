using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class TestGameLogic : MonoBehaviourPunCallbacks
{

    public Transform elementalSpawnPoint;
    public Transform reptileSpawnPoint;

    private void Start()
    {
        DecalSystem.DecalUtils.UpdateAffectedObjects();
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("Test Game cannot operate if not in room, returning to Main Menu...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
            LobbyManager.sceneName = "Test Game";
        }
    }
    public void SpawnAsReptile()
    {
        GameManager.instance.SpawnLocalPlayer(PlayerType.Reptile, reptileSpawnPoint.position);
    }
    
    public void SpawnAsElemental()
    { 
        GameManager.instance.SpawnLocalPlayer(PlayerType.Elemental, reptileSpawnPoint.position);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if(PhotonNetwork.IsMasterClient) NetworkingManager.instance.Sync();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from server. Returning to Main Menu...");
        StartCoroutine(CoroutineMainMenu());
        
    }

    IEnumerator CoroutineMainMenu()
    {
        yield return new WaitForSeconds(1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }
}
