using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class MainMenuLogic : MonoBehaviour
{
    public InputField input_CharacterName;
    private void Start()
    {
        Music.Play("MainMenu");
        input_CharacterName.text = PlayerPrefs.GetString("characterName", "이름없는 영웅");
    }

    public void PlayGladiator()
    {
        LobbyManager.gameType = GameType.Gladiator;
        LobbyManager.properlyConfiguredGame = true;
        SceneManager.LoadScene("Lobby");
    }

    public void PlayPlayground()
    {
        PhotonNetwork.Disconnect();
        LobbyManager.gameType = GameType.Playground;
        LobbyManager.properlyConfiguredGame = true;
        SceneManager.LoadScene("Lobby");
    }

    public void UpdateCharacterName()
    {
        PlayerPrefs.SetString("characterName", input_CharacterName.text);
    }

}
