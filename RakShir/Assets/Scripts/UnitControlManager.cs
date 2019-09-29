using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class UnitControlManager : MonoBehaviour
{
    public LivingThing selectedUnit;


    public AbilityInstanceCastType spellCastSettings = AbilityInstanceCastType.Normal;
    private InputState inputState = InputState.None;

    private AbilityTrigger pendingAbilityTrigger;
    private KeyCode pendingAbilityTriggerActivationKey;

    private Camera mainCamera;
    private DecalSystem.Decal rangeIndicator;
    private DecalSystem.Decal arrowHead;
    private DecalSystem.Decal arrowBase;

    [Header("Preconfigurations")]
    public LayerMask maskLivingThing;
    public LayerMask maskGroundMask;

    public enum AbilityInstanceCastType { Normal, Quick, OnRelease }
    private enum InputState { None, OnReleaseCastPending, NormalCastPending, ContinuousMove, AttackMove, RightClickAttackPressed }

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

    private LivingThing GetFirstValidTarget(TargetValidator tv)
    {
        Ray cursorRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(cursorRay, 100, maskLivingThing);
        IEnumerable<RaycastHit> byDistance = hits.OrderBy(hit => hit.distance);
        foreach(RaycastHit hit in hits)
        {
            LivingThing lt = hit.collider.GetComponent<LivingThing>();
            if(tv.Evaluate(selectedUnit, hit.collider.GetComponent<LivingThing>()))
            {
                return lt;
            }
        }
        return null;
    }


    Vector3 GetCurrentCursorPositionInWorldSpace()
    {
        Ray cursorRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(cursorRay, out hit, 100, maskGroundMask))
        {
            return hit.point;
        }
        else
        {
            return cursorRay.origin - cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y);
        }

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
                LivingThing result = GetFirstValidTarget(abilityTrigger.targetValidator);
                if (result != null)
                {
                    selectedUnit.control.ReserveAbilityTrigger(abilityTrigger, Vector3.zero, Vector3.zero, result);
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
        rangeIndicator = transform.Find("Range Indicator").GetComponent<DecalSystem.Decal>();
        arrowHead = transform.Find("ArrowHead").GetComponent<DecalSystem.Decal>();
        arrowBase = transform.Find("ArrowBase").GetComponent<DecalSystem.Decal>();
    }

    private void DoRightClickActions()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (TryReserveAbilityTrigger(selectedUnit.control.basicAttackAbilityTrigger))
            {
                inputState = InputState.RightClickAttackPressed;
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

        DisplayAppropriateIndicator();
    }


    private void DisplayAppropriateIndicator()
    {
        if (pendingAbilityTrigger == null || pendingAbilityTrigger.indicator.type == IndicatorType.None)
        {
            rangeIndicator.gameObject.SetActive(false);
            arrowHead.gameObject.SetActive(false);
            arrowBase.gameObject.SetActive(false);
        }
        else if(pendingAbilityTrigger.indicator.type == IndicatorType.Range)
        {
            rangeIndicator.transform.position = selectedUnit.transform.position;
            rangeIndicator.gameObject.SetActive(true);
            arrowHead.gameObject.SetActive(false);
            arrowBase.gameObject.SetActive(false);
            rangeIndicator.transform.localScale = new Vector3(pendingAbilityTrigger.indicator.range * 2, pendingAbilityTrigger.indicator.range * 2, 4);
        } else if (pendingAbilityTrigger.indicator.type == IndicatorType.Arrow)
        {
            rangeIndicator.gameObject.SetActive(false);
            arrowHead.gameObject.SetActive(true);
            arrowBase.gameObject.SetActive(true);

            Vector3 headScale = Vector3.one;
            Vector3 rotation = new Vector3(90, 0, 0);
            Vector3 baseScale = Vector3.one;
            Vector3 cursorPos = GetCurrentCursorPositionInWorldSpace();
            rotation.z = -Quaternion.LookRotation(cursorPos - selectedUnit.transform.position, Vector3.up).eulerAngles.y;

            baseScale.x = pendingAbilityTrigger.indicator.arrowWidth;
            baseScale.y = pendingAbilityTrigger.indicator.arrowLength - 1f;
            baseScale.z = 4f;

            headScale.x = pendingAbilityTrigger.indicator.arrowWidth;
            headScale.y = 1f;
            headScale.z = 4f;

            arrowBase.transform.localScale = baseScale;
            arrowBase.transform.rotation = Quaternion.Euler(rotation);
            arrowBase.transform.position = selectedUnit.transform.position + (cursorPos - selectedUnit.transform.position).normalized * (baseScale.y / 2);

            arrowHead.transform.localScale = headScale;
            arrowHead.transform.rotation = Quaternion.Euler(rotation);
            arrowHead.transform.position = arrowBase.transform.position + (cursorPos - selectedUnit.transform.position).normalized * (baseScale.y / 2 + headScale.y / 2);
            

            

        }


    }

    private void IndicateCooldown(int index)
    {

    }

    private void IndicateCantActivate(int index)
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

        if (!selectedUnit.control.keybindings[index].CanActivate())
        {
            IndicateCantActivate(index);
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
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    inputState = InputState.ContinuousMove;
                    pendingAbilityTriggerActivationKey = KeyCode.Mouse1;
                }
                break;
            case InputState.RightClickAttackPressed:
                if (Input.GetKeyUp(KeyCode.Mouse1)){
                    inputState = InputState.None;
                }
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
                    selectedUnit.CommandMove(GetCurrentCursorPositionInWorldSpace());
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
                        selectedUnit.CommandAttackMove(GetCurrentCursorPositionInWorldSpace());
                        pendingAbilityTrigger = null;
                    }
                }
                break;
        }
    }
}
