using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayerSpell : MonoBehaviour
{
    public Vector3 flatPosition
    {
        get
        {
            Vector3 pos = transform.position;
            pos.y = 0;
            return pos;
        }
    }

    private Player player;
    private Camera m_mainCamera;
    private NavMeshAgent m_nma;

    private Spell pendingSpell;

    public Spell QSpell;
    public Spell WSpell;
    public Spell ESpell;
    public Spell RSpell;

    public List<Spell> availableSpells;

    [Header("Preconfigurations")]
    [SerializeField]
    private LayerMask groundMask;
    public Spell moveSpell;
    public Spell attackSpell;

    [Header("Debug")]
    [SerializeField]
    private Spell reservedSpell;
    [SerializeField]
    private Vector3 reservedSpellFowardVector;
    [SerializeField]
    private Vector3 reservedSpellPoint;
    [SerializeField]
    private Collider reservedSpellTarget;



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




    private void Update()
    {
        foreach(Spell spell in availableSpells)
        {
            spell.remainingCooldown = Mathf.MoveTowards(spell.remainingCooldown, 0, Time.deltaTime);
        }

        if(reservedSpell != null)
        {
            TryCastReservedSpell();
        }
        else
        {
            m_nma.destination = transform.position;
        }
    }

    private void TryCastReservedSpell()
    {
        
        Spell spellInstance = null;
        switch (reservedSpell.targetingType)
        {
            case Spell.SpellTargetingType.None:
                if (reservedSpell.remainingCooldown > 0) return;
                spellInstance = Instantiate(reservedSpell.gameObject, flatPosition, Quaternion.identity).GetComponent<Spell>();
                spellInstance.gameObject.SetActive(true);
                break;
            case Spell.SpellTargetingType.Direction:
                if (reservedSpell.remainingCooldown > 0) return;
                spellInstance = Instantiate(reservedSpell.gameObject, flatPosition, Quaternion.identity).GetComponent<Spell>();
                spellInstance.forwardVector = reservedSpellFowardVector;
                spellInstance.gameObject.SetActive(true);
                break;
            case Spell.SpellTargetingType.Target:
                Vector3 targetPos = reservedSpellTarget.transform.position;
                targetPos.y = 0;
                if (Vector3.Distance(flatPosition, targetPos) <= reservedSpell.range)
                {
                    m_nma.destination = transform.position;
                    if (reservedSpell.remainingCooldown > 0) return;
                    spellInstance = Instantiate(reservedSpell.gameObject, flatPosition, Quaternion.identity).GetComponent<Spell>();
                    spellInstance.target = reservedSpellTarget;
                    spellInstance.gameObject.SetActive(true);

                }
                else
                {
                    m_nma.destination = targetPos;
                }
                break;
            case Spell.SpellTargetingType.PointNonStrict:
                if (reservedSpell.remainingCooldown > 0) return;
                spellInstance = Instantiate(reservedSpell.gameObject, flatPosition, Quaternion.identity).GetComponent<Spell>();
                spellInstance.point = reservedSpellPoint;
                spellInstance.gameObject.SetActive(true);
                break;
            case Spell.SpellTargetingType.PointStrict:
                if (Vector3.Distance(flatPosition, reservedSpellPoint) <= reservedSpell.range)
                {
                    m_nma.destination = transform.position;
                    if (reservedSpell.remainingCooldown > 0) return;
                    spellInstance = Instantiate(reservedSpell.gameObject, flatPosition, Quaternion.identity).GetComponent<Spell>();
                    spellInstance.point = reservedSpellPoint;
                    spellInstance.gameObject.SetActive(true);

                }
                else
                {
                    m_nma.destination = reservedSpellPoint;
                }
                break;
        }

        if(spellInstance != null)
        {
            reservedSpell.remainingCooldown = reservedSpell.cooldown;
            if (!reservedSpell.isBasicAttack)
            {
                reservedSpell = null;
            }
       
        }

        
    }


    private void IndicateInvalidAction()
    {

    }


}
