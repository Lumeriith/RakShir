using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitFrame : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Required References")]
    private HealthFill _playerHealthFill;
    [SerializeField, FoldoutGroup("Required References")]
    private ManaFill _playerManaFill;

    private void Awake()
    {
        GameManager.OnLocalPlayerSpawn += (Entity entity) =>
        {
            _playerHealthFill.Setup(entity);
            _playerManaFill.Setup(entity);
        };
    }



}
