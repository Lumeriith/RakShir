using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHealthbarPrototype : MonoBehaviour
{
    public Entity targetPlayer;
    
    Text text_Name;
    Text text_Detail;
    Image image_HealthFill;
    Image image_ManaFill;
    Image image_ChannelFill;

    public Vector3 worldOffset;
    public Vector3 UIOffset;

    public List<StatusEffectType> displayCCOrder;
    public List<string> displayCCText;
    private void Awake()
    {
        text_Name = transform.Find("Name").GetComponent<Text>();
        text_Detail = transform.Find("Detail").GetComponent<Text>();

        image_HealthFill = transform.Find("Health/Fill").GetComponent<Image>();
        image_ManaFill = transform.Find("Mana/Fill").GetComponent<Image>();
        image_ChannelFill = transform.Find("Channel/Fill").GetComponent<Image>();
    }
    void Update()
    {
        if (targetPlayer == null) return;

        
        
        text_Name.text = targetPlayer.photonView.name;

        text_Name.color = Color.white;
        for(int i = 0;i<displayCCOrder.Count;i++)
        {
            if (targetPlayer.statusEffect.IsAffectedBy(displayCCOrder[i]))
            {
                text_Name.text = displayCCText[i];
                text_Name.color = new Color(255, 190, 0);
                break;
            }
        }


        image_HealthFill.fillAmount = targetPlayer.currentHealth / targetPlayer.maximumHealth;
        image_ManaFill.fillAmount = targetPlayer.stat.currentMana / targetPlayer.stat.finalMaximumMana;
        
        image_ChannelFill.enabled = false;

        string extraText = "";

        foreach(StatusEffectType type in System.Enum.GetValues(typeof(StatusEffectType)))
        {
            if (targetPlayer.statusEffect.IsAffectedBy(type))
            {
                extraText += string.Format("{0}\n", type.ToString());

            }
        }

        text_Detail.text = string.Format("{4}Health {0}/{1}\nMana {2}/{3}", (int)targetPlayer.currentHealth, (int)targetPlayer.maximumHealth, (int)targetPlayer.stat.currentMana, (int)targetPlayer.stat.finalMaximumMana, extraText);

        transform.position = Camera.main.WorldToScreenPoint(targetPlayer.transform.position + worldOffset) + UIOffset;
    }
}
