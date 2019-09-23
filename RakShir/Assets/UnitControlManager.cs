using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitControlManager : MonoBehaviour
{
    public LivingThing selectedUnit;


    public SpellCastType spellCastSettings = SpellCastType.Normal;
    private InputState inputState = InputState.None;

    private SpellTrigger pendingSpellTrigger;
    private KeyCode pendingSpellTriggerActivationKey;

    private Camera mainCamera;

    [Header("Preconfigurations")]
    public LayerMask basicattackableTargets;


    public enum SpellCastType { Normal, Quick, OnRelease }
    private enum InputState { None, OnReleaseCastPending, NormalCastPending, ContinuousMove }

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

    private bool TryReserveSpellTrigger(SpellTrigger spellTrigger)
    {
        if (spellTrigger == null) return false;
        Vector3 flatPosition = selectedUnit.transform.position;
        flatPosition.y = 0;

        switch (spellTrigger.targetingType)
        {
            case SpellTrigger.TargetingType.Direction:
                Vector3 directionVector = GetCurrentCursorPositionInWorldSpace() - flatPosition;
                directionVector.y = 0;
                directionVector.Normalize();
                selectedUnit.control.ReserveSpellTrigger(spellTrigger, Vector3.zero, directionVector, null);
                return true;
            case SpellTrigger.TargetingType.None:
                selectedUnit.control.ReserveSpellTrigger(spellTrigger, Vector3.zero, Vector3.zero, null);
                return true;
            case SpellTrigger.TargetingType.PointNonStrict:
                Vector3 differenceVector = GetCurrentCursorPositionInWorldSpace() - flatPosition;
                differenceVector = Vector3.ClampMagnitude(differenceVector, spellTrigger.range);
                selectedUnit.control.ReserveSpellTrigger(spellTrigger, flatPosition + differenceVector, Vector3.zero, null);
                return true;
            case SpellTrigger.TargetingType.PointStrict:
                selectedUnit.control.ReserveSpellTrigger(spellTrigger, GetCurrentCursorPositionInWorldSpace(), Vector3.zero, null);
                return true;
            case SpellTrigger.TargetingType.Target:
                RaycastHit hit;
                Ray cursorRay = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(cursorRay, out hit, 100, spellTrigger.targetMask))
                {
                    selectedUnit.control.ReserveSpellTrigger(spellTrigger, Vector3.zero, Vector3.zero, hit.collider.GetComponent<LivingThing>());
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
            if (TryReserveSpellTrigger(selectedUnit.control.basicAttackSpellTrigger))
            {
                inputState = InputState.None;
            }
            else
            {
                selectedUnit.control.StartMove(GetCurrentCursorPositionInWorldSpace());
                inputState = InputState.ContinuousMove;
                pendingSpellTriggerActivationKey = KeyCode.Mouse1;
            }
        }
    }

    private void DoAttackKeyActions()
    {
        if (Input.GetKeyDown(KeyCode.A) && selectedUnit.control.basicAttackSpellTrigger != null)
        {
            pendingSpellTrigger = selectedUnit.control.basicAttackSpellTrigger;
            inputState = InputState.NormalCastPending;
        }
    }

    void Update()
    {
        if(selectedUnit == null)
        {
            pendingSpellTrigger = null;
            inputState = InputState.None;
            return;
        }

        DoRightClickActions();
        DoAttackKeyActions();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpellKeyPressed(0, KeyCode.Q);
        } else if (Input.GetKeyDown(KeyCode.W))
        {
            SpellKeyPressed(1, KeyCode.W);
        } else if (Input.GetKeyDown(KeyCode.E))
        {
            SpellKeyPressed(2, KeyCode.E);
        } else if (Input.GetKeyDown(KeyCode.R))
        {
            SpellKeyPressed(3, KeyCode.R);
        }

        HandleReserveInputEvents();

        SetAppropriateCursor();
    }
    private void IndicateCooldown(int index)
    {

    }
    private void SpellKeyPressed(int index, KeyCode activationKey)
    {
        if (selectedUnit.control.keybindings.Length <= index || selectedUnit.control.keybindings[index] == null) return;
        if (!selectedUnit.control.keybindings[index].isCooledDown)
        {
            IndicateCooldown(index);
            return;
        }

        pendingSpellTriggerActivationKey = activationKey;

        switch (spellCastSettings)
        {
            case SpellCastType.Normal:
                if(selectedUnit.control.keybindings[index].targetingType == SpellTrigger.TargetingType.None)
                {
                    TryReserveSpellTrigger(selectedUnit.control.keybindings[index]);
                }
                else
                {
                    pendingSpellTrigger = selectedUnit.control.keybindings[index];
                    inputState = InputState.NormalCastPending;
                }
                break;
            case SpellCastType.Quick:
                TryReserveSpellTrigger(selectedUnit.control.keybindings[index]);
                break;
            case SpellCastType.OnRelease:
                if (selectedUnit.control.keybindings[index].targetingType == SpellTrigger.TargetingType.None)
                {
                    TryReserveSpellTrigger(selectedUnit.control.keybindings[index]);
                }
                else
                {
                    pendingSpellTrigger = selectedUnit.control.keybindings[index];
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
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Spell;
                break;
            case InputState.OnReleaseCastPending:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Spell;
                break;
            case InputState.ContinuousMove:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
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
                    if (TryReserveSpellTrigger(pendingSpellTrigger))
                    {
                        inputState = InputState.None;
                        pendingSpellTrigger = null;
                    }
                }
                break;
            case InputState.OnReleaseCastPending:
                if (Input.GetKeyUp(pendingSpellTriggerActivationKey))
                {
                    TryReserveSpellTrigger(pendingSpellTrigger);
                    inputState = InputState.None;
                    pendingSpellTrigger = null;
                }
                break;
            case InputState.ContinuousMove:
                if (Input.GetKey(pendingSpellTriggerActivationKey))
                {
                    selectedUnit.control.StartMove(GetCurrentCursorPositionInWorldSpace());
                }
                else
                {
                    inputState = InputState.None;
                    pendingSpellTrigger = null;
                }
                break;
        }
    }
}
