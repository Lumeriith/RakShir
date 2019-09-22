using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitControlManager : MonoBehaviour
{
    public LivingThing selectedUnit;


    public SpellCastType spellCastSettings = SpellCastType.Normal;
    private InputState inputState = InputState.None;
    private Spell pendingSpell;
    private KeyCode pendingSpellActivationKey;

    private Camera mainCamera;

    [Header("Preconfigurations")]
    public LayerMask basicattackableTargets;


    public enum SpellCastType { Normal, Quick, OnRelease }
    private enum InputState { None, OnReleaseCastPending, NormalCastPending, ContinuousCastPending }

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

    private bool TryReserveSpell(Spell spell)
    {
        if (spell == null) return false;
        Vector3 flatPosition = selectedUnit.transform.position;
        flatPosition.y = 0;

        switch (spell.targetingType)
        {
            case Spell.SpellTargetingType.Direction:
                Vector3 directionVector = GetCurrentCursorPositionInWorldSpace() - flatPosition;
                directionVector.y = 0;
                directionVector.Normalize();
                selectedUnit.spell.ReserveSpell_Direction(spell, directionVector);
                return true;
            case Spell.SpellTargetingType.None:
                selectedUnit.spell.ReserveSpell_None(spell);
                return true;
            case Spell.SpellTargetingType.PointNonStrict:
                Vector3 differenceVector = GetCurrentCursorPositionInWorldSpace() - flatPosition;
                differenceVector = Vector3.ClampMagnitude(differenceVector, spell.range);
                selectedUnit.spell.ReserveSpell_Point(spell, flatPosition + differenceVector);
                return true;
            case Spell.SpellTargetingType.PointStrict:
                selectedUnit.spell.ReserveSpell_Point(spell, GetCurrentCursorPositionInWorldSpace());
                return true;
            case Spell.SpellTargetingType.Target:
                RaycastHit hit;
                Ray cursorRay = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(cursorRay, out hit, 100, spell.targetMask))
                {
                    selectedUnit.spell.ReserveSpell_Target(spell, hit.collider);
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
            if (TryReserveSpell(selectedUnit.spell.attackSpell))
            {
                inputState = InputState.None;
            }
            else
            {
                TryReserveSpell(selectedUnit.spell.moveSpell);
                pendingSpell = selectedUnit.spell.moveSpell;
                inputState = InputState.ContinuousCastPending;
                pendingSpellActivationKey = KeyCode.Mouse1;
            }
        }
    }

    private void DoAttackKeyActions()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            pendingSpell = selectedUnit.spell.attackSpell;
            inputState = InputState.NormalCastPending;
        }
    }

    void Update()
    {
        if(selectedUnit == null)
        {
            pendingSpell = null;
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
        PlayerSpell ps = selectedUnit.GetComponent<PlayerSpell>();
        if (ps == null) return;
        if (ps.spellKeyBindings.Length <= index || ps.spellKeyBindings[index] == null) return;
        if (!ps.spellKeyBindings[index].isCooledDown)
        {
            IndicateCooldown(index);
            return;
        }

        pendingSpellActivationKey = activationKey;

        switch (spellCastSettings)
        {
            case SpellCastType.Normal:
                if(ps.spellKeyBindings[index].targetingType == Spell.SpellTargetingType.None)
                {
                    TryReserveSpell(ps.spellKeyBindings[index]);
                }
                else
                {
                    pendingSpell = ps.spellKeyBindings[index];
                    inputState = InputState.NormalCastPending;
                }
                break;
            case SpellCastType.Quick:
                TryReserveSpell(ps.spellKeyBindings[index]);
                break;
            case SpellCastType.OnRelease:
                if (ps.spellKeyBindings[index].targetingType == Spell.SpellTargetingType.None)
                {
                    TryReserveSpell(ps.spellKeyBindings[index]);
                }
                else
                {
                    pendingSpell = ps.spellKeyBindings[index];
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
            case InputState.ContinuousCastPending:
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
                    if (TryReserveSpell(pendingSpell))
                    {
                        inputState = InputState.None;
                        pendingSpell = null;
                    }
                }
                break;
            case InputState.OnReleaseCastPending:
                if (Input.GetKeyUp(pendingSpellActivationKey))
                {
                    TryReserveSpell(pendingSpell);
                    inputState = InputState.None;
                    pendingSpell = null;
                }
                break;
            case InputState.ContinuousCastPending:
                if (Input.GetKey(pendingSpellActivationKey))
                {
                    TryReserveSpell(pendingSpell);
                }
                else
                {
                    inputState = InputState.None;
                    pendingSpell = null;
                }
                break;
        }
    }
}
