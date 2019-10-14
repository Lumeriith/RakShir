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
        GameObject decalObject = new GameObject("Base Decal");
        decalObject.transform.parent = transform;
        decalObject.transform.position = transform.position;
        decalObject.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        decal = decalObject.AddComponent<DecalSystem.Decal>();
        decal.LayerMask = LayerMask.GetMask("Ground");
    }

    private bool isBaseSet = false;
    private Relation previousRelation;

    private void FixedUpdate()
    {
        if (GameManager.instance.localPlayer == null) return;
        Relation relation = GameManager.instance.localPlayer.GetRelationTo(livingThing);
        decal.gameObject.SetActive(livingThing.IsAlive());
        if (livingThing.IsAlive() && (!isBaseSet || previousRelation != relation)) UpdateBase(relation);
    }

    private void UpdateBase(Relation relation)
    {
        isBaseSet = true;
        previousRelation = relation;
        switch (relation)
        {
            case Relation.Own:
                if(ownBase == null)
                {
                    decal.Material = null;
                    decal.Sprite = null;
                    return;
                }
                decal.Material = ownBase;
                break;
            case Relation.Enemy:
                if (enemyBase == null)
                {
                    decal.Material = null;
                    decal.Sprite = null;
                    return;
                }
                decal.Material = enemyBase;
                break;
            case Relation.Ally:
                if (allyBase == null)
                {
                    decal.Material = null;
                    decal.Sprite = null;
                    return;
                }
                decal.Material = allyBase;
                break;
        }

        Texture tex = ownBase.GetTexture("_MainTex");
        decal.Sprite = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
    }

}
