using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "DewResourceReferences", menuName = "DewResourceReferences", order = 1)]
public class DewResourceReferences : SerializedScriptableObject
{
    private const string AbilityInstancesPath = "Assets/Dew/AbilityInstances";
    private const string AnimationsPath = "Assets/Res/Animations";
    private const string LivingThingsPath = "Assets/Dew/LivingThings";
    private const string ItemsPath = "Assets/Dew/Items";
    private const string RoomsPath = "Assets/Dew/Rooms";
    private const string SFXInstancesPath = "Assets/Dew/SFXInstances";

    public Dictionary<string, AnimationClip> animations;
    public Dictionary<string, GameObject> livingThings;
    public Dictionary<string, GameObject> items;
    public Dictionary<string, GameObject> abilityInstances;
    public Dictionary<string, GameObject> rooms;
    public Dictionary<string, GameObject> sfxInstances;


    [Button]
    public void Build()
    {
        animations = new Dictionary<string, AnimationClip>();
        livingThings = new Dictionary<string, GameObject>();
        items = new Dictionary<string, GameObject>();
        abilityInstances = new Dictionary<string, GameObject>();
        rooms = new Dictionary<string, GameObject>();
        sfxInstances = new Dictionary<string, GameObject>();

        int addedAnimations = AppendObjectsAtPathToDictionary(AnimationsPath, animations);
        int addedLivingThings = AppendObjectsAtPathToDictionary(LivingThingsPath, livingThings);
        int addedItems = AppendObjectsAtPathToDictionary(ItemsPath, items);
        int addedRooms = AppendObjectsAtPathToDictionary(RoomsPath, rooms);
        int addedSFXInstances = AppendObjectsAtPathToDictionary(SFXInstancesPath, sfxInstances);
        int addedAbilityInstances = AppendObjectsAtPathToDictionary(AbilityInstancesPath, abilityInstances);

        if (addedAnimations > 0) Debug.LogFormat("Added {0} AnimationClips.", addedAnimations);
        if (addedLivingThings > 0) Debug.LogFormat("Added {0} LivingThings.", addedLivingThings);
        if (addedItems > 0) Debug.LogFormat("Added {0} Items.", addedLivingThings);
        if (addedRooms > 0) Debug.LogFormat("Added {0} Rooms.", addedLivingThings);
        if (addedSFXInstances > 0) Debug.LogFormat("Added {0} SFXInstances.", addedLivingThings);
        if (addedAbilityInstances > 0) Debug.LogFormat("Added {0} AbilityInstances", addedAbilityInstances);
    }

    [Button]
    public void Clear()
    {
        animations = new Dictionary<string, AnimationClip>();
        livingThings = new Dictionary<string, GameObject>();
        items = new Dictionary<string, GameObject>();
        abilityInstances = new Dictionary<string, GameObject>();
        rooms = new Dictionary<string, GameObject>();
        sfxInstances = new Dictionary<string, GameObject>();
    }

    /// <summary>
    /// Appends Objects at the given path to the dictionary, and returns the number of Objects added.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    private int AppendObjectsAtPathToDictionary<T>(string path, Dictionary<string, T> dictionary) where T : Object
    {
        string[] paths = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
        int added = 0;

        if (paths != null && paths.Length > 0)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(paths[i]);
                if (obj is T asset)
                {
                    if (!dictionary.ContainsValue(asset))
                    {
                        dictionary.Add(asset.name, asset);
                        added++;
                    }
                }
            }
        }
        return added;
    }
}
