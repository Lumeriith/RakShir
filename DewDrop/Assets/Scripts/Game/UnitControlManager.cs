using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Doozy.Engine.Nody;
using Doozy.Engine.Nody.Nodes;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;
using System.Reflection;

public enum CastInputMethod { Normal, Quick, OnRelease }

public class UnitControlManager : SingletonBehaviour<UnitControlManager>
{
    public Entity selectedUnit;

    [Header("General Key Configurations")]
    public KeyCode confirmKey = KeyCode.Mouse0;
    public KeyCode actionKey = KeyCode.Mouse1;

    public KeyCode attackKey = KeyCode.A;
    public CastInputMethod attackInputMethod = CastInputMethod.Normal;
    public KeyCode stopKey = KeyCode.S;

    public KeyCode reservationModifier = KeyCode.LeftShift;
    public KeyCode selfCastModifier = KeyCode.LeftAlt;

    public KeyCode activateKey = KeyCode.G;

    [Header("Skill Key Configurations")]
    public KeyCode weaponSkillKey = KeyCode.Q;
    public CastInputMethod weaponSkillInputMethod = CastInputMethod.OnRelease;
    public KeyCode armorSkillKey = KeyCode.W;
    public CastInputMethod armorSkillInputMethod = CastInputMethod.OnRelease;
    public KeyCode bootsSkillKey = KeyCode.E;
    public CastInputMethod bootsSkillInputMethod = CastInputMethod.OnRelease;
    public KeyCode weaponUltimateSkillKey = KeyCode.R;
    public CastInputMethod weaponUltimateInputMethod = CastInputMethod.OnRelease;
    public KeyCode ringSkillKey = KeyCode.Space;
    public CastInputMethod ringSkillInputMethod = CastInputMethod.OnRelease;

    [Header("Item Key Configurations")]
    public KeyCode item1Key = KeyCode.Alpha1;
    public KeyCode item2Key = KeyCode.Alpha2;
    public KeyCode item3Key = KeyCode.Alpha3;

    public CastInputMethod itemInputMethod = CastInputMethod.OnRelease;

    [Header("Outline Settings")]
    public TargetValidator canDrawOutline;
    public Color selfOutlineColor;
    public Color allyOutlineColor;
    public Color enemyOutlineColor;

    public GameObject commandMarkerAttackMove;
    public GameObject commandMarkerAttack;
    public GameObject commandMarkerMove;
    public GameObject commandMarkerInterest;

    private InputState _inputState = InputState.None;

    private ICastMethod _pendingCastMethod;
    private ICastable _pendingCastable;

    private Camera _mainCamera;

    private GraphController _nodyGraphControlelr;
    private CanvasGroup _debugLogWindow;

    private enum InputState { None, HoldingMove, PendingCast }

    private Activatable GetFirstActivatable()
    {
        Ray cursorRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
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

    private Entity GetFirstValidTarget(TargetValidator tv)
    {
        Ray cursorRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(cursorRay, 100, LayerMask.GetMask("LivingThing"));
        IEnumerable<RaycastHit> byDistance = hits.OrderBy(hit => hit.distance);
        foreach(RaycastHit hit in hits)
        {
            Entity lt = hit.collider.GetComponent<Entity>();
            if (lt == null || lt.IsDead()) continue;
            if(tv.Evaluate(selectedUnit, lt))
            {
                return lt;
            }
        }
        return null;
    }



    private Vector3 GetCurrentCursorPositionInWorldSpace()
    {
        Ray cursorRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
        float targetY = selectedUnit.transform.position.y;
        return cursorRay.origin - (cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y)) * (cursorRay.origin.y - targetY) / cursorRay.origin.y;
    }

    /*
    private bool CommandCastableOnContext(ICastable castable, bool isReservation)
    {
        CastInfo info = new CastInfo
        {
            owner = selectedUnit,
            point = Vector3.zero,
            directionVector = Vector3.zero,
            target = null
        };

        switch (castable.castMethod.type)
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
                Entity result = GetFirstValidTarget(trigger.targetValidator);
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
    */

    private void Awake()
    {
        _mainCamera = Camera.main;
        _nodyGraphControlelr = FindObjectOfType<GraphController>();

        IngameDebugConsole.DebugLogManager dbg = FindObjectOfType<IngameDebugConsole.DebugLogManager>();
        if (dbg != null) _debugLogWindow = dbg.transform.Find("DebugLogWindow").GetComponent<CanvasGroup>();

        GameManager.instance.OnLivingThingInstantiate += (Entity thing) =>
        {
            if (thing.type == LivingThingType.Player && thing.photonView.IsMine && selectedUnit == null) selectedUnit = thing;
        };
    }

