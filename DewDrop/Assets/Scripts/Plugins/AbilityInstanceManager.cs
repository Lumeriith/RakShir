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


public struct SourceInfo
{
    public LivingThing thing;
    public AbilityTrigger trigger;
    public Gem gem;
    public AbilityInstance instance;

    public static SourceInfo CasterOnly(LivingThing thing) { return new SourceInfo { thing = thing }; }

    public static SourceInfo Empty() { return new SourceInfo(); }

    public static byte[] SerializeSourceInfo(object sourceObject)
    {
        int thing, gem, instance;
        string trigger;

        SourceInfo source = (SourceInfo)sourceObject;

        thing = source.thing != null ? source.thing.photonView.ViewID : -1;
        gem = source.gem != null ? source.gem.photonView.ViewID : -1;
        instance = source.instance != null ? source.instance.photonView.ViewID : -1;
        trigger = source.trigger != null ? source.trigger.name : "";

        return Concat(new byte[][] {
            System.BitConverter.GetBytes(thing),
            System.BitConverter.GetBytes(gem),
            System.BitConverter.GetBytes(instance),
            System.Text.Encoding.UTF8.GetBytes(trigger)
        });
    }

    private static byte[] Concat(byte[][] arrays)
    {
        return arrays.SelectMany(x => x).ToArray();
    }

    public static object DeserializeSourceInfo(byte[] data)
    {
        int thing, gem, instance;
        string trigger;

        thing = System.BitConverter.ToInt32(data, 0);
        gem = System.BitConverter.ToInt32(data, 4);
        instance = System.BitConverter.ToInt32(data, 8);
        trigger = System.Text.Encoding.UTF8.GetString(data, 12, data.Length - 12);

        SourceInfo source = new SourceInfo();
        PhotonView thingPhotonView = thing == -1 ? null : PhotonNetwork.GetPhotonView(thing);
        PhotonView gemPhotonView = gem == -1 ? null : PhotonNetwork.GetPhotonView(gem);
        PhotonView instancePhotonView = instance == -1 ? null : PhotonNetwork.GetPhotonView(instance);

        source.thing = thingPhotonView == null ? null : thingPhotonView.GetComponent<LivingThing>();
        source.gem = gemPhotonView == null ? null : gemPhotonView.GetComponent<Gem>();
        source.instance = instancePhotonView == null ? null : instancePhotonView.GetComponent<AbilityInstance>();
        source.trigger = null;
        if (source.thing != null && trigger != "")
        {
            foreach(Transform t in source.thing.transform)
            {
                if(t.name == trigger)
                {
                    source.trigger = t.GetComponent<AbilityTrigger>();
                    break;
                }
            }
        }

        return source;
    }

    public bool IsSameSourceExceptInstance(SourceInfo other)
    {
        return other.gem == gem && other.thing == thing && other.trigger == trigger;
    }

}



public class AbilityInstanceManager : MonoBehaviour
{

    public static AbilityInstance CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, CastInfo castInfo, SourceInfo source, object[] data = null)
    {

        object[] initData;

        if (data == null)
        {
            initData = new object[6];
        }
        else
        {
            initData = new object[6 + data.Length];
        }
        
        if(castInfo.owner != null)
        {
            initData[0] = castInfo.owner.photonView.ViewID;
        }
        else
        {
            initData[0] = -1;
        }

        initData[1] = castInfo.point;
        initData[2] = castInfo.directionVector;
        
        if(castInfo.target != null)
        {
            initData[3] = castInfo.target.photonView.ViewID;
        }
        else
        {
            initData[3] = -1;
        }

        if(source.trigger != null)
        {
            initData[4] = source.trigger.name;
        }
        else
        {
            initData[4] = "";
        }

        if(source.gem != null)
        {
            initData[5] = source.gem.photonView.ViewID;
        }
        else
        {
            initData[5] = -1;
        }

        if (data != null)
        {
            for (int i = 0; i < data.Length; i++)
            {
                initData[6 + i] = data[i];
            }
        }

        return PhotonNetwork.Instantiate("AbilityInstances/" + prefabName, position, rotation, 0, initData).GetComponent<AbilityInstance>();
        
    }

   
}
