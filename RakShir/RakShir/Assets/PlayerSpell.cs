using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayerSpell : MonoBehaviour
{
    private Player player;
    private Camera m_mainCamera;
    private NavMeshAgent m_nma;

    private Spell pendingSpell;

    private Spell reservedSpell;
    private Vector3 reservedSpellFowardVector;
    private Vector3 reservedSpellPoint;
    private Collider reservedSpellTarget;

    public List<Spell> availableSpells;

    [Header("Preconfigurations")]
    [SerializeField]
    private LayerMask groundMask;

    Vector3 GetCurrentCursorPositionInWorldSpace()
    {
        Ray cursorRay = m_mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(cursorRay, out hit, 100, groundMask))
        {
            return hit.point;
        }
        else
        {
            return cursorRay.origin - cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y);
        }
    }

    void Awake()
    {
        player = GetComponent<Player>();
        m_nma = GetComponent<NavMeshAgent>();
        m_mainCamera = Camera.main;
    }

    public void ReserveSpell_None(Spell spell)
    {
        reservedSpell = spell;
    }
    public void ReserveSpell_Target(Spell spell, Collider target)
    {
        reservedSpell = spell;
        reservedSpellTarget = target;
    }
    public void ReserveSpell_Direction(Spell spell, Vector3 forwardVector)
    {
        reservedSpell = spell;
        reservedSpellFowardVector = forwardVector;
    }

    public void ReserveSpell_Point(Spell spell, Vector3 point)
    {
        reservedSpell = spell;
        reservedSpellPoint = point;
    }

    private bool TryToReserveSpell(Spell spell)
    {
        switch (spell.targetingType)
        {
            case Spell.SpellTargetingType.Direction:
                Vector3 directionVector = GetCurrentCursorPositionInWorldSpace() - transform.position;
                directionVector.y = 0;
                directionVector.Normalize();
                ReserveSpell_Direction(spell, directionVector);
                return true;
            case Spell.SpellTargetingType.None:
                ReserveSpell_None(spell);
                return true;
            case Spell.SpellTargetingType.PointNonStrict:
                Vector3 differenceVector = GetCurrentCursorPositionInWorldSpace() - transform.position;
                differenceVector = Vector3.ClampMagnitude(differenceVector, spell.range);
                ReserveSpell_Point(spell, transform.position + differenceVector);
                return true;
            case Spell.SpellTargetingType.PointStrict:
                ReserveSpell_Point(spell, GetCurrentCursorPositionInWorldSpace());
                return true;
            case Spell.SpellTargetingType.Target:
                RaycastHit hit;
                Ray cursorRay = m_mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(cursorRay, out hit, 100, spell.targetMask))
                {
                    ReserveSpell_Target(spell, hit.collider);
                    return true;
                }
                else
                {
                    return false;
                }
        }
        return false;
    }


    private void Update()
    {



        foreach(Spell spell in availableSpells)
        {
            if (Input.GetKeyDown(spell.activationKey))
            {
                if (spell.isCooledDown && spell.CanBeCast())
                {
                     pendingSpell = spell;
                }
                else
                {
                    IndicateInvalidAction();
                }
            }
        }

        if(pendingSpell != null)
        {
            if (pendingSpell.castType == Spell.SpellCastType.Normal && Input.GetMouseButtonDown(0))
            {
                if (TryToReserveSpell(pendingSpell))
                {
                    pendingSpell = null;
                }
            } else if (pendingSpell.castType == Spell.SpellCastType.OnRelease && Input.GetKeyUp(pendingSpell.activationKey)){
                if (TryToReserveSpell(pendingSpell))
                {
                    pendingSpell = null;
                }
            } else if (pendingSpell.castType == Spell.SpellCastType.Quick)
            {
                if (TryToReserveSpell(pendingSpell))
                {
                    pendingSpell = null;
                }
            }
        }

        if(reservedSpell != null)
        {
            Spell spellInstance;
            switch (reservedSpell.targetingType)
            {
                case Spell.SpellTargetingType.None:
                    spellInstance = Instantiate(reservedSpell.gameObject, transform.position, Quaternion.identity).GetComponent<Spell>();
                    spellInstance.gameObject.SetActive(true);
                    reservedSpell = null;
                    break;
                case Spell.SpellTargetingType.Direction:
                    spellInstance = Instantiate(reservedSpell.gameObject, transform.position, Quaternion.identity).GetComponent<Spell>();
                    spellInstance.forwardVector = reservedSpellFowardVector;
                    spellInstance.gameObject.SetActive(true);
                    reservedSpell = null;
                    break;
                case Spell.SpellTargetingType.Target:
                    if (Vector3.Distance(transform.position, reservedSpellTarget.transform.position) <= reservedSpell.range)
                    {
                        spellInstance = Instantiate(reservedSpell.gameObject, transform.position, Quaternion.identity).GetComponent<Spell>();
                        spellInstance.forwardVector = reservedSpellFowardVector;
                        spellInstance.gameObject.SetActive(true);
                        reservedSpell = null;

                    }
                    else
                    {
                        m_nma.destination = reservedSpellTarget.transform.position;
                    }
                    break;
                case Spell.SpellTargetingType.PointNonStrict:
                    spellInstance = Instantiate(reservedSpell.gameObject, transform.position, Quaternion.identity).GetComponent<Spell>();
                    spellInstance.point = reservedSpellPoint;
                    spellInstance.gameObject.SetActive(true);
                    reservedSpell = null;
                    break;
                case Spell.SpellTargetingType.PointStrict:
                    if (Vector3.Distance(transform.position, reservedSpellPoint) <= reservedSpell.range)
                    {
                        spellInstance = Instantiate(reservedSpell.gameObject, transform.position, Quaternion.identity).GetComponent<Spell>();
                        spellInstance.point = reservedSpellPoint;
                        spellInstance.gameObject.SetActive(true);
                        reservedSpell = null;

                    }
                    else
                    {
                        m_nma.destination = reservedSpellPoint;
                    }
                    break;
            }
        }



    }




    private void IndicateInvalidAction()
    {

    }


}
