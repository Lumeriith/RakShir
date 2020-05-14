using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColliderEntityExtension
{
    public static bool IsAppropriateEntity(this Collider collider, Entity self, TargetValidator validator, out Entity entity)
    {
        if (collider == null)
        {
            entity = null;
            return false;
        }
        entity = collider.GetComponent<Entity>();
        if (entity == null) return false;
        return validator.Evaluate(self, entity);
    }
}
