using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using Photon.Pun;
using System.Linq;
public class DebugCommands : MonoBehaviour
{
    public static Object[] resources;
    private static Entity GetFirstValidTarget()
    {
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(cursorRay, 100, LayerMask.GetMask("LivingThing"));
        IEnumerable<RaycastHit> byDistance = hits.OrderBy(hit => hit.distance);
        foreach (RaycastHit hit in hits)
        {
            Entity lt = hit.collider.GetComponent<Entity>();
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
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.EarnGold(amount);
    }


    [ConsoleMethod("blue", "Apply superb cooldown reduction and mana regeneration bonus to the target")]
    public static void Blue()
    {
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.stat.bonusManaRegenerationPerSecond += 1000f;
        target.stat.bonusCooldownReduction += 10000f;
        target.stat.SyncTemporaryAttributes();
    }

    [ConsoleMethod("red", "Apply superb attack damage bonus to the target")]
    public static void Red()
    {
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.stat.bonusAttackDamage += 10000f;
        target.stat.SyncTemporaryAttributes();
    }



    [ConsoleMethod("reset", "Reset all cooldowns of local player and fully heals mana")]
    public static void Reset()
    {
        Entity target = GameManager.instance.localPlayer;
        for(int i = 0; i < target.control.skillSet.Length; i++)
        {
            if (target.control.skillSet[i] != null) target.control.skillSet[i].ResetCooldown();
        }
        target.DoManaHeal(target, target.stat.finalMaximumMana, true, null);
    }


    [ConsoleMethod("skip", "Skip this room of local player and move to next random room")]
    public static void Skip()
    {
        Entity target = GameManager.instance.localPlayer;
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
        Entity target = GameManager.instance.localPlayer;
        if (target.currentRoom != null && target.currentRoom.nextRooms.Count != 0)
        {
            Room nextRoom = target.currentRoom.nextRooms[Random.Range(0, target.currentRoom.nextRooms.Count)];
            GladiatorGameManager.DoObeliskTeleportation(nextRoom);
        }
    }


    [ConsoleMethod("heal", "Fully heal a living thing at cursor position")]
    public static void Heal()
    {
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.DoHeal(target, target.maximumHealth, true, null);
    }

    [ConsoleMethod("revive", "Revive a living thing at cursor position")]
    public static void Revive()
    {
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.Revive();
        target.DoHeal(target, target.maximumHealth, true, null);
    }

    [ConsoleMethod("kill", "Kill a living thing at cursor position")]
    public static void Kill()
    {
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.Kill();
    }

    [ConsoleMethod("spawn", "Spawn all Networked GameObjects with matching names at cursor position.")]
    public static void Spawn(string name)
    {
        PhotonNetwork.Instantiate(DewResources.GetEntityOrItemBySubstring(name).name, GetCurrentCursorPositionInWorldSpace() + Vector3.up * 1f + Random.onUnitSphere, Quaternion.identity);


    }

    [ConsoleMethod("hot", "Heal LivingThing at cursor location over time for given amount and duration.")]
    public static void Hot(float amount, float duration)
    {
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.statusEffect.ApplyStatusEffect(StatusEffect.HealOverTime(duration, amount), null);
    }

    [ConsoleMethod("dot", "Damage LivingThing at cursor location over time for given amount and duration.")]
    public static void Dot(float amount, float duration)
    {
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.statusEffect.ApplyStatusEffect(StatusEffect.DamageOverTime(duration, amount), null);
    }

    [ConsoleMethod("shield", "Sield LivingThing at cursor location for given amount and duration.")]
    public static void Shield(float amount, float duration)
    {
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        target.statusEffect.ApplyStatusEffect(StatusEffect.Shield(duration, amount), null);
    }

    [ConsoleMethod("music", "Tunes the jukebox.")]
    public static void ChangeMusic(string name)
    {
        Music.Play(name);
    }

    [ConsoleMethod("equipgem", "Equips first gem found in the inventory to the AbilityTrigger at the index specified")]
    public static void EquipGem(int index)
    {
        PlayerInventory inventory = GameManager.instance.localPlayer.GetComponent<PlayerInventory>();
        Gem gem;
        for (int i = 0; i < inventory.inventory.Length; i++)
        {
            gem = inventory.inventory[i] as Gem;
            if (gem != null)
            {
                inventory.EquipGemFromInventory(i, GameManager.instance.localPlayer.control.skillSet[index]);
                return;
            }
        }
    }

    [ConsoleMethod("unequipgem", "Unequips all gems attached to the AbilityTrigger at the index specified")]
    public static void UnequipGem(int index)
    {
        PlayerInventory inventory = GameManager.instance.localPlayer.GetComponent<PlayerInventory>();

        for (int i = 0; i < GameManager.instance.localPlayer.control.skillSet[index].connectedGems.Count; i++)
        {
            inventory.UnequipGem(GameManager.instance.localPlayer.control.skillSet[index].connectedGems[i], true);
        }
    }


    [ConsoleMethod("select", "Select the unit on the cursor. Expect an unexpected behaviour that isn't local.")]
    public static void Select()
    {
        Entity target = GetFirstValidTarget();
        if (target == null) target = GameManager.instance.localPlayer;
        UnitControlManager.instance.selectedUnit = target;
    }










}
