using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using Photon.Pun;
using System.Linq;
public class DebugCommands : MonoBehaviour
{

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

    [ConsoleMethod("dot", "Poison a living thing at cursor position for a certain amount for a duration")]
    public static void DoT(float amount, float duration)
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;

        StatusEffect dot = new StatusEffect(target, StatusEffectType.DamageOverTime, duration, amount);
        target.statusEffect.ApplyStatusEffect(dot);
    }

    [ConsoleMethod("hot", "Heal a living thing at cursor position for a certain amount for a duration")]
    public static void HoT(float amount, float duration)
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;

        StatusEffect hot = new StatusEffect(target, StatusEffectType.HealOverTime, duration, amount);
        target.statusEffect.ApplyStatusEffect(hot);
    }




    [ConsoleMethod("spawn", "Spawn a Networked GameObject at cursor position.")]
    public static void Spawn(string name, int amount=1)
    {
        for(int i = 0; i < amount; i++)
        {
            PhotonNetwork.Instantiate(name, GetCurrentCursorPositionInWorldSpace(), Quaternion.identity);
        }

    }



}
