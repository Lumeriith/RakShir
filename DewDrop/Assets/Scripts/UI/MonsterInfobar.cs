using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterInfobar : MonoBehaviour, IInfobar
{
    private Entity target;

    private new Renderer renderer
    {
        get
        {
            if (_renderer == null) _renderer = target.transform.Find("Model").GetComponentInChildren<Renderer>();
            return _renderer;
        }
    }
    private Renderer _renderer;

    public Vector3 worldOffset;
    public Vector3 UIOffset;

    private UniversalHealthbar uhb;


    private CanvasGroup canvasGroup;

    public void SetTarget(Entity target)
    {
        this.target = target;
    }

    private void Awake()
    {
        uhb = GetComponentInChildren<UniversalHealthbar>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (target.IsDead() || !renderer.isVisible)
        {
            canvasGroup.alpha = 0f;
            uhb.enabled = false;
            return;
        }
        else
        {
            uhb.enabled = true;
            canvasGroup.alpha = 1f;
        }
        
        uhb.SetTarget(target);
        transform.position = Camera.main.WorldToScreenPoint(target.transform.position + renderer.bounds.size.y * Vector3.up + worldOffset) + UIOffset;
    }
}
