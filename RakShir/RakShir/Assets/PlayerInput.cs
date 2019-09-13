using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    [SerializeField]
    private LayerMask groundMask;

    private Camera m_mainCamera;
    private PlayerMotor motor;

    public LayerMask basicAttackableTargets;
    [Header("Prefab Assignments")]
    [SerializeField]
    private GameObject moveCommandMarkerPrefab;
    [SerializeField]
    private GameObject attackCommandMarkerPrefab;

    private InputHoldState currentInputHoldState = InputHoldState.None;
    private enum InputHoldState { None, Move, Attack}

    private bool isAttackCommandPending = false;

    private void Awake()
    {
        m_mainCamera = Camera.main;
        motor = GetComponent<PlayerMotor>();
    }

    GameObject GetObjectUnderCursor(LayerMask layerMask)
    {
        Ray cursorRay = m_mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(cursorRay, out hit, 100, layerMask))
        {
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    Vector3 GetCurrentCursorPositionInWorldSpace()
    {
        Ray cursorRay = m_mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(cursorRay, out hit, 100, groundMask))
        {
            return hit.point;
        }
        else
        {
            return cursorRay.origin - cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            isAttackCommandPending = true;
            GameManager.instance.SetAttackCursor();
        }


        if (Input.GetMouseButtonDown(0) && isAttackCommandPending)
        {
            GameObject targetObject = GetObjectUnderCursor(basicAttackableTargets);
            if(targetObject != null)
            {
                motor.IssueAttackCommand(targetObject);
                Instantiate(attackCommandMarkerPrefab, targetObject.transform.position, targetObject.transform.rotation, targetObject.transform);
            }
            else
            {
                Vector3 targetPosition = GetCurrentCursorPositionInWorldSpace();
                motor.IssueAttackMoveCommand(targetPosition);
                Instantiate(attackCommandMarkerPrefab, targetPosition, Quaternion.identity);
            }
            isAttackCommandPending = false;
            GameManager.instance.SetNormalCursor();
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (isAttackCommandPending)
            {
                isAttackCommandPending = false;
                GameManager.instance.SetNormalCursor();
            }
            else
            {
                GameObject targetObject = GetObjectUnderCursor(basicAttackableTargets);
                if (targetObject == null)
                {
                    currentInputHoldState = InputHoldState.Move;
                    Vector3 targetPosition = GetCurrentCursorPositionInWorldSpace();
                    motor.IssueMoveCommand(targetPosition);
                    Instantiate(moveCommandMarkerPrefab, targetPosition, Quaternion.identity);
                }
                else
                {
                    currentInputHoldState = InputHoldState.Attack;
                    motor.IssueAttackCommand(targetObject);
                    Instantiate(attackCommandMarkerPrefab, targetObject.transform.position, targetObject.transform.rotation, targetObject.transform);
                }
            }
        }
        else if (Input.GetMouseButton(1))
        {
            switch (currentInputHoldState)
            {
                case InputHoldState.Attack:
                    break;
                case InputHoldState.Move:
                    motor.IssueMoveCommand(GetCurrentCursorPositionInWorldSpace());
                    break;
                case InputHoldState.None:
                    break;
            }
        }
        else
        {
            currentInputHoldState = InputHoldState.None;
        }


    }
}
