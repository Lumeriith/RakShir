using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingThingBase : MonoBehaviour
{
    LivingThing livingThing;

    DecalSystem.Decal decal;

    public Material ownBase;
    public Material enemyBase;
    public Material allyBase;
    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        decal = transform.Find("Base Decal").GetComponent<DecalSystem.Decal>();
    }

    private void Update()
    {
        if (GameManager.instance.localPlayer == null) return;

        Relation relation = GameManager.instance.localPlayer.GetRelationTo(livingThing);
        switch (relation)
        {
            case Relation.Own:
                decal.Material = ownBase;
                break;
            case Relation.Enemy:
                decal.Material = enemyBase;
                break;
            case Relation.Ally:
                decal.Material = allyBase;
                break;
        }
    }

}
