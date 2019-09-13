using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerControl : MonoBehaviour
{
    public enum CommandType { None, Move, Attack }

    private Camera m_mainCamera;
    private NavMeshAgent m_nma;

    public CommandType currentCommand = CommandType.None;
    public Vector3 moveCommandWorldPosition = Vector3.zero;

    [Header("Prefab Assignments")]
    [SerializeField]
    private GameObject moveCommandMarkerPrefab;

    void Start()
    {
        m_mainCamera = Camera.main;
        m_nma = GetComponent<NavMeshAgent>();
    }


    Vector3 GetCurrentCursorPositionInWorldSpace()
    {
        Ray cursorRay = m_mainCamera.ScreenPointToRay(Input.mousePosition);
        return cursorRay.origin - cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y);
    }



    void Update()
    {

        if (Input.GetMouseButton(1))
        {
            currentCommand = CommandType.Move;
            moveCommandWorldPosition = GetCurrentCursorPositionInWorldSpace();
            if (Input.GetMouseButtonDown(1))
            {
                Instantiate(moveCommandMarkerPrefab, moveCommandWorldPosition, Quaternion.identity);
                
            }
            m_nma.destination = moveCommandWorldPosition;
        }

        switch (currentCommand)
        {
            case CommandType.Attack:
                break;
        }


    }
}
