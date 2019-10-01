using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class UnitControlManager : MonoBehaviour
{
    public KeyCode castConfirmKey = KeyCode.Mouse0;
    public KeyCode actionKey = KeyCode.Mouse1;

    public KeyCode attackKey = KeyCode.A;
    public KeyCode stopKey = KeyCode.S;

    public KeyCode reservationModifier = KeyCode.LeftShift;
    public KeyCode selfCastModifier = KeyCode.LeftAlt;

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

    public LivingThing selectedUnit;

    private InputState inputState = InputState.None;

    private AbilityTrigger pendingTrigger;
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

    private LivingThing GetFirstValidTarget(TargetValidator tv)
    {
        Ray cursorRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(cursorRay, 100, LayerMask.GetMask("LivingThing"));
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

    private void Awake()
    {
        mainCamera = Camera.main;
        rangeIndicator = transform.Find("Range Indicator").GetComponent<DecalSystem.Decal>();
        arrowHead = transform.Find("ArrowHead").GetComponent<DecalSystem.Decal>();
        arrowBase = transform.Find("ArrowBase").GetComponent<DecalSystem.Decal>();
    }


    

    void Update()
    {
        bool isReserveKeyPressed = Input.GetKey(reservationModifier);
        
        CheckForStop();
        CheckForNewCast();
        CheckForAction();

        switch (inputState)
        {
            case InputState.None:
                break;
            case InputState.Attack:
                if (Input.GetKeyDown(castConfirmKey))
                {
                    LivingThing target = GetFirstValidTarget(selectedUnit.control.skillSet[0].targetValidator);
                    if(target != null)
                    {
                        selectedUnit.control.CommandChase(target, isReserveKeyPressed);
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
                    bool commandSuccess = CommandAbilityOnContext(pendingTrigger, isReserveKeyPressed);
                    if (commandSuccess) inputState = InputState.None;
                }
                break;
            case InputState.PendingOnReleaseCast:
                if (Input.GetKeyUp(pendingTriggerActivationKey))
                {
                    CommandAbilityOnContext(pendingTrigger, isReserveKeyPressed);
                }
                break;
        }
        SetAppropriateCursor();
        DisplayAppropriateIndicator();
    }


    private void CheckForStop()
    {
        if (Input.GetKeyDown(stopKey))
        {
            selectedUnit.control.CommandStop();
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

    private void CastButtonPressed(AbilityTrigger trigger, AbilityCastMethod method, KeyCode button)
    {
        if (trigger.targetingType == AbilityTrigger.TargetingType.None)
        {
            selectedUnit.control.CommandAbility(trigger, new CastInfo(), Input.GetKey(reservationModifier));
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

    }

    private void DisplayAppropriateIndicator()
    {
        if (pendingTrigger == null || pendingTrigger.indicator.type == IndicatorType.None)
        {
            rangeIndicator.gameObject.SetActive(false);
            arrowHead.gameObject.SetActive(false);
            arrowBase.gameObject.SetActive(false);
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