    private MeshOutline previousOutline;
    private void DrawAppropriateOutline()
    {
        Entity target = GetFirstValidTarget(canDrawOutline);

        if (target != null)
        {
            if (previousOutline != null)
            {
                previousOutline.OutlineMode = MeshOutline.Mode.SilhouetteOnly;
                if (previousOutline.GetComponentInParent<Entity>().type != LivingThingType.Player) previousOutline.enabled = false; // TODO: Optimize This.
            }
            previousOutline = target.outline;


            if (target.outline != null)
            {
                target.outline.OutlineMode = MeshOutline.Mode.OutlineAndSilhouette;
                target.outline.enabled = true;
            }
        }
        else
        {
            if (previousOutline != null)
            {
                previousOutline.OutlineMode = MeshOutline.Mode.SilhouetteOnly;
                if (previousOutline.GetComponentInParent<Entity>().type != LivingThingType.Player) previousOutline.enabled = false;
                previousOutline = null;
            }
        }
    }

    private void DisableOutline()
    {
        if (previousOutline != null)
        {
            previousOutline.OutlineMode = MeshOutline.Mode.SilhouetteOnly;
            previousOutline = null;
        }
    }


    private void Update()
    {
        if (selectedUnit == null) return;

        bool shouldTakeInputs = _debugLogWindow.alpha == 0 && GameManager.cachedCurrentNodeType == IngameNodeType.Ingame;

        if (!shouldTakeInputs)
        {
            _inputState = InputState.None;
            _pendingCastable = null;
            DisableOutline();
        }
        else
        {
            bool isReserveKeyPressed = Input.GetKey(reservationModifier);
            DrawAppropriateOutline();
            CheckForActivate();
            CheckForAttack();
            CheckForNewCast();
            CheckForStop();
            CheckForGameViewEvents();
            SetAppropriateCursor();

            if(_inputState == InputState.HoldingMove) selectedUnit.control.CommandMove(GetCurrentCursorPositionInWorldSpace());
        }
    }


