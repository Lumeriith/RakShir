using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreGem : MonoBehaviour
{
    private Image image;

    public int index;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if (GladiatorGameManager.instance.phase != GladiatorGamePhase.PvP) return;
        if (GladiatorGameManager.instance.roundIndex <= index) return;

        if((GladiatorGameManager.instance.didRedTeamWin[index] && GameManager.instance.localPlayer.team == Team.Red)
            ||(!GladiatorGameManager.instance.didRedTeamWin[index] && GameManager.instance.localPlayer.team == Team.Blue))
        {
            image.color = Color.green;
        }
        else
        {
            image.color = Color.red;
        }
    }
}
