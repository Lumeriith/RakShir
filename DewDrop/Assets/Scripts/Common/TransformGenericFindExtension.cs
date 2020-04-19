using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformGenericFindExtension
{
    public static T Find<T>(this Transform t, string name)
    {
        return t.Find(name).GetComponent<T>();
    }
}