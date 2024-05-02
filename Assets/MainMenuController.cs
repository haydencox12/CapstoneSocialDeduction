using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button m_CreateLobbyButton;

    public void start()
    {
        //m_CreateLobbyButton.onClick.AddListener(createLobby);
    }
    
    public void createLobby()
    {
        Debug.Log("To the LOBBY");
        SceneManager.LoadScene("lobby");
    }
}
