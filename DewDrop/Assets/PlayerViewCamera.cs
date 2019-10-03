using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class PlayerViewCamera : MonoBehaviour
{
    CinemachineVirtualCamera cvc;
    // Start is called before the first frame update
    private void Awake()
    {
        cvc = GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        if(GameManager.instance.localPlayer != null)
        {
            cvc.Follow = GameManager.instance.localPlayer.transform;
        }
    }



}
