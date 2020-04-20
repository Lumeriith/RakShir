using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DewResources
{
    private static Dictionary<string, AnimationClip> _animationClipByName;

    public static AnimationClip GetAnimationClip(string name)
    {
        if (_animationClipByName == null) BuildAnimationClipByNameDictionary();
        _animationClipByName.TryGetValue(name, out AnimationClip result);
        if (result == null) Debug.LogErrorFormat("No AniamtionClip with name {0} was found!", name);
        return result;
    }

    private static void BuildAnimationClipByNameDictionary()
    {
        _animationClipByName = new Dictionary<string, AnimationClip>();
        AnimationClip[] clips = Resources.LoadAll<AnimationClip>("Animations");
        for(int i = 0; i < clips.Length; i++)
        {
            if (!_animationClipByName.ContainsKey(clips[i].name)) _animationClipByName.Add(clips[i].name, clips[i]);
        }
    }
}
