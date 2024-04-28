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
    int playerCount;
    
    //the role array is initalized this way so that when it removes roles, the dictator will always be present and there will always be more supporters than conspirators
    role[] playerRoleQueue = {role.Dictator, role.Supporter, role.Conspirator, role.Supporter, role.Conspirator, role.Supporter, role.Conspirator, role.Supporter, role.Conspirator, role.Supporter, role.Conspirator, role.Supporter};

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
        
        //tempdebug
        //startTheGame();

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

    PlayerObject getByNick(string nick)
    {
        return this.PlayerList.FirstOrDefault(z => z.PlayerNickname == nick);
    }

    public void startTheGame() //assigns random roles
    {
        playerCount =0;
        var PlayerQuery =
            from player in PlayerList
            where player.PlayerNickname != ""
            select player;

        foreach (PlayerObject player in PlayerQuery) //counts the # of players
        {
            playerCount++;
            Debug.Log("Counted" + playerCount + " players!");
        }
        Debug.Log("number of players: " + playerCount.ToString());
        
        //List<role> ActiveRolesList = new List<role>();
        role[] reducedPlayerList = new role[playerCount];

        int tempPlayerCount = playerCount;
        int tempCounter = 0;
        while (tempPlayerCount > 0) //reduces the number of roles to the # of players while ensuring enough of each role is present
        {
            //ActiveRolesList.Add(playerRoleQueue[tempCounter]);
            reducedRoleArray[tempCounter] = playerRoleQueue[tempCounter]; //sets new role array index to the role array value
            tempCounter++;
            tempPlayerCount--;
            Debug.Log("Added role to ActiveRoleList!");
        }
        Debug.Log("Added all active roles!");
        tempCounter = 0;
        var rng = new Random();
        rng.Shuffle(reducedRoleArray);
        foreach (PlayerObject player in PlayerQuery)
        {
            //player.playerRole = ActiveRolesList[tempCounter];
            player.playerRole = reducedRoleArray[tempCounter];
            tempCounter++;
            string tempRole = "";
            switch (player.playerRole)
            {
                case role.Dictator:
                    tempRole = ("Dictator");
                    break;
                case role.Supporter:
                    tempRole = ("Supporter");
                    break;
                case role.Conspirator:
                    tempRole = ("Conspirator");
                    break;
            }
            Debug.Log("Assigned " + player.playerNickname + " the " + tempRole + " role!");
            
        }
        Debug.Log("Completed startTheGame Routine!");
        
    }

    public static void Shuffle<T> (this Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}
