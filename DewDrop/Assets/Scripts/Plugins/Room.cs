using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;
using Aura2API;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(NavMeshSurface))]
public class Room : MonoBehaviourPun
{

    public List<Room> nextRooms;
    public Transform entryPoint;
    public Transform redCustomEntryPoint;
    public Transform blueCustomEntryPoint;

    public bool enableAuraCamera = true;
    public PostProcessProfile customPostProcessProfile;
    public string customMusic;
    public bool isActivated { get; private set; }
    public bool rerollsShopUponEntering = true;
    [Header("Dynamic Map Elements Settings")]
    public float iterationDelay = 0.5f;
    [ReorderableList]
    //[InfoBox("Null element will delay the next element until all previous elements are marked finished.")]
    public List<MapElement> mapElements;

    private Light[] roomLights;

    [Button("Add Stray Map Elements")]
    private void AddStrayMapElements()
    {
        MapElement[] elements = GetComponentsInChildren<MapElement>();
        for (int i = 0; i < elements.Length; i++)
        {
            if (!mapElements.Contains(elements[i])) mapElements.Add(elements[i]);
        }
    }


    private void Awake()
    {
        roomLights = GetComponentsInChildren<Light>();
        if (entryPoint == null) entryPoint = transform.Find("Entry Point");
        for (int i = 0; i < roomLights.Length; i++) roomLights[i].enabled = false;
    }

    private void Start()
    {
        Transform map = transform.Find("/Map");
        if(map != null)
        {
            transform.parent = map; 
        }
    }

    public void ToggleTheLights(bool toggle)
    {
        for (int i = 0; i < roomLights.Length; i++) roomLights[i].enabled = toggle;
    }





    public void ActivateRoom(LivingThing activator)
    {
        if (isActivated) return;
        photonView.RPC("RpcActivateRoom", RpcTarget.All, activator.photonView.ViewID);
    }


    [PunRPC]
    private void RpcActivateRoom(int activator_id)
    {
        isActivated = true;
        LivingThing activator = PhotonNetwork.GetPhotonView(activator_id).GetComponent<LivingThing>();
        if (!activator.photonView.IsMine) return;
        if (rerollsShopUponEntering) ShopManager.instance.RerollShop();
        if (customMusic != null) Music.Play(customMusic);
        StartCoroutine(CoroutineElement(activator));
        ToggleTheLights(true);

        if (customPostProcessProfile != null)
        {
            Camera.main.GetComponent<PostProcessVolume>().profile = customPostProcessProfile;
        }

        Camera.main.GetComponent<AuraCamera>().enabled = enableAuraCamera;

    }


    private IEnumerator CoroutineElement(LivingThing activator)
    {
        int i = 0;
        yield return new WaitForSeconds(iterationDelay);
        while (i < mapElements.Count)
        {
            mapElements[i].Activate();
            do
            {
                yield return new WaitForSeconds(iterationDelay);
            }
            while (!mapElements[i].isFinished);
            i++;
        }


    }



}
