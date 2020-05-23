using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionFlattenExtension
{
    /// <summary>
    /// Flattens this quaternion, leaving only its y-axis rotation.
    /// </summary>
    /// <param name="q"></param>
    public static void Flatten(this ref Quaternion q)
    {
        q = Quaternion.Euler(0f, q.eulerAngles.y, 0f);
    }

    /// <summary>
    /// Returns a flattened version of this quaternion, leaving only its y-axis rotation.
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public static Quaternion Flattened(this Quaternion q)
    {
        return Quaternion.Euler(0f, q.eulerAngles.y, 0f);
    }
}
