using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class UnitControlManager : MonoBehaviour
{
    public LivingThing selectedUnit;

    [Header("General Key Configurations")]
    public KeyCode castConfirmKey = KeyCode.Mouse0;
    public KeyCode actionKey = KeyCode.Mouse1;

    public KeyCode attackKey = KeyCode.A;
    public KeyCode stopKey = KeyCode.S;

    public KeyCode reservationModifier = KeyCode.LeftShift;
    public KeyCode selfCastModifier = KeyCode.LeftAlt;

    [Header("Skill Key Configurations")]
    public KeyCode WeaponSkillKey = KeyCode.Q;
    public AbilityCastMethod WeaponSkillCastMethod = AbilityCastMethod.OnRelease;
    public KeyCode ArmorSkillKey = KeyCode.W;
    public AbilityCastMethod ArmorSkillCastMethod = AbilityCastMethod.OnRelease;
    public KeyCode BootsSkillKey = KeyCode.E;
    public AbilityCastMethod BootsSkillCastMethod = AbilityCastMethod.OnRelease;
    public KeyCode WeaponUltimateSkillKey = KeyCode.R;
    public AbilityCastMethod WeaponUltimateSkillCastMethod = AbilityCastMethod.OnRelease;
    public KeyCode RingSkillKey = KeyCode.Space;
    public AbilityCastMethod RingSkillCastMethod = AbilityCastMethod.OnRelease;

    [Header("Item Key Configurations")]
    public KeyCode Item1Key = KeyCode.Alpha1;
    public KeyCode Item2Key = KeyCode.Alpha2;
    public KeyCode Item3Key = KeyCode.Alpha3;
    public KeyCode Item4Key = KeyCode.Alpha4;
    public KeyCode Item5Key = KeyCode.Alpha5;
    public KeyCode Item6Key = KeyCode.Alpha6;

    public AbilityCastMethod ItemUseMethod = AbilityCastMethod.OnRelease;

    private InputState inputState = InputState.None;

    private AbilityTrigger pendingTrigger;
    private Consumable pendingConsumable;
    private KeyCode pendingTriggerActivationKey;

    private Camera mainCamera;
    private DecalSystem.Decal rangeIndicator;
    private DecalSystem.Decal arrowHead;
    private DecalSystem.Decal arrowBase;


    public enum AbilityCastMethod { Normal, Quick, OnRelease }
    private enum InputState { None, ContinousMove, Attack, PendingOnReleaseCast, PendingNormalCast }

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

    private Activatable GetFirstActivatable()
    {
        Ray cursorRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(cursorRay, 100, LayerMask.GetMask("Activatable"));
        IEnumerable<RaycastHit> byDistance = hits.OrderBy(hit => hit.distance);
        foreach (RaycastHit hit in hits)
        {
            Activatable act = hit.collider.GetComponent<Activatable>();
            if (act == null) continue;
            return act;
        }
        return null;
    }


    private LivingThing GetFirstValidTarget(TargetValidator tv)
    {
        Ray cursorRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(cursorRay, 100, LayerMask.GetMask("LivingThing"));
        IEnumerable<RaycastHit> byDistance = hits.OrderBy(hit => hit.distance);
        foreach(RaycastHit hit in hits)
        {
            LivingThing lt = hit.collider.GetComponent<LivingThing>();
            if (lt == null) continue;
            if(tv.Evaluate(selectedUnit, lt))
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

        if(Physics.Raycast(cursorRay, out hit, 100, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }
        else
        {
            return cursorRay.origin - cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y);
        }

    }

    private bool CommandAbilityOnContext(AbilityTrigger trigger, bool isReservation)
    {
        if (trigger == null) return false;
        CastInfo info = new CastInfo
        {
            owner = selectedUnit,
            point = Vector3.zero,
            directionVector = Vector3.zero,
            target = null
        };

        switch (trigger.targetingType)
        {
            case AbilityTrigger.TargetingType.Direction:
                info.directionVector = GetCurrentCursorPositionInWorldSpace() - selectedUnit.transform.position;
                info.directionVector.y = 0;
                info.directionVector.Normalize();

                selectedUnit.control.CommandAbility(trigger, info, isReservation);
                return true;
            case AbilityTrigger.TargetingType.None:
                selectedUnit.control.CommandAbility(trigger, info, isReservation);
                return true;
            case AbilityTrigger.TargetingType.PointNonStrict:
                info.point = GetCurrentCursorPositionInWorldSpace();
                selectedUnit.control.CommandAbility(trigger, info, isReservation);
                return true;
            case AbilityTrigger.TargetingType.PointStrict:
                info.point = GetCurrentCursorPositionInWorldSpace();
                selectedUnit.control.CommandAbility(trigger, info, isReservation);
                return true;
            case AbilityTrigger.TargetingType.Target:
                LivingThing result = GetFirstValidTarget(trigger.targetValidator);
                if (result != null)
                {
                    info.target = result;
                    selectedUnit.control.CommandAbility(trigger, info, isReservation);
                    return true;
                }
                else
                {
                    return false;
                }
        }
        return false;
    }
    private bool CommandConsumableOnContext(Consumable consumable, bool isReservation)
    {
        if (consumable == null) return false;
        CastInfo info = new CastInfo
        {
            owner = selectedUnit,
            point = Vector3.zero,
            directionVector = Vector3.zero,
            target = null
        };

        switch (consumable.targetingType)
        {
            case AbilityTrigger.TargetingType.Direction:
                info.directionVector = GetCurrentCursorPositionInWorldSpace() - selectedUnit.transform.position;
                info.directionVector.y = 0;
                info.directionVector.Normalize();

                selectedUnit.control.CommandConsumable(consumable, info, isReservation);
                return true;
            case AbilityTrigger.TargetingType.None:
                selectedUnit.control.CommandConsumable(consumable, info, isReservation);
                return true;
            case AbilityTrigger.TargetingType.PointNonStrict:
                info.point = GetCurrentCursorPositionInWorldSpace();
                selectedUnit.control.CommandConsumable(consumable, info, isReservation);
                return true;
            case AbilityTrigger.TargetingType.PointStrict:
                info.point = GetCurrentCursorPositionInWorldSpace();
                selectedUnit.control.CommandConsumable(consumable, info, isReservation);
                return true;
            case AbilityTrigger.TargetingType.Target:
                LivingThing result = GetFirstValidTarget(consumable.targetValidator);
                if (result != null)
                {
                    info.target = result;
                    selectedUnit.control.CommandConsumable(consumable, info, isReservation);
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


    

    void Update()
    {
        if (selectedUnit == null) return;

        bool isReserveKeyPressed = Input.GetKey(reservationModifier);
        
        
        CheckForAction();
        CheckForAttack();
        CheckForNewCast();
        CheckForItem();

        switch (inputState)
        {
            case InputState.None:
                break;
            case InputState.Attack:
                if (Input.GetKeyDown(castConfirmKey))
                {
                    LivingThing target = selectedUnit.control.skillSet[0] != null ? GetFirstValidTarget(selectedUnit.control.skillSet[0].targetValidator) : null;
                    if(target != null)
                    {
                        selectedUnit.control.CommandChase(target, isReserveKeyPressed);
                        inputState = InputState.None;
                    }
                    else
                    {
                        selectedUnit.control.CommandAttackMove(GetCurrentCursorPositionInWorldSpace(), isReserveKeyPressed);
                        inputState = InputState.None;
                    }
                }
                break;
            case InputState.ContinousMove:
                if (Input.GetKeyUp(actionKey))
                {
                    inputState = InputState.None;
                }
                else
                {
                    selectedUnit.control.CommandMove(GetCurrentCursorPositionInWorldSpace(), isReserveKeyPressed);
                }
                break;
            case InputState.PendingNormalCast:
                if (Input.GetKeyDown(castConfirmKey))
                {

                    bool commandSuccess = pendingTrigger != null ? CommandAbilityOnContext(pendingTrigger, isReserveKeyPressed) : CommandConsumableOnContext(pendingConsumable, isReserveKeyPressed);
                    if (commandSuccess)
                    {
                        inputState = InputState.None;
                        pendingTrigger = null;
                    }
                }
                break;
            case InputState.PendingOnReleaseCast:
                if (Input.GetKeyUp(pendingTriggerActivationKey) || Input.GetKeyDown(castConfirmKey))
                {
                    if (pendingTrigger != null)
                    {
                        CommandAbilityOnContext(pendingTrigger, isReserveKeyPressed);
                    }
                    else
                    {
                        CommandConsumableOnContext(pendingConsumable, isReserveKeyPressed);
                    }
                    pendingTrigger = null;
                }
                break;
        }

        CheckForStop();

        SetAppropriateCursor();
        DisplayAppropriateIndicator();
    }


    private void CheckForStop()
    {
        if (Input.GetKey(stopKey))
        {
            selectedUnit.control.CommandStop();
        }
    }

    private void CheckForAttack()
    {
        if (Input.GetKeyDown(attackKey))
        {
            pendingTrigger = null;
            inputState = InputState.Attack;
        }
    }

    private void CheckForNewCast()
    {
        if (Input.GetKeyDown(WeaponSkillKey))
        {
            CastButtonPressed(selectedUnit.control.skillSet[1], WeaponSkillCastMethod, WeaponSkillKey);
        }
        if (Input.GetKeyDown(ArmorSkillKey))
        {
            CastButtonPressed(selectedUnit.control.skillSet[2], ArmorSkillCastMethod, ArmorSkillKey);
        }
        if (Input.GetKeyDown(BootsSkillKey))
        {
            CastButtonPressed(selectedUnit.control.skillSet[3], BootsSkillCastMethod, BootsSkillKey);
        }
        if (Input.GetKeyDown(WeaponUltimateSkillKey))
        {
            CastButtonPressed(selectedUnit.control.skillSet[4], WeaponUltimateSkillCastMethod, WeaponUltimateSkillKey);
        }
        if (Input.GetKeyDown(RingSkillKey))
        {
            CastButtonPressed(selectedUnit.control.skillSet[5], RingSkillCastMethod, RingSkillKey);
        }
    }

    private void CheckForItem()
    {
        if (Input.GetKeyDown(Item1Key))
        {
            ItemButtonPressed(1, ItemUseMethod, Item1Key);
        }
        if (Input.GetKeyDown(Item2Key))
        {
            ItemButtonPressed(2, ItemUseMethod, Item2Key);
        }
        if (Input.GetKeyDown(Item3Key))
        {
            ItemButtonPressed(3, ItemUseMethod, Item3Key);
        }
        if (Input.GetKeyDown(Item4Key))
        {
            ItemButtonPressed(4, ItemUseMethod, Item4Key);
        }
        if (Input.GetKeyDown(Item5Key))
        {
            ItemButtonPressed(5, ItemUseMethod, Item5Key);
        }
        if (Input.GetKeyDown(Item6Key))
        {
            ItemButtonPressed(6, ItemUseMethod, Item6Key);
        }
    }



    private void IndicateCooldown(AbilityTrigger trigger)
    {

    }

    private void ItemButtonPressed(int itemIndex, AbilityCastMethod method, KeyCode button)
    {
        PlayerItemBelt belt = selectedUnit.GetComponent<PlayerItemBelt>();

        if (belt == null) return;
        if (belt.consumables[itemIndex - 1] == null) return;
        if (!belt.consumables[itemIndex - 1].selfValidator.Evaluate(selectedUnit)) return;

        if (belt.consumables[itemIndex - 1].targetingType == AbilityTrigger.TargetingType.None)
        {
            selectedUnit.control.CommandConsumable(belt.consumables[itemIndex - 1], new CastInfo() { owner = selectedUnit }, Input.GetKey(reservationModifier));
            return;
        }
        switch (method)
        {
            case AbilityCastMethod.Normal:
                inputState = InputState.PendingNormalCast;
                pendingTrigger = null;
                pendingConsumable = belt.consumables[itemIndex - 1];
                break;
            case AbilityCastMethod.OnRelease:
                inputState = InputState.PendingOnReleaseCast;
                pendingTrigger = null;
                pendingConsumable = belt.consumables[itemIndex - 1];
                pendingTriggerActivationKey = button;
                break;
            case AbilityCastMethod.Quick:
                CommandConsumableOnContext(belt.consumables[itemIndex - 1], Input.GetKey(reservationModifier));
                break;
        }
    }


    private void CastButtonPressed(AbilityTrigger trigger, AbilityCastMethod method, KeyCode button)
    {
        if (trigger == null) return;
        if (!trigger.isCooledDown)
        {
            IndicateCooldown(trigger);
            return;
        }
        if (!trigger.selfValidator.Evaluate(selectedUnit)) return;

        if (trigger.targetingType == AbilityTrigger.TargetingType.None)
        {
            selectedUnit.control.CommandAbility(trigger, new CastInfo() { owner = selectedUnit }, Input.GetKey(reservationModifier));
            return;
        }
        switch (method)
        {
            case AbilityCastMethod.Normal:
                inputState = InputState.PendingNormalCast;
                pendingTrigger = trigger;
                break;
            case AbilityCastMethod.OnRelease:
                inputState = InputState.PendingOnReleaseCast;
                pendingTrigger = trigger;
                pendingTriggerActivationKey = button;
                break;
            case AbilityCastMethod.Quick:
                CommandAbilityOnContext(trigger, Input.GetKey(reservationModifier));
                break;
        }
    }



    private void CheckForAction()
    {
        if (Input.GetKeyDown(actionKey))
        {
            if(selectedUnit.control.skillSet[0] == null)
            {
                Activatable act = GetFirstActivatable();
                if (act != null)
                {
                    pendingTrigger = null;
                    inputState = InputState.None;
                    selectedUnit.control.CommandActivate(act, Input.GetKey(reservationModifier));
                } else if (Input.GetKey(reservationModifier))
                {
                    pendingTrigger = null;
                    inputState = InputState.None;
                    selectedUnit.control.CommandMove(GetCurrentCursorPositionInWorldSpace(), true);
                }
                else
                {
                    pendingTrigger = null;
                    inputState = InputState.ContinousMove;
                    selectedUnit.control.CommandMove(GetCurrentCursorPositionInWorldSpace(), false);
                }
            }
            else
            {
                LivingThing target = GetFirstValidTarget(selectedUnit.control.skillSet[0].targetValidator);
                Activatable act = GetFirstActivatable();

                if (target != null)
                {
                    pendingTrigger = null;
                    inputState = InputState.None;
                    selectedUnit.control.CommandChase(target, Input.GetKey(reservationModifier));
                }
                else if (act != null)
                {
                    pendingTrigger = null;
                    inputState = InputState.None;
                    selectedUnit.control.CommandActivate(act, Input.GetKey(reservationModifier));
                }
                else
                {
                    if (Input.GetKey(reservationModifier))
                    {
                        pendingTrigger = null;
                        inputState = InputState.None;
                        selectedUnit.control.CommandMove(GetCurrentCursorPositionInWorldSpace(), true);
                    }
                    else
                    {
                        pendingTrigger = null;
                        inputState = InputState.ContinousMove;
                        selectedUnit.control.CommandMove(GetCurrentCursorPositionInWorldSpace(), false);
                    }
                }
            }




        }
    }

    private void DisplayAppropriateIndicator()
    {
        if (pendingTrigger == null || pendingTrigger.indicator.type == IndicatorType.None)
        {
            rangeIndicator.gameObject.SetActive(false);
            arrowHead.gameObject.SetActive(false);
            arrowBase.gameObject.SetActive(false);
        }
        else if(inputState == InputState.Attack)
        {
            rangeIndicator.transform.position = selectedUnit.transform.position;
            rangeIndicator.gameObject.SetActive(true);
            arrowHead.gameObject.SetActive(false);
            arrowBase.gameObject.SetActive(false);
            rangeIndicator.transform.localScale = new Vector3(selectedUnit.control.skillSet[0].indicator.range * 2, selectedUnit.control.skillSet[0].indicator.range * 2, 4);

        }
        else if(pendingTrigger.indicator.type == IndicatorType.Range)
        {
            rangeIndicator.transform.position = selectedUnit.transform.position;
            rangeIndicator.gameObject.SetActive(true);
            arrowHead.gameObject.SetActive(false);
            arrowBase.gameObject.SetActive(false);
            rangeIndicator.transform.localScale = new Vector3(pendingTrigger.indicator.range * 2, pendingTrigger.indicator.range * 2, 4);
        } else if (pendingTrigger.indicator.type == IndicatorType.Arrow)
        {
            rangeIndicator.gameObject.SetActive(false);
            arrowHead.gameObject.SetActive(true);
            arrowBase.gameObject.SetActive(true);

            Vector3 headScale = Vector3.one;
            Vector3 rotation = new Vector3(90, 0, 0);
            Vector3 baseScale = Vector3.one;
            Vector3 cursorPos = GetCurrentCursorPositionInWorldSpace();
            rotation.z = -Quaternion.LookRotation(cursorPos - selectedUnit.transform.position, Vector3.up).eulerAngles.y;

            baseScale.x = pendingTrigger.indicator.arrowWidth;
            baseScale.y = pendingTrigger.indicator.arrowLength - 1f;
            baseScale.z = 4f;

            headScale.x = pendingTrigger.indicator.arrowWidth;
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


    private void SetAppropriateCursor()
    {
        switch (inputState)
        {
            case InputState.None:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
                break;
            case InputState.Attack:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Attack;
                break;
            case InputState.ContinousMove:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
                break;
            case InputState.PendingNormalCast:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
                break;
            case InputState.PendingOnReleaseCast:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
                break;
        }
    }

}
