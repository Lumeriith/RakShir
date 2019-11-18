using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    private string playerName = "흰뿔도마뱀";
    private Text titleName;

    private void Awake()
    {
        titleName = transform.Find("Character Panel/Title Panel/Title - Name").GetComponent<Text>();
    }

    private void Update()
    {
        if (GameManager.instance.localPlayer == null) return;
        if(playerName != GameManager.instance.localPlayer.readableName)
        {
            playerName = GameManager.instance.localPlayer.readableName;
            titleName.text = playerName;
        }
    }

}