    private bool IsPointerHoveringGameView()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, results);
        if (results.Count == 0) return false;
        results.Sort((x, y) => x.distance.CompareTo(y.distance));
        return results[0].gameObject.CompareTag(Tags.GameView);
    }

    private void CheckForGameViewEvents()
    {
        if (Input.GetKeyDown(actionKey))
        {
            if (!IsPointerHoveringGameView()) return;

            ClearInputState();

            if (selectedUnit.control.skillSet[0] != null)
            {
                Entity target = GetFirstValidTarget(selectedUnit.control.skillSet[0].targetValidator);
                if (target != null)
                {
                    selectedUnit.control.CommandChase(target, Input.GetKey(reservationModifier));
                    Instantiate(commandMarkerAttack, target.transform.position, Quaternion.identity, target.transform);
                    return;
                }
            }

            Activatable activatable = GetFirstActivatable();
            if (activatable != null)
            {
                selectedUnit.control.CommandActivate(activatable, Input.GetKey(reservationModifier));
                Instantiate(commandMarkerInterest, activatable.transform.position, Quaternion.identity, activatable.transform);
                return;
            }

            _inputState = InputState.HoldingMove;
            Vector3 pos = GetCurrentCursorPositionInWorldSpace();
            selectedUnit.control.CommandMove(pos, true);
            Instantiate(commandMarkerMove, pos, Quaternion.identity);
        }
        else if (Input.GetKeyUp(actionKey) || _inputState == InputState.HoldingMove)
        {
            ClearInputState();
        }
        else if (Input.GetKeyDown(confirmKey) && _inputState == InputState.PendingCast)
        {
            if (!IsPointerHoveringGameView()) return;
            _pendingCastMethod.OnCastConfirmKeyDown(Input.mousePosition);

        }
        else if (Input.GetKeyUp(confirmKey) && _inputState == InputState.PendingCast)
        {
            _pendingCastMethod.OnCastConfirmKeyUp(Input.mousePosition);
        }
    }


    private void CheckForActivate()
    {
        if (!Input.GetKeyDown(activateKey)) return;

        Collider[] colliders;
        colliders = Physics.OverlapSphere(selectedUnit.transform.position, 5f, LayerMask.GetMask("Activatable"));
        if (colliders.Length != 0)
        {
            colliders = colliders.OrderBy(collider => Vector3.Distance(selectedUnit.transform.position, collider.transform.position)).ToArray();
            _pendingCastable = null;
            selectedUnit.control.CommandActivate(colliders[0].GetComponent<Activatable>(), Input.GetKey(reservationModifier));
            ClearInputState();
        }
    }

    private void ClearInputState()
    {
        if(_inputState == InputState.PendingCast)
        {
            _pendingCastMethod.CancelCast();
            _pendingCastMethod = null;
            _pendingCastable = null;
        }
        _inputState = InputState.None;
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
        if (!Input.GetKeyDown(attackKey)) return;
        if (selectedUnit.control.skillSet[0] == null) return;
        ClearInputState();
        _pendingCastMethod = CastMethodTargetOrPoint.instance;
        _pendingCastMethod.StartCast(selectedUnit.control.skillSet[0].castMethod, attackInputMethod, attackKey, OnAttackCastInfoYielded, OnCastCanceled);
    }

    private void CheckForNewCast()
    {
        // Not using else-ifs to take account in castables in uncastable states...
        if (Input.GetKeyDown(weaponSkillKey)) CastableButtonPressed(selectedUnit.control.skillSet[1], weaponSkillInputMethod, weaponSkillKey);
        if (Input.GetKeyDown(armorSkillKey)) CastableButtonPressed(selectedUnit.control.skillSet[2], armorSkillInputMethod, armorSkillKey);
        if (Input.GetKeyDown(bootsSkillKey)) CastableButtonPressed(selectedUnit.control.skillSet[3], bootsSkillInputMethod, bootsSkillKey);
        if (Input.GetKeyDown(weaponUltimateSkillKey)) CastableButtonPressed(selectedUnit.control.skillSet[4], weaponUltimateInputMethod, weaponUltimateSkillKey);
        if (Input.GetKeyDown(ringSkillKey)) CastableButtonPressed(selectedUnit.control.skillSet[5], ringSkillInputMethod, ringSkillKey);

        bool item1KeyPressed = Input.GetKeyDown(item1Key);
        bool item2KeyPressed = Input.GetKeyDown(item2Key);
        bool item3KeyPressed = Input.GetKeyDown(item3Key);
        
        if((item1KeyPressed || item2KeyPressed || item3KeyPressed) && selectedUnit.TryGetComponent(out PlayerInventory inventory))
        {
            if (item1KeyPressed) CastableButtonPressed(inventory.consumableBelt[0], itemInputMethod, item1Key);
            if (item2KeyPressed) CastableButtonPressed(inventory.consumableBelt[0], itemInputMethod, item2Key);
            if (item3KeyPressed) CastableButtonPressed(inventory.consumableBelt[0], itemInputMethod, item3Key);
        }
    }

    private void CastableButtonPressed(ICastable castable, CastInputMethod inputMethod, KeyCode button)
    {
        if (!castable.castMethod.selfValidator.Evaluate(selectedUnit) || !castable.IsReady()) return;
        _pendingCastable = castable;

        ICastMethod usedMethod = null;
        switch (castable.castMethod.type)
        {
            case CastMethodType.None:
                usedMethod = CastMethodNone.instance;
                break;
            case CastMethodType.Cone:
                usedMethod = CastMethodCone.instance;
                break;
            case CastMethodType.Arrow:
                usedMethod = CastMethodArrow.instance;
                break;
            case CastMethodType.Target:
                usedMethod = CastMethodTarget.instance;
                break;
            case CastMethodType.Point:
                usedMethod = CastMethodPoint.instance;
                break;
        }
        ClearInputState();
        _inputState = InputState.PendingCast;
        usedMethod?.StartCast(castable.castMethod, inputMethod, button, OnCastInfoYielded, OnCastCanceled);
    }

    private void OnCastInfoYielded(CastInfo info)
    {
        selectedUnit.control.CommandCast(_pendingCastable, info, Input.GetKey(reservationModifier));
    }

    private void OnAttackCastInfoYielded(CastInfo info)
    {
        if (info.target != null) selectedUnit.control.CommandAttack(info.target, Input.GetKey(reservationModifier));
        else selectedUnit.control.CommandAttackMove(info.point, Input.GetKey(reservationModifier));
    }

    private void OnCastCanceled()
    {
        _pendingCastable = null;
        _inputState = InputState.None;
    }

    private void SetAppropriateCursor()
    {
        switch (_inputState)
        {
            case InputState.None:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
                break;
            case InputState.HoldingMove:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
                break;
            case InputState.PendingCast:
                GameManager.instance.cursorShape = GameManager.CursorShapeType.Normal;
                break;
        }
    }

}
