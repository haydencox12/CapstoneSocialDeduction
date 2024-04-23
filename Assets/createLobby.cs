using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class createLobby : MonoBehaviour
{
    public GameObject MainMenu, LobbyMenu, LobbyDisbandConfirmation, go_MainMenuButton;
    public Button m_CreateLobbyButton, m_BacktoMainMenuButton, m_DisbandConfirm, m_DisbandBack;
    
    // Start is called before the first frame update
    void Start()
    {
        m_CreateLobbyButton.onClick.AddListener(goToLobby);
        m_BacktoMainMenuButton.onClick.AddListener(goToLobbyDisband);
        m_DisbandConfirm.onClick.AddListener(goToMain);
        m_DisbandBack.onClick.AddListener(disbandCancel);
    }

    // Update is called once per frame
    void goToLobby()
    {
        Debug.Log("To the LOBBY");
        MainMenu.SetActive(false);
        LobbyDisbandConfirmation.SetActive(false);
        LobbyMenu.SetActive(true);
    }

    void disbandCancel()
    {
        Debug.Log("To the LOBBY");
        LobbyDisbandConfirmation.SetActive(false);
        go_MainMenuButton.SetActive(true);
    }

    void goToLobbyDisband()
    {
        Debug.Log("To the LOBBY DISBAND");
        LobbyDisbandConfirmation.SetActive(true); 
        go_MainMenuButton.SetActive(false);    
    }

    void goToMain()
    {
        Debug.Log("To the MAIN MENU");
        MainMenu.SetActive(true);
        LobbyDisbandConfirmation.SetActive(false);
        LobbyMenu.SetActive(false);
        go_MainMenuButton.SetActive(true);
    }
}
