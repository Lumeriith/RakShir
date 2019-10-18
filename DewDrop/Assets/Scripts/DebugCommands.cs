using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using Photon.Pun;
using System.Linq;
public class DebugCommands : MonoBehaviour
{
    public static Object[] resources;
    private static LivingThing GetFirstValidTarget()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(cursorRay, 100, LayerMask.GetMask("LivingThing"));
        IEnumerable<RaycastHit> byDistance = hits.OrderBy(hit => hit.distance);
        foreach (RaycastHit hit in hits)
        {
            LivingThing lt = hit.collider.GetComponent<LivingThing>();
            if (lt == null) continue;
            return lt;
        }
        return null;
    }

    private static Vector3 GetCurrentCursorPositionInWorldSpace()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(cursorRay, out hit, 100, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }
        else
        {
            return cursorRay.origin - cursorRay.direction * (cursorRay.origin.y / cursorRay.direction.y);
        }

    }

    [ConsoleMethod("heal", "Fully heal a living thing at cursor position")]
    public static void Heal()
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.DoHeal(target.maximumHealth, target, true);
    }
    [ConsoleMethod("revive", "Revive a living thing at cursor position")]
    public static void Revive()
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.Revive();
        target.DoHeal(target.maximumHealth, target, true);
    }

    [ConsoleMethod("kill", "Kill a living thing at cursor position")]
    public static void Kill()
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.Kill();
    }

    [ConsoleMethod("spawnall", "Spawn all Networked GameObjects with matching names at cursor position.")]
    public static void SpawnAll(string name)
    {
        List<GameObject> targets = new List<GameObject>();
        if (resources == null) resources = Resources.LoadAll("");
        foreach (object obj in resources)
        {
            GameObject gobj = obj as GameObject;
            if (gobj != null && gobj.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if(gobj.name.StartsWith("cons_") || gobj.name.StartsWith("equip_") || gobj.name.StartsWith("player_") || gobj.name.StartsWith("monster_"))
                {
                    targets.Add(gobj);
                }

            }
        }

        foreach(GameObject target in targets)
        {
            PhotonNetwork.Instantiate(target.name, GetCurrentCursorPositionInWorldSpace() + Vector3.up * 1f + Random.onUnitSphere, Quaternion.identity);
        }


    }



    [ConsoleMethod("spawnmultiple", "Spawn a specified number of Networked GameObject at cursor position.")]
    public static void SpawnMultiple(string name, int amount = 1)
    {
        GameObject target = null;
        if (resources == null) resources = Resources.LoadAll("");
        foreach (object obj in resources)
        {
            GameObject gobj = obj as GameObject;
            if (gobj != null && gobj.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (gobj.name.StartsWith("cons_") || gobj.name.StartsWith("equip_") || gobj.name.StartsWith("player_") || gobj.name.StartsWith("monster_"))
                {
                    target = gobj;
                }
                break;
            }
        }

        if (target == null)
        {
            print("Cannot find a prefab with a name containing `" + name + "`.");
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                PhotonNetwork.Instantiate(target.name, GetCurrentCursorPositionInWorldSpace() + Vector3.up * 1f + Random.onUnitSphere, Quaternion.identity);
            }
        }
    }
    [ConsoleMethod("spawn", "Spawn a Networked GameObject at cursor position.")]
    public static void Spawn(string name)
    {
        SpawnMultiple(name, 1);
    }



    [ConsoleMethod("hot", "Heal LivingThing at cursor location over time for given amount and duration.")]
    public static void Hot(float amount, float duration)
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.statusEffect.ApplyStatusEffect(StatusEffect.HealOverTime(target, duration, amount));
    }

    [ConsoleMethod("dot", "Damage LivingThing at cursor location over time for given amount and duration.")]
    public static void Dot(float amount, float duration)
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.statusEffect.ApplyStatusEffect(StatusEffect.DamageOverTime(target, duration, amount));
    }

    [ConsoleMethod("shield", "Sield LivingThing at cursor location for given amount and duration.")]
    public static void Shield(float amount, float duration)
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.statusEffect.ApplyStatusEffect(StatusEffect.Shield(target, duration, amount));
    }





}
