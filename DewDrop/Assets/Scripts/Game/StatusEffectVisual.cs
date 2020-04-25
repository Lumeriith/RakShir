using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class StatusEffectVisual : MonoBehaviour
{
    public enum AttachPosition { AboveHead, Center };

    public AttachPosition position;
    public StatusEffectType type;
    
    private Entity _target;

    public void Attach(Entity to)
    {
        _target = to;
        transform.SetParent(to.transform);
        switch (position)
        {
            case AttachPosition.AboveHead:
                transform.position = to.top.position;
                break;
            case AttachPosition.Center:
                transform.position = to.transform.position + to.GetCenterOffset();
                break;
        }
        transform.rotation = to.transform.rotation;
    }



    private void FixedUpdate()
    {
        if(_target == null || _target.IsDead() || !_target.IsAffectedBy(type))
        {
            Destroy(gameObject);
        }
    }

}
