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

    [ConsoleMethod("setobjective", "Set objective to current cursor position.")]
    public static void SetObjective()
    {
        GuidanceArrowManager.SetObjective(GetCurrentCursorPositionInWorldSpace());
    }

    [ConsoleMethod("removeobjective", "Remove objective if it exists.")]
    public static void RemoveObjective()
    {
        GuidanceArrowManager.RemoveObjective();
    }




    [ConsoleMethod("reroll", "Reroll Shop stocks.")]
    public static void Reroll()
    {
        ShopManager.instance.RerollShop();
    }

    [ConsoleMethod("earn", "Make a living thing at cursor position earn specified aount of money")]
    public static void Earn(float amount)
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.EarnGold(amount);
    }


    [ConsoleMethod("blue", "Apply superb cooldown reduction and mana regeneration bonus to the target")]
    public static void Blue()
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.stat.bonusManaRegenerationPerSecond += 1000f;
        target.stat.bonusCooldownReduction += 10000f;
        target.stat.SyncTemporaryAttributes();
    }

    [ConsoleMethod("red", "Apply superb attack damage bonus to the target")]
    public static void Red()
    {
        LivingThing target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.stat.bonusAttackDamage += 10000f;
        target.stat.SyncTemporaryAttributes();
    }



    [ConsoleMethod("reset", "Reset all cooldowns of local player and fully heals mana")]
    public static void Reset()
    {
        LivingThing target = GameManager.instance.localPlayer;
        for(int i = 0; i < target.control.skillSet.Length; i++)
        {
            if (target.control.skillSet[i] != null) target.control.skillSet[i].ResetCooldown();
        }
        target.DoManaHeal(target.stat.finalMaximumMana, target, true);
    }


    [ConsoleMethod("skip", "Skip this room of local player and move to next random room")]
    public static void Skip()
    {
        LivingThing target = GameManager.instance.localPlayer;
        if (target.currentRoom != null && target.currentRoom.nextRooms.Count != 0)
        {
            Room nextRoom = target.currentRoom.nextRooms[Random.Range(0, target.currentRoom.nextRooms.Count)];
            if (target.team == Team.Red && nextRoom.redCustomEntryPoint != null)
            {
                target.Teleport(nextRoom.redCustomEntryPoint.position);
            }
            else if (target.team == Team.Blue && nextRoom.blueCustomEntryPoint != null)
            {
                target.Teleport(nextRoom.blueCustomEntryPoint.position);
            }
            else
            {
                target.Teleport(nextRoom.entryPoint.position);
            }
            target.SetCurrentRoom(nextRoom);
        }
    }

    [ConsoleMethod("skipfancy", "Skip this room of local player and move to next random room by Obelisk teleportation")]
    public static void SkipFancy()
    {
        LivingThing target = GameManager.instance.localPlayer;
        if (target.currentRoom != null && target.currentRoom.nextRooms.Count != 0)
        {
            Room nextRoom = target.currentRoom.nextRooms[Random.Range(0, target.currentRoom.nextRooms.Count)];
            GameManager.DoObeliskTeleportation(nextRoom);
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

    [ConsoleMethod("spawn", "Spawn all Networked GameObjects with matching names at cursor position.")]
    public static void Spawn(string name)
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
            if(target.name.StartsWith("player_") || target.name.StartsWith("monster_"))
            {
                GameManager.SpawnLivingThing(target.name, GetCurrentCursorPositionInWorldSpace() + Vector3.up * 1f + Random.onUnitSphere);
            } else if (target.name.StartsWith("cons_") || target.name.StartsWith("equip_"))
            {
                GameManager.SpawnItem(target.name, GetCurrentCursorPositionInWorldSpace() + Vector3.up * 1f + Random.onUnitSphere);
            }
            
        }
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
