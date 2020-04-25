using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DewResources
{
    private const string MainReferencePath = "Assets/Resources/MainReferences.asset";
    private const string MainReferenceResourceName = "MainReferences";

    private static DewResourceReferences _references;
    private static List<string> _entityAndItemNames = new List<string>();

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

    public static GameObject GetEntity(string name)
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

    public static GameObject GetEntityOrItemBySubstring(string substring)
    {
        if(_entityAndItemNames.Count == 0)
        {
            foreach (string name in _references.livingThings.Keys) _entityAndItemNames.Add(name);
            foreach (string name in _references.items.Keys) _entityAndItemNames.Add(name);
        }

        string foundName = "";
        for(int i = 0;i< _entityAndItemNames.Count; i++)
        {
            if (_entityAndItemNames[i] == substring) return GetGameObject(substring);
            else if (foundName == "" && _entityAndItemNames[i].Contains(substring)) foundName = _entityAndItemNames[i];
        }
        if (foundName == "") return null;
        return GetGameObject(foundName);
    }

    public static GameObject GetGameObject(string name)
    {
        if (_references.sfxInstances.ContainsKey(name)) return _references.sfxInstances[name];
        if (_references.items.ContainsKey(name)) return _references.items[name];
        if (_references.livingThings.ContainsKey(name)) return _references.livingThings[name];
        if (_references.rooms.ContainsKey(name)) return _references.rooms[name];
        return null;
    }

}
