using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyThisOnLoad : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
