using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EntityBehaviour : MonoBehaviourPun
{
    public Entity entity
    {
        get
        {
            if (_cachedEntity == null) _cachedEntity = GetComponent<Entity>();
            return _cachedEntity;
        }
    }

    private Entity _cachedEntity;
}
