using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StatsPanel : MonoBehaviour
{
    private Text details;

    private void Awake()
    {
        details = transform.Find("Details").GetComponent<Text>();
    }

    void Update()
    {
        if (GameManager.instance.localPlayer == null) return;
        LivingThingStat stat = GameManager.instance.localPlayer.stat;

        string health = string.Format("{0} / {1}", (int)stat.currentHealth, (int)stat.finalMaximumHealth);
        if (stat.bonusMaximumHealth > 0) health += string.Format(" <color=yellow>(+{0})</color>", (int)stat.bonusMaximumHealth);
        else if (stat.bonusMaximumHealth < 0) health += string.Format(" <color=red>(-{0})</color>", (int)-stat.bonusMaximumHealth);

        string healthRegeneration = string.Format("초당 {0:0.0#}", stat.finalHealthRegenerationPerSecond);
        if (stat.bonusHealthRegenerationPerSecond > 0) healthRegeneration += string.Format(" <color=yellow>(+{0:0.0#})</color>", stat.bonusHealthRegenerationPerSecond);
        if (stat.bonusHealthRegenerationPerSecond < 0) healthRegeneration += string.Format(" <color=yellow>(-{0:0.0#})</color>", -stat.bonusHealthRegenerationPerSecond);

        string mana = string.Format("{0} / {1}", (int)stat.currentMana, (int)stat.finalMaximumMana);
        if (stat.bonusMaximumMana > 0) mana += string.Format(" <color=yellow>(+{0})</color>", (int)stat.bonusMaximumMana);
        else if (stat.bonusMaximumMana < 0) mana += string.Format(" <color=red>(-{0})</color>", (int)-stat.bonusMaximumMana);

        string manaRegeneration = string.Format("초당 {0:0.0#}", stat.finalManaRegenerationPerSecond);
        if (stat.bonusManaRegenerationPerSecond > 0) manaRegeneration += string.Format(" <color=yellow>(+{0:0.0#})</color>", stat.bonusManaRegenerationPerSecond);
        if (stat.bonusManaRegenerationPerSecond < 0) manaRegeneration += string.Format(" <color=red>(-{0:0.0#})</color>", -stat.bonusManaRegenerationPerSecond);

        string movementSpeed = string.Format("{0}", (int)stat.finalMovementSpeed);
        if (stat.bonusMovementSpeed > 0) movementSpeed += string.Format(" <color=yellow>(+{0})</color>", (int)stat.bonusMovementSpeed);
        if (stat.bonusMovementSpeed < 0) movementSpeed += string.Format(" <color=red>(-{0})</color>", -(int)stat.bonusMovementSpeed);

        string attackDamage = string.Format("{0}", (int)stat.finalAttackDamage);
        if (stat.bonusAttackDamage > 0) attackDamage += string.Format(" <color=yellow>(+{0})</color>", stat.bonusAttackDamage);
        if (stat.bonusAttackDamage < 0) attackDamage += string.Format(" <color=red>(-{0})</color>", -stat.bonusAttackDamage);

        string attackSpeed = string.Format("{0:0.0#}", stat.finalAttacksPerSecond);
        if (stat.bonusAttackSpeedPercentage > 0) attackSpeed += string.Format(" <color=yellow>(+{0}%)</color>", (int)stat.bonusAttackSpeedPercentage);
        if (stat.bonusAttackSpeedPercentage < 0) attackSpeed += string.Format(" <color=red>(-{0}%)</color>", -(int)stat.bonusAttackSpeedPercentage);

        string spellPower = string.Format("{0}", (int)stat.finalSpellPower);
        if (stat.bonusSpellPower > 0) spellPower += string.Format(" <color=yellow>(+{0})</color>", (int)stat.bonusSpellPower);
        if (stat.bonusSpellPower < 0) spellPower += string.Format(" <color=red>(-{0})</color>", -(int)stat.bonusSpellPower);

        string cooldownReduction = string.Format("{0}", (int)stat.finalCooldownReduction);
        if (stat.bonusCooldownReduction > 0) cooldownReduction += string.Format(" <color=yellow>(+{0})</color>", (int)stat.bonusCooldownReduction);
        if (stat.bonusCooldownReduction < 0) cooldownReduction += string.Format(" <color=red>(-{0})</color>", -(int)stat.bonusCooldownReduction);

        string dodge = string.Format("{0}", (int)stat.finalDodgeChance);
        if (stat.bonusDodgeChance > 0) cooldownReduction += string.Format(" <color=yellow>(+{0})</color>", (int)stat.bonusDodgeChance);
        if (stat.bonusDodgeChance < 0) cooldownReduction += string.Format(" <color=red>(-{0})</color>", -(int)stat.bonusDodgeChance);

        details.text = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}", health, healthRegeneration, mana, manaRegeneration, movementSpeed, attackDamage, attackSpeed, spellPower, cooldownReduction, dodge);
    }
}
