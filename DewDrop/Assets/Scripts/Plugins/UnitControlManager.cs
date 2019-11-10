using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Doozy.Engine.Nody;
using Doozy.Engine.Nody.Nodes;
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

    public KeyCode activateKey = KeyCode.G;

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
    private DecalSystem.Decal secondRangeIndicator;
    private DecalSystem.Decal arrowHead;
    private DecalSystem.Decal arrowBase;

    private GraphController nodyGraphController;

    [Header("Outline Settings")]
    public TargetValidator canDrawOutline;
    public Color selfOutlineColor;
    public Color allyOutlineColor;
    public Color enemyOutlineColor;


    private CanvasGroup debugLogWindow;

    public GameObject commandMarkerAttackMove;
    public GameObject commandMarkerAttack;
    public GameObject commandMarkerMove;
    public GameObject commandMarkerInterest;




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
            if (lt == null || lt.IsDead()) continue;
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
        float targetY = selectedUnit.transform.position.y;
        return cursorRay.origin - (cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y)) * (cursorRay.origin.y - targetY) / cursorRay.origin.y;

        /*
        
        RaycastHit hit;

        if(Physics.Raycast(cursorRay, out hit, 100, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }
        else
        {
            float targetY = selectedUnit.transform.position.y;
            return cursorRay.origin - (cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y)) * (cursorRay.origin.y - targetY) / cursorRay.origin.y;
        }
        */
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
        secondRangeIndicator = transform.Find("Second Range Indicator").GetComponent<DecalSystem.Decal>();
        arrowHead = transform.Find("ArrowHead").GetComponent<DecalSystem.Decal>();
        arrowBase = transform.Find("ArrowBase").GetComponent<DecalSystem.Decal>();
        nodyGraphController = FindObjectOfType<GraphController>();

        IngameDebugConsole.DebugLogManager dbg = FindObjectOfType<IngameDebugConsole.DebugLogManager>();
        if (dbg != null) debugLogWindow = dbg.transform.Find("DebugLogWindow").GetComponent<CanvasGroup>();

        GameManager.instance.OnLivingThingInstantiate += (LivingThing thing) =>
        {
            if (thing.type == LivingThingType.Player && thing.photonView.IsMine && selectedUnit == null) selectedUnit = thing;
        };
    }

    private MeshOutline previousOutline;
    private void DrawAppropriateOutline()
    {
        LivingThing target = GetFirstValidTarget(canDrawOutline);

        if (target != null)
        {
            if (previousOutline != null) previousOutline.enabled = false;
            previousOutline = target.outline;


            if (target.outline != null)
            {
                target.outline.enabled = true;
                switch (selectedUnit.GetRelationTo(target))
                {
                    case Relation.Own:
                        target.outline.OutlineColor = selfOutlineColor;
                        break;
                    case Relation.Ally:
                        target.outline.OutlineColor = allyOutlineColor;
                        break;
                    case Relation.Enemy:
                        target.outline.OutlineColor = enemyOutlineColor;
                        break;
                }
                target.outline.OutlineMode = MeshOutline.Mode.OutlineVisible;
                target.outline.OutlineWidth = 1.5f;
            }
        }
        else
        {
            if (previousOutline != null)
            {
                previousOutline.enabled = false;
                previousOutline = null;
            }
        }




    }
    private void DisableOutline()
    {

        if (previousOutline != null)
        {
            previousOutline.enabled = false;
            previousOutline = null;
        }



    }


    void Update()
    {
        if (selectedUnit == null) return;

        bool shouldTakeInputs = false;

        if (debugLogWindow.alpha == 0 && nodyGraphController.Graph.ActiveNode != null && nodyGraphController.Graph.ActiveNode.NodeType == Doozy.Engine.Nody.Models.NodeType.SubGraph && ((SubGraphNode)nodyGraphController.Graph.ActiveNode).SubGraph.ActiveNode.Name == "Ingame")
            shouldTakeInputs = true;
        



        if (!shouldTakeInputs)
        {
            inputState = InputState.None;
            pendingTrigger = null;
            pendingConsumable = null;
            DisableOutline();
        }
        else
        {
            bool isReserveKeyPressed = Input.GetKey(reservationModifier);
            DrawAppropriateOutline();




            switch (inputState)
            {
                case InputState.None:
                    break;
                case InputState.Attack:
                    if (Input.GetKeyDown(castConfirmKey))
                    {
                        LivingThing target = selectedUnit.control.skillSet[0] != null ? GetFirstValidTarget(selectedUnit.control.skillSet[0].targetValidator) : null;
                        if (target != null)
                        {
                            selectedUnit.control.CommandChase(target, isReserveKeyPressed);
                            Instantiate(commandMarkerAttack, target.transform.position, Quaternion.identity, target.transform);
                            inputState = InputState.None;
                        }
                        else
                        {
                            Vector3 pos = GetCurrentCursorPositionInWorldSpace();
                            selectedUnit.control.CommandAttackMove(pos, isReserveKeyPressed);
                            Instantiate(commandMarkerAttackMove, pos, Quaternion.identity);
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
                            pendingConsumable = null;
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
                        pendingConsumable = null;
                    }
                    break;
            }
            CheckForActivate();
            CheckForAction();
            CheckForAttack();
            CheckForNewCast();
            CheckForItem();
            CheckForStop();

            SetAppropriateCursor();
            DisplayAppropriateIndicator();
        }


    }

    private void CheckForActivate()
    {
        if (Input.GetKeyDown(activateKey))
        {
            Collider[] colliders;
            colliders = Physics.OverlapSphere(selectedUnit.transform.position, 5f, LayerMask.GetMask("Activatable"));
            if(colliders.Length != 0)
            {
                colliders = colliders.OrderBy(collider => Vector3.Distance(selectedUnit.transform.position, collider.transform.position)).ToArray();
                pendingTrigger = null;
                pendingConsumable = null;
                inputState = InputState.None;
                selectedUnit.control.CommandActivate(colliders[0].GetComponent<Activatable>(), Input.GetKey(reservationModifier));
            }


        }
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
            pendingConsumable = null;
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
        if (belt.consumableBelt[itemIndex - 1] == null) return;
        if (!belt.consumableBelt[itemIndex - 1].selfValidator.Evaluate(selectedUnit) || !belt.consumableBelt[itemIndex - 1].IsReady()) return;

        if (belt.consumableBelt[itemIndex - 1].targetingType == AbilityTrigger.TargetingType.None)
        {
            selectedUnit.control.CommandConsumable(belt.consumableBelt[itemIndex - 1], new CastInfo() { owner = selectedUnit }, Input.GetKey(reservationModifier));
            return;
        }
        switch (method)
        {
            case AbilityCastMethod.Normal:
                inputState = InputState.PendingNormalCast;
                pendingTrigger = null;
                pendingConsumable = belt.consumableBelt[itemIndex - 1];
                break;
            case AbilityCastMethod.OnRelease:
                inputState = InputState.PendingOnReleaseCast;
                pendingTrigger = null;
                pendingConsumable = belt.consumableBelt[itemIndex - 1];
                pendingTriggerActivationKey = button;
                break;
            case AbilityCastMethod.Quick:
                CommandConsumableOnContext(belt.consumableBelt[itemIndex - 1], Input.GetKey(reservationModifier));
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
        if (!selectedUnit.HasMana(trigger.manaCost)) return;
        if (!trigger.IsReady()) return;
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
                    pendingConsumable = null;
                    inputState = InputState.None;
                    selectedUnit.control.CommandActivate(act, Input.GetKey(reservationModifier));
                    Instantiate(commandMarkerInterest, act.transform.position, Quaternion.identity, act.transform);
                } else if (Input.GetKey(reservationModifier))
                {
                    pendingTrigger = null;
                    pendingConsumable = null;
                    inputState = InputState.None;
                    Vector3 pos = GetCurrentCursorPositionInWorldSpace();
                    selectedUnit.control.CommandMove(pos, true);
                    Instantiate(commandMarkerMove, pos, Quaternion.identity);
                }
                else
                {
                    pendingTrigger = null;
                    pendingConsumable = null;
                    inputState = InputState.ContinousMove;
                    Vector3 pos = GetCurrentCursorPositionInWorldSpace();
                    selectedUnit.control.CommandMove(pos, false);
                    Instantiate(commandMarkerMove, pos, Quaternion.identity);
                }
            }
            else
            {
                LivingThing target = GetFirstValidTarget(selectedUnit.control.skillSet[0].targetValidator);
                Activatable act = GetFirstActivatable();

                if (target != null)
                {
                    pendingTrigger = null;
                    pendingConsumable = null;
                    inputState = InputState.None;
                    selectedUnit.control.CommandChase(target, Input.GetKey(reservationModifier));
                    Instantiate(commandMarkerAttack, target.transform.position, Quaternion.identity, target.transform);
                }
                else if (act != null)
                {
                    pendingTrigger = null;
                    pendingConsumable = null;
                    inputState = InputState.None;
                    selectedUnit.control.CommandActivate(act, Input.GetKey(reservationModifier));
                    Instantiate(commandMarkerInterest, act.transform.position, Quaternion.identity, act.transform);
                }
                else
                {
                    if (Input.GetKey(reservationModifier))
                    {
                        pendingTrigger = null;
                        pendingConsumable = null;
                        inputState = InputState.None;
                        Vector3 pos = GetCurrentCursorPositionInWorldSpace();
                        selectedUnit.control.CommandMove(pos, true);
                        Instantiate(commandMarkerMove, pos, Quaternion.identity);
                    }
                    else
                    {
                        pendingTrigger = null;
                        pendingConsumable = null;
                        inputState = InputState.ContinousMove;
                        Vector3 pos = GetCurrentCursorPositionInWorldSpace();
                        selectedUnit.control.CommandMove(pos, false);
                        Instantiate(commandMarkerMove, pos, Quaternion.identity);
                    }
                }
            }




        }
    }

    private void DisplayAppropriateIndicator()
    {
        Indicator indicator = null;
        AbilityTrigger.TargetingType targetingType = AbilityTrigger.TargetingType.None;
        TargetValidator targetValidator = null;

        float targetingRange = 0f;
        if (pendingTrigger != null)
        {
            indicator = pendingTrigger.indicator;
            targetingType = pendingTrigger.targetingType;
            targetingRange = pendingTrigger.range;
            targetValidator = pendingTrigger.targetValidator;
        }
        else if (pendingConsumable != null)
        {
            indicator = pendingConsumable.indicator;
            targetingType = pendingConsumable.targetingType;
            targetingRange = pendingConsumable.range;
            targetValidator = pendingConsumable.targetValidator;
        }

        if(inputState == InputState.Attack && selectedUnit.control.skillSet[0] != null)
        {
            rangeIndicator.transform.position = selectedUnit.transform.position;
            rangeIndicator.gameObject.SetActive(true);
            secondRangeIndicator.gameObject.SetActive(false);
            arrowHead.gameObject.SetActive(false);
            arrowBase.gameObject.SetActive(false);
            rangeIndicator.transform.localScale = new Vector3(selectedUnit.control.skillSet[0].indicator.range * 2, selectedUnit.control.skillSet[0].indicator.range * 2, 4);
            
        }
        else if(indicator != null && indicator.type == IndicatorType.Range)
        {
            rangeIndicator.transform.position = selectedUnit.transform.position;
            rangeIndicator.gameObject.SetActive(true);
            if (indicator.enableSecondRangeIndicator)
            {
                secondRangeIndicator.gameObject.SetActive(true);
                if (targetingType == AbilityTrigger.TargetingType.PointNonStrict) secondRangeIndicator.transform.position = selectedUnit.transform.position + Vector3.ClampMagnitude(GetCurrentCursorPositionInWorldSpace() - selectedUnit.transform.position, targetingRange);
                else if (targetingType == AbilityTrigger.TargetingType.PointStrict) secondRangeIndicator.transform.position = GetCurrentCursorPositionInWorldSpace();
                else if (targetingType == AbilityTrigger.TargetingType.Target)
                {
                    LivingThing thing = GetFirstValidTarget(targetValidator);
                    if(thing == null)
                    {
                        secondRangeIndicator.gameObject.SetActive(false);
                    }
                    else
                    {
                        secondRangeIndicator.transform.position = thing.transform.position;
                    }
                    
                }
                secondRangeIndicator.transform.localScale = new Vector3(indicator.secondRange * 2, indicator.secondRange * 2, 4);
            }
            else
            {
                secondRangeIndicator.gameObject.SetActive(false);
            }
            arrowHead.gameObject.SetActive(false);
            arrowBase.gameObject.SetActive(false);
            rangeIndicator.transform.localScale = new Vector3(indicator.range * 2, indicator.range * 2, 4);
        } else if (indicator != null && indicator.type == IndicatorType.Arrow)
        {
            rangeIndicator.gameObject.SetActive(false);

            arrowHead.gameObject.SetActive(true);
            arrowBase.gameObject.SetActive(true);

            Vector3 headScale = Vector3.one;
            Vector3 rotation = new Vector3(90, 0, 0);
            Vector3 baseScale = Vector3.one;
            Vector3 cursorPos = GetCurrentCursorPositionInWorldSpace();
            rotation.z = -Quaternion.LookRotation(cursorPos - selectedUnit.transform.position, Vector3.up).eulerAngles.y;

            baseScale.x = indicator.arrowWidth;
            baseScale.y = indicator.arrowLength - 1f;
            baseScale.z = 4f;

            headScale.x = indicator.arrowWidth;
            headScale.y = 1f;
            headScale.z = 4f;

            arrowBase.transform.localScale = baseScale;
            arrowBase.transform.rotation = Quaternion.Euler(rotation);
            arrowBase.transform.position = selectedUnit.transform.position + (cursorPos - selectedUnit.transform.position).normalized * (baseScale.y / 2);

            arrowHead.transform.localScale = headScale;
            arrowHead.transform.rotation = Quaternion.Euler(rotation);
            arrowHead.transform.position = arrowBase.transform.position + (cursorPos - selectedUnit.transform.position).normalized * (baseScale.y / 2 + headScale.y / 2);

            if (indicator.enableSecondRangeIndicator)
            {
                secondRangeIndicator.gameObject.SetActive(true);
                secondRangeIndicator.transform.position = selectedUnit.transform.position + (cursorPos - selectedUnit.transform.position).normalized * indicator.arrowLength;
                secondRangeIndicator.transform.localScale = new Vector3(indicator.secondRange * 2, indicator.secondRange * 2, 4);
            }
            else
            {
                secondRangeIndicator.gameObject.SetActive(false);
            }

        }
        else
        {
            rangeIndicator.gameObject.SetActive(false);
            secondRangeIndicator.gameObject.SetActive(false);
            arrowHead.gameObject.SetActive(false);
            arrowBase.gameObject.SetActive(false);
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
