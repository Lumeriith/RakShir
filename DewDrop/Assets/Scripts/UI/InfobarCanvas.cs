using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfobarCanvas : MonoBehaviour
{
    public GameObject monsterInfobar;
    public GameObject playerInfobar;
    public GameObject summonInfobar;

    private void Awake()
    {
        GameManager.instance.OnLivingThingInstantiate += (Entity thing) =>
        {
            if(thing.type == LivingThingType.Player)
            {
                StartCoroutine(CoroutineInstantiateInfobar(playerInfobar, thing));
            }
            else if (thing.type == LivingThingType.Monster)
            {
                StartCoroutine(CoroutineInstantiateInfobar(monsterInfobar, thing));
            }
            else if (thing.type == LivingThingType.Summon)
            {
                StartCoroutine(CoroutineInstantiateInfobar(summonInfobar, thing));
            }
        };
    }

    IEnumerator CoroutineInstantiateInfobar(GameObject infobar, Entity target)
    {
        yield return new WaitForSeconds(.5f);
        if (infobar != null)
        {
            Instantiate(infobar, Vector3.zero, Quaternion.identity, transform).GetComponent<IInfobar>().SetTarget(target);
        }
    }
}
