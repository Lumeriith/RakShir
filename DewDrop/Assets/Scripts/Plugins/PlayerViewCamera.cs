using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine.Rendering.PostProcessing;
public class PlayerViewCamera : MonoBehaviour
{
    
    // Start is called before the first frame update
    public Vector2 desiredCameraDistance;
    public Vector2 deadzoneCameraDistance;
    //public Vector2 cameraAngle;
    public float cameraDistanceStepSize;
    public float clampingSpeed;

    public float visionMultiplier = 1f;
    //public float angleSpeed;

    private CinemachineFramingTransposer cft;
    private CinemachineVirtualCamera cvc;

    private float cameraDistance;

    private static PlayerViewCamera _instance;

    private PostProcessVolume volume;

    private Camera main;

    public static PlayerViewCamera instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerViewCamera>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        cvc = GetComponent<CinemachineVirtualCamera>();
        cft = cvc.GetCinemachineComponent<CinemachineFramingTransposer>();
        volume = Camera.main.GetComponent<PostProcessVolume>();
        cameraDistance = cft.m_CameraDistance;
        main = Camera.main;
    }

    private void Update()
    {
        if (GameManager.instance.localPlayer != null)
        {
            cvc.Follow = GameManager.instance.localPlayer.transform;
        }

        if(GameManager.cachedCurrentNodeType == IngameNodeType.Ingame)
        {
            if (Input.mouseScrollDelta.y < 0)
            {
                cameraDistance = Mathf.MoveTowards(cameraDistance, deadzoneCameraDistance.y, cameraDistanceStepSize);
            }
            else if (Input.mouseScrollDelta.y > 0)
            {
                cameraDistance = Mathf.MoveTowards(cameraDistance, deadzoneCameraDistance.x, cameraDistanceStepSize);
            }

        }
        if (cameraDistance > desiredCameraDistance.y) cameraDistance = Mathf.MoveTowards(cameraDistance, desiredCameraDistance.y, clampingSpeed * Time.deltaTime);
        if (cameraDistance < desiredCameraDistance.x) cameraDistance = Mathf.MoveTowards(cameraDistance, desiredCameraDistance.x, clampingSpeed * Time.deltaTime);

        cft.m_CameraDistance = cameraDistance * visionMultiplier;
        if (volume == null || volume.profile == null || cvc.Follow == null) return;
        DepthOfField dof;
        volume.profile.TryGetSettings(out dof);
        if (dof != null) dof.focusDistance.value = Vector3.Distance(main.transform.position, cvc.Follow.position);

        //Vector3 euler = transform.rotation.eulerAngles;
        //float targetAngle = (cft.m_CameraDistance - desiredCameraDistance.x) / (desiredCameraDistance.y - desiredCameraDistance.x) * (cameraAngle.y - cameraAngle.x) + cameraAngle.x;
        //euler.x = Mathf.MoveTowards(euler.x, targetAngle, angleSpeed * Time.deltaTime)  ;
        //transform.rotation = Quaternion.Euler(euler);
    }




}
