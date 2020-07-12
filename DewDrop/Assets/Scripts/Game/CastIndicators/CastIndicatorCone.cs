using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CastIndicatorCone : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Required References")]
    private SpriteRenderer _arcRenderer;
    [SerializeField, FoldoutGroup("Required References")]
    private Transform _sideLeft;
    [SerializeField, FoldoutGroup("Required References")]
    private Transform _sideRight;

    private Material _arcMaterial;

    public void Show(Vector3 pivot, Vector3 direction, float radius, float angle)
    {
        if(_arcMaterial == null)
        {
            _arcMaterial = Instantiate(_arcRenderer.sharedMaterial);
            _arcRenderer.sharedMaterial = _arcMaterial;
        }

        _arcMaterial.SetFloat("_Angle", 270f);
        _arcMaterial.SetFloat("_Arc1", 180f - angle / 2f);
        _arcMaterial.SetFloat("_Arc2", 180f - angle / 2f);

        transform.position = pivot;
        transform.localScale = Vector3.one * radius * 2f;
        transform.rotation = Quaternion.LookRotation(direction).Flattened();
        _sideLeft.localRotation = Quaternion.Euler(90f, -angle / 2f, 0f);
        _sideRight.localRotation = Quaternion.Euler(90f, angle / 2f, 0f);
    }
}
