using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class LobbyManager : MonoBehaviour
{
    public int lobbyID;
    public List<PlayerObject> PlayerList = new List<PlayerObject>();
    public TMP_Text _Player0, _Player1, _Player2, _Player3, _Player4, _Player5, _Player6, _Player7, _Player8, _Player9, _Player10, _Player11, _GameIDText;
    int search;
    public static LobbyManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayerList.Add(new PlayerObject(0));
        PlayerList.Add(new PlayerObject(1));
        PlayerList.Add(new PlayerObject(2));
        PlayerList.Add(new PlayerObject(3));
        PlayerList.Add(new PlayerObject(4));
        PlayerList.Add(new PlayerObject(5));
        PlayerList.Add(new PlayerObject(6));
        PlayerList.Add(new PlayerObject(7));
        PlayerList.Add(new PlayerObject(8));
        PlayerList.Add(new PlayerObject(9));
        PlayerList.Add(new PlayerObject(10));
        PlayerList.Add(new PlayerObject(11));

        getByID(0).PlayerNickname = "Paul";
        
        _Player0.text = getByID(0).PlayerNickname;
        _Player1.text = getByID(1).PlayerNickname;
        _Player2.text = getByID(2).PlayerNickname;
        _Player3.text = getByID(3).PlayerNickname;
        _Player4.text = getByID(4).PlayerNickname;
        _Player5.text = getByID(5).PlayerNickname;
        _Player6.text = getByID(6).PlayerNickname;
        _Player7.text = getByID(7).PlayerNickname;
        _Player8.text = getByID(8).PlayerNickname;
        _Player9.text = getByID(9).PlayerNickname;
        _Player10.text = getByID(10).PlayerNickname;
        _Player11.text = getByID(11).PlayerNickname;
        string LobbyID = lobbyID.ToString();
        _GameIDText.text = LobbyID;

    }

    /*
    void Update()
    {
        _Player0.text = getByID(0).PlayerNickname;
        _Player1.text = getByID(1).PlayerNickname;
        _Player2.text = getByID(2).PlayerNickname;
        _Player3.text = getByID(3).PlayerNickname;
        _Player4.text = getByID(4).PlayerNickname;
        _Player5.text = getByID(5).PlayerNickname;
        _Player6.text = getByID(6).PlayerNickname;
        _Player7.text = getByID(7).PlayerNickname;
        _Player8.text = getByID(8).PlayerNickname;
        _Player9.text = getByID(9).PlayerNickname;
        _Player10.text = getByID(10).PlayerNickname;
        _Player11.text = getByID(11).PlayerNickname;
    }
    */

    PlayerObject getByID(int id)
    {
        return this.PlayerList.FirstOrDefault(z => z.PlayerID == id);
    }

    public void startTheGame()
    {
        int[12] RoleIndexer = {};

        int RoleIndexer2 = 0;

        foreach(PlayerObject player in PlayerList.Where(s.PlayerNickname.ToString() != "" ))
        {
            RoleIndexer = RoleIndexer.Append(RoleIndexer2).ToArray();
            RoleIndexer2++;
        }
        
        int ArrayIndexer;
        ArrayIndexer = random.next(0, RoleIndexer.Length);
        int ceasar;
        int firstsup;
        int supporter;
        int conspirator;
        
        int RandomNum;

        Random random = new Random();
        RandomNum = random.Next(0,RoleIndexer.Length);
        ceasar = RandomNum;
        RandomNum = random.Next(0,RoleIndexer.Length);
        firstsup = RandomNum;
        

    }
}
