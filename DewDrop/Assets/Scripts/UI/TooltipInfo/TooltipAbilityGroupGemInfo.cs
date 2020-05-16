using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class TooltipAbilityGroupGemInfo : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Required References")]
    private Text _gemName;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _gemDescription;

    public void Setup(Gem gem)
    {
        _gemName.text = gem.itemName;
        _gemDescription.text = DescriptionSyntax.Decode(gem.itemDescription, gem);
    }
}
