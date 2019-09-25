using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitControlManager : MonoBehaviour
{
    public LivingThing selectedUnit;


    public AbilityInstanceCastType spellCastSettings = AbilityInstanceCastType.Normal;
    private InputState inputState = InputState.None;

    private AbilityTrigger pendingAbilityTrigger;
    private KeyCode pendingAbilityTriggerActivationKey;

    private Camera mainCamera;

    [Header("Preconfigurations")]
    public LayerMask basicattackableTargets;


    public enum AbilityInstanceCastType { Normal, Quick, OnRelease }
    private enum InputState { None, OnReleaseCastPending, NormalCastPending, ContinuousMove, AttackMove }

    private static UnitControlManager _instance;
    public static UnitControlManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UnitControlManager>();
            }
            return _instance;
        }
    }

    Vector3 GetCurrentCursorPositionInWorldSpace()
    {
        Ray cursorRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        return cursorRay.origin - cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y);
    }

    private bool TryReserveAbilityTrigger(AbilityTrigger abilityTrigger)
    {
        if (abilityTrigger == null) return false;
        Vector3 flatPosition = selectedUnit.transform.position;
        flatPosition.y = 0;

        switch (abilityTrigger.targetingType)
        {
            case AbilityTrigger.TargetingType.Direction:
                Vector3 directionVector = GetCurrentCursorPositionInWorldSpace() - flatPosition;
                directionVector.y = 0;
                directionVector.Normalize();
                selectedUnit.control.ReserveAbilityTrigger(abilityTrigger, Vector3.zero, directionVector, null);
                return true;
            case AbilityTrigger.TargetingType.None:
                selectedUnit.control.ReserveAbilityTrigger(abilityTrigger, Vector3.zero, Vector3.zero, null);
                return true;
            case AbilityTrigger.TargetingType.PointNonStrict:
                Vector3 differenceVector = GetCurrentCursorPositionInWorldSpace() - flatPosition;
                differenceVector = Vector3.ClampMagnitude(differenceVector, abilityTrigger.range);
                selectedUnit.control.ReserveAbilityTrigger(abilityTrigger, flatPosition + differenceVector, Vector3.zero, null);
                return true;
            case AbilityTrigger.TargetingType.PointStrict:
                selectedUnit.control.ReserveAbilityTrigger(abilityTrigger, GetCurrentCursorPositionInWorldSpace(), Vector3.zero, null);
                return true;
            case AbilityTrigger.TargetingType.Target:
                RaycastHit hit;
                Ray cursorRay = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(cursorRay, out hit, 100, abilityTrigger.targetMask))
                {
                    selectedUnit.control.ReserveAbilityTrigger(abilityTrigger, Vector3.zero, Vector3.zero, hit.collider.GetComponent<LivingThing>());
                    return true;
                }
                else
                {
                    return false;
                }
        }
        return false;
    }


    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void DoRightClickActions()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (TryReserveAbilityTrigger(selectedUnit.control.basicAttackAbilityTrigger))
            {
                inputState = InputState.None;
            }
            else
            {
                selectedUnit.control.StartMoving(GetCurrentCursorPositionInWorldSpace());
                inputState = InputState.ContinuousMove;
                pendingAbilityTriggerActivationKey = KeyCode.Mouse1;
            }
        }
    }

    private void DoAttackKeyActions()
    {
        if (Input.GetKeyDown(KeyCode.A) && selectedUnit.control.basicAttackAbilityTrigger != null)
        {
            pendingAbilityTrigger = selectedUnit.control.basicAttackAbilityTrigger;
            inputState = InputState.AttackMove;
        }
    }

    void Update()
    {
        if(selectedUnit == null)
        {
            pendingAbilityTrigger = null;
            inputState = InputState.None;
            return;
        }

        DoRightClickActions();
        DoAttackKeyActions();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            AbilityInstanceKeyPressed(0, KeyCode.Q);
        } else if (Input.GetKeyDown(KeyCode.W))
        {
            AbilityInstanceKeyPressed(1, KeyCode.W);
        } else if (Input.GetKeyDown(KeyCode.E))
        {
            AbilityInstanceKeyPressed(2, KeyCode.E);
        } else if (Input.GetKeyDown(KeyCode.R))
        {
            AbilityInstanceKeyPressed(3, KeyCode.R);
        }

        HandleReserveInputEvents();

        SetAppropriateCursor();
    }
    private void IndicateCooldown(int index)
    {

    }
    private void AbilityInstanceKeyPressed(int index, KeyCode activationKey)
    {
        if (selectedUnit.control.keybindings.Length <= index || selectedUnit.control.keybindings[index] == null) return;
        if (!selectedUnit.control.keybindings[index].isCooledDown)
        {
            IndicateCooldown(index);
            return;
        }

        pendingAbilityTriggerActivationKey = activationKey;

        switch (spellCastSettings)
        {
            case AbilityInstanceCastType.Normal:
                if(selectedUnit.control.keybindings[index].targetingType == AbilityTrigger.TargetingType.None)
                {
                    TryReserveAbilityTrigger(selectedUnit.control.keybindings[index]);
                }
                else
                {
                    pendingAbilityTrigger = selectedUnit.control.keybindings[index];
                    inputState = InputState.NormalCastPending;
                }
                break;
            case AbilityInstanceCastType.Quick:
                TryReserveAbilityTrigger(selectedUnit.control.keybindings[index]);
                break;
            case AbilityInstanceCastType.OnRelease:
                if (selectedUnit.control.keybindings[index].targetingType == AbilityTrigger.TargetingType.None)
                {
                    TryReserveAbilityTrigger(selectedUnit.control.keybindings[index]);
                }
                else
                {
                    pendingAbilityTrigger = selectedUnit.control.keybindings[index];
                    inputState = InputState.OnReleaseCastPending;
                }
                break;
        }
    }

    private void SetAppropriateCursor()
    {
        switch (inputState)
        {
            case InputState.None:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
                break;
            case InputState.NormalCastPending:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.AbilityInstance;
                break;
            case InputState.OnReleaseCastPending:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.AbilityInstance;
                break;
            case InputState.ContinuousMove:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
                break;
            case InputState.AttackMove:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Attack;
                break;
        }
    }

    private void HandleReserveInputEvents()
    {
        switch (inputState)
        {
            case InputState.None:
                break;
            case InputState.NormalCastPending:
                if (Input.GetMouseButtonDown(0))
                {
                    if (TryReserveAbilityTrigger(pendingAbilityTrigger))
                    {
                        inputState = InputState.None;
                        pendingAbilityTrigger = null;
                    }
                }
                break;
            case InputState.OnReleaseCastPending:
                if (Input.GetKeyUp(pendingAbilityTriggerActivationKey))
                {
                    TryReserveAbilityTrigger(pendingAbilityTrigger);
                    inputState = InputState.None;
                    pendingAbilityTrigger = null;
                }
                break;
            case InputState.ContinuousMove:
                if (Input.GetKey(pendingAbilityTriggerActivationKey))
                {
                    selectedUnit.control.StartMoving(GetCurrentCursorPositionInWorldSpace());
                }
                else
                {
                    inputState = InputState.None;
                    pendingAbilityTrigger = null;
                }
                break;
            case InputState.AttackMove:
                if (Input.GetMouseButtonDown(0))
                {
                    if (TryReserveAbilityTrigger(pendingAbilityTrigger))
                    {
                        inputState = InputState.None;
                        pendingAbilityTrigger = null;
                    }
                    else
                    {
                        inputState = InputState.None;
                        selectedUnit.control.StartAttackMoving(GetCurrentCursorPositionInWorldSpace());
                        pendingAbilityTrigger = null;
                    }
                }
                break;
        }
    }
}
