using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3QuantitizeExtension
{
    /// <summary>
    /// Returns a copy of this vector with its coordinates rounded to the nearest integer values.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 Quantitized(this Vector3 v)
    {
        return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
    }

    /// <summary>
    /// Round this vector's coordinates to the nearest integer values.
    /// </summary>
    /// <param name="v"></param>
    public static void Quantitize(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
    }
}
