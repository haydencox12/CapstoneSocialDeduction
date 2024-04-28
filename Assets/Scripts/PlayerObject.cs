using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum role
{
    Dictator, Supporter, Conspirator
}

public class PlayerObject
{
    public int playerID;
    public string playerNickname = "";
    public role playerRole;

    public PlayerObject(int playerID)
    {
        this.playerID = playerID;
        //playerNickname = PlayerNickname;
    }

    public int PlayerID
    {
        get {return playerID;}
        set {playerID = value;}
    }

    public string PlayerNickname
    {
        get {return playerNickname;}
        set {playerNickname = value;}
    }

    public role PlayerRole
    {
        get {return playerRole;}
        set {playerRole = value;}
    }

}
