using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
public class CustomAnimationBox : MonoBehaviour
{
    private static CustomAnimationBox _instance;
    public static CustomAnimationBox instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CustomAnimationBox>();
            }
            return _instance;
        }
    }

    [ReorderableList]
    public List<AnimationClip> animations;

}
