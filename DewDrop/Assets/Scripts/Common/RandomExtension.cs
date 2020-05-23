using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomExtension
{
    /// <summary>
    /// Returns a rotation that deviates with a random angle using provided parameters.
    /// </summary>
    /// <param name="minAngle"></param>
    /// <param name="maxAngle"></param>
    /// <returns></returns>
    public static Quaternion Spread(float minAngle, float maxAngle)
    {
        return Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Quaternion.AngleAxis(Random.Range(minAngle, maxAngle), Vector3.up);
    }
}
