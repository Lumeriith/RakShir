using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;
public class PlayerViewCamera : MonoBehaviour
{
    CinemachineVirtualCamera cvc;
    // Start is called before the first frame update
    public Vector2 possibleCameraDistance;
    public float cameraDistanceStepSize;

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

        if(Input.mouseScrollDelta.y < 0)
        {
            CinemachineFramingTransposer cft = cvc.GetCinemachineComponent<CinemachineFramingTransposer>();
            cft.m_CameraDistance = Mathf.MoveTowards(cft.m_CameraDistance, possibleCameraDistance.y, cameraDistanceStepSize);
        } else if (Input.mouseScrollDelta.y > 0)
        {
            CinemachineFramingTransposer cft = cvc.GetCinemachineComponent<CinemachineFramingTransposer>();
            cft.m_CameraDistance = Mathf.MoveTowards(cft.m_CameraDistance, possibleCameraDistance.x, cameraDistanceStepSize);
        }
    }



}
