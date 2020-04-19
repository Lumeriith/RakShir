using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public List<AnimationClip> animations;

}
