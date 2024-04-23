using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum role
{
    Ceasar, Supporter, Conspirator
}
public class PlayerObject : MonoBehaviour
{
    public int playerID;
    public string playerNickname;
    public role playerRole;

    public PlayerObject(int PlayerID, string PlayerNickname)
    {
        playerID = PlayerID;
        playerNickname = PlayerNickname;
    }

}
