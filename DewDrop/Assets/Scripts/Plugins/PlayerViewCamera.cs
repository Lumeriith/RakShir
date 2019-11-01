using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;
public class PlayerViewCamera : MonoBehaviour
{

    // Start is called before the first frame update
    public Vector2 desiredCameraDistance;
    public Vector2 deadzoneCameraDistance;
    public float cameraDistanceStepSize;
    public float clampingSpeed;

    private CinemachineFramingTransposer cft;
    private CinemachineVirtualCamera cvc;

    private void Awake()
    {
        cvc = GetComponent<CinemachineVirtualCamera>();
        cft = cvc.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private void Update()
    {
        if (GameManager.instance.localPlayer != null)
        {
            cvc.Follow = GameManager.instance.localPlayer.transform;
        }

        if (Input.mouseScrollDelta.y < 0)
        {
            cft.m_CameraDistance = Mathf.MoveTowards(cft.m_CameraDistance, deadzoneCameraDistance.y, cameraDistanceStepSize);
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            cft.m_CameraDistance = Mathf.MoveTowards(cft.m_CameraDistance, deadzoneCameraDistance.x, cameraDistanceStepSize);
        }
        if (cft.m_CameraDistance > desiredCameraDistance.y) cft.m_CameraDistance = Mathf.MoveTowards(cft.m_CameraDistance, desiredCameraDistance.y, clampingSpeed * Time.deltaTime);
        if (cft.m_CameraDistance < desiredCameraDistance.x) cft.m_CameraDistance = Mathf.MoveTowards(cft.m_CameraDistance, desiredCameraDistance.x, clampingSpeed * Time.deltaTime);

    }




}
