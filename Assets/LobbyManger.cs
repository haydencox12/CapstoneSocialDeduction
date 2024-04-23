using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManger : MonoBehaviour
{
    public int lobbyID;
    List<PlayerObject> PlayerList = new List<PlayerObject>();
    public TMP_Text _Player0, _Player1, _Player2, _Player3, _Player4, _Player5, _Player6, _Player7, _Player8, _Player9, _Player10, _Player11, _GameIDText;
    
    void Start()
    {
        _Player0.text = "";
        _Player1.text = "";
        _Player2.text = "";
        _Player3.text = "";
        _Player4.text = "";
        _Player5.text = "";
        _Player6.text = "";
        _Player7.text = "";
        _Player8.text = "";
        _Player9.text = "";
        _Player10.text = "";
        _Player11.text = "";
        _GameIDText.text = "";

    }
}
