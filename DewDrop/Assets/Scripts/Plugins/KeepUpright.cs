using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepUpright : MonoBehaviour
{

    private void Awake()
    {
        transform.rotation = Quaternion.Euler(-90, 0, 0);
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(-90, 0, 0);
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(-90, 0, 0);
    }
}
