using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public struct CastInfo
{
    public LivingThing owner;
    public Vector3 point;
    public Vector3 directionVector;
    public LivingThing target;

    public Quaternion directionQuaternion
    {
        get
        {
            return Quaternion.LookRotation(directionVector, Vector3.up);
        }
    }

    public static CastInfo OwnerOnly(LivingThing owner)
    {
        return new CastInfo { owner = owner };
    }

    public static CastInfo OwnerAndTarget(LivingThing owner, LivingThing target) {
        return new CastInfo { owner = owner, target = target };
    }
}

/// <summary>
/// Safe reference of an instance of AbilityInstance that invalidates itself when the AbilityInstance is despawned/reused.
/// </summary>
public class AbilityInstanceSafeReference : System.IEquatable<AbilityInstanceSafeReference>, IDewActionCaller
{
    private const int MaxStoredReferences = 1000;

    private static Dictionary<int, AbilityInstanceSafeReference> _references = new Dictionary<int, AbilityInstanceSafeReference>();
    private static Queue<int> _viewIDQueue = new Queue<int>();

    public System.Action<InfoManaSpent> OnSpendMana { get; set; } = (InfoManaSpent _) => { };
    public System.Action<InfoDamage> OnDealDamage { get; set; } = (InfoDamage _) => { };
    public System.Action<InfoDamage> OnDealPureDamage { get; set; } = (InfoDamage _) => { };
    public System.Action<InfoMagicDamage> OnDealMagicDamage { get; set; } = (InfoMagicDamage _) => { };
    public System.Action<InfoBasicAttackHit> OnDoBasicAttackHit { get; set; } = (InfoBasicAttackHit _) => { };
    public System.Action<InfoHeal> OnDoHeal { get; set; } = (InfoHeal _) => { };
    public System.Action<InfoManaHeal> OnDoManaHeal { get; set; } = (InfoManaHeal _) => { };
    public LivingThing entity { get => owner; }

    /// <summary>
    /// Is the instance active and hasn't been marked for despawn/despawned/reused?
    /// </summary>
    public bool isValid
    {
        get => _instance != null && _instance.isActiveAndEnabled && _instance.photonView.ViewID == viewID && _instance.isAlive;
    }

    private AbilityInstance _instance;
    public int viewID { get; private set; }
    public LivingThing owner { get; private set; }

    /// <summary>
    /// Create a safe reference of an instance of AbilityInstance that invalidates itself when the AbilityInstance is marked for despawn/despawned/reused.
    /// </summary>
    public AbilityInstanceSafeReference(AbilityInstance instance)
    {
        _instance = instance;
        viewID = instance.photonView.ViewID;
        owner = instance.info.owner;

        _references.Add(viewID, this);
        _viewIDQueue.Enqueue(viewID);
        if (_viewIDQueue.Count > MaxStoredReferences) _references.Remove(_viewIDQueue.Dequeue());
    }

    /// <summary>
    /// Retrieve an existing safe reference of AbilityInstance instance of the specified viewID. If there isn't, this will attempt to create a new one. If all fails, returns null.
    /// </summary>
    /// <param name="viewID"></param>
    /// <returns></returns>
    public static AbilityInstanceSafeReference RetrieveOrCreate(int viewID)
    {
        if (_references.ContainsKey(viewID))
        {
            return _references[viewID];
        }
        else if (PhotonNetwork.PhotonViewExists(viewID))
        {
            AbilityInstance instance = PhotonNetwork.GetPhotonView(viewID).GetComponent<AbilityInstance>();
            return new AbilityInstanceSafeReference(instance);
        }
        return null;
    }

    /// <summary>
    /// Gets the target instance. Returns null if this reference is not valid.
    /// </summary>
    /// <returns></returns>
    public AbilityInstance Dereference()
    {
        return isValid ? _instance : null;
    }

    public bool Equals(AbilityInstanceSafeReference other)
    {
        return viewID == other.viewID &&
            OnSpendMana == other.OnSpendMana &&
            OnDealDamage == other.OnDealDamage &&
            OnDealPureDamage == other.OnDealPureDamage &&
            OnDealMagicDamage == other.OnDealMagicDamage &&
            OnDoBasicAttackHit == other.OnDoBasicAttackHit &&
            OnDoHeal == other.OnDoHeal &&
            OnDoManaHeal == other.OnDoManaHeal;
    }

    public int Serialize()
    {
        return viewID;
    }
}

public class AbilityInstanceManager : MonoBehaviour
{

    public static AbilityInstanceSafeReference CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, CastInfo castInfo, object[] data = null)
    {
        return CreateAbilityInstanceFromGem(prefabName, position, rotation, castInfo, null, data);
    }

    public static AbilityInstanceSafeReference CreateAbilityInstanceFromGem(string prefabName, Vector3 position, Quaternion rotation, CastInfo castInfo, Gem gem, object[] data = null)
    {

        object[] initData;

        if (data == null)
        {
            initData = new object[5];
        }
        else
        {
            initData = new object[5 + data.Length];
        }

        if (castInfo.owner != null)
        {
            initData[0] = castInfo.owner.photonView.ViewID;
        }
        else
        {
            initData[0] = -1;
        }

        initData[1] = castInfo.point;
        initData[2] = castInfo.directionVector;

        if (castInfo.target != null)
        {
            initData[3] = castInfo.target.photonView.ViewID;
        }
        else
        {
            initData[3] = -1;
        }

        initData[4] = gem != null ? gem.photonView.ViewID : -1;

        if (data != null)
        {
            for (int i = 0; i < data.Length; i++)
            {
                initData[5 + i] = data[i];
            }
        }

        return PhotonNetwork.Instantiate("AbilityInstances/" + prefabName, position, rotation, 0, initData).GetComponent<AbilityInstance>().reference;
    }
}
