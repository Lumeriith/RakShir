using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererBoundDetectingCube : MonoBehaviour
{
    public Renderer target;

    void Update()
    {
        if (target == null) return;

        transform.position = target.bounds.center;
        transform.localScale = target.bounds.size;
    }
}
