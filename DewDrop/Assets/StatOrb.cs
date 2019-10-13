using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public enum SecondaryStatType { Strength, Agility, Intelligence } 

public class StatOrb : MonoBehaviour
{
    TextMeshProUGUI text;
    public SecondaryStatType type;

    private void Awake()
    {
        text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (GameManager.instance.localPlayer == null) return;
        if(type == SecondaryStatType.Strength)
        {
            text.text = ((int)GameManager.instance.localPlayer.stat.finalStrength).ToString();
        }
        else if (type== SecondaryStatType.Agility)
        {
            text.text = ((int)GameManager.instance.localPlayer.stat.finalAgility).ToString();
        } else if (type == SecondaryStatType.Intelligence)
        {
            text.text = ((int)GameManager.instance.localPlayer.stat.finalIntelligence).ToString();
        }
        
    }
}
