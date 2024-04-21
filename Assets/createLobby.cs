using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class createLobby : MonoBehaviour
{
    public GameObject MainMenu, LobbyMenu;
    public Button m_CreateLobbyButton, m_BacktoMainMenuButton;
    public int lobbyID;
    
    // Start is called before the first frame update
    void Start()
    {
        m_CreateLobbyButton.onClick.AddListener(goToLobby);
        m_BacktoMainMenuButton.onClick.AddListener(goToMain);
    }

    // Update is called once per frame
    void goToLobby()
    {
        Debug.Log("To the LOBBY");
        MainMenu.SetActive(false);
        LobbyMenu.SetActive(true);
    }

    void goToMain()
    {
        Debug.Log("To the MAIN MENU");
        LobbyMenu.SetActive(false);
        MainMenu.SetActive(true);
        
    }
}
