using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DewResources
{
    private const string MainReferencePath = "Assets/Resources/MainReferences.asset";
    private const string MainReferenceResourceName = "MainReferences";

    private static DewResourceReferences _references;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void BuildOnResources()
    {
        UnityEditor.AssetDatabase.LoadAssetAtPath<DewResourceReferences>(MainReferencePath).Build();
    }

    public static void Initialize()
    {
        _references = Resources.Load<DewResourceReferences>(MainReferenceResourceName);
    }

    public static AnimationClip GetAnimationClip(string name)
    {
        if (!_references.animations.TryGetValue(name, out AnimationClip result)) Debug.LogErrorFormat("No AnimationClip with name {0} was found!", name);
        return result;
    }

    public static GameObject GetLivingThing(string name)
    {
        if (!_references.livingThings.TryGetValue(name, out GameObject result)) Debug.LogErrorFormat("No LivingThing with name {0} was found!", name);
        return result;
    }

    public static GameObject GetAbilityInstance(string name)
    {
        if (!_references.abilityInstances.TryGetValue(name, out GameObject result)) Debug.LogErrorFormat("No AbilityInstance with name {0} was found!", name);
        return result;
    }

    public static GameObject GetItem(string name)
    {
        if (!_references.items.TryGetValue(name, out GameObject result)) Debug.LogErrorFormat("No Item with name {0} was found!", name);
        return result;
    }

    public static GameObject GetRoom(string name)
    {
        if (!_references.rooms.TryGetValue(name, out GameObject result)) Debug.LogErrorFormat("No Room with name {0} was found!", name);
        return result;
    }

    public static GameObject GetSFXInstance(string name)
    {
        if (!_references.sfxInstances.TryGetValue(name, out GameObject result)) Debug.LogErrorFormat("No SFXInstance with name {0} was found!", name);
        return result;
    }
}
