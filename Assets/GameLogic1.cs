using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Linq;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
//using UnityEditor.VersionControl;
//using static UnityEngine.GraphicsBuffer;
//using System.Security.Cryptography;


public class GameLogic : MonoBehaviour
{
    public TextMeshProUGUI outputText;
    public int controllingID = -1;


    [SerializeField]
    TextMeshProUGUI timerText;
    [SerializeField]
    int roundTime;
    [SerializeField]
    int totalRounds;
    [SerializeField]
    GameObject answer;
    [SerializeField]
    Transform canvasTransform;
    [SerializeField]
    GameObject avatar;


    [SerializeField]
    GameObject joinGame;
    [SerializeField]
    GameObject startGameButton;
    [SerializeField]
    GameObject timer;
    [SerializeField]
    GameObject output;
    [SerializeField]
    float offset;


    float timerNum;
    int currentRound;
    List<GameObject> answers;
    List<GameObject> avatars;
    
    List<int> highestAnswers;


    
    bool enoughPlayers;
    int numPlayers;
    bool startTimer;
    int gameMode;
    


    Dictionary<int, string> playerRoles = new Dictionary<int, string>();
    Dictionary<int, int> votes = new Dictionary<int, int>();
    private Dictionary<int, string> playerNames = new Dictionary<int, string>();
    private Dictionary<int, List<string>> playerPreferences = new Dictionary<int, List<string>>();
    private Dictionary<int, GameObject> playerAvatars = new Dictionary<int, GameObject>();
    private Dictionary<int, bool> playerDead = new Dictionary<int, bool>();


    void Awake()
    {
        AirConsole.instance.onMessage += OnMessage;
        AirConsole.instance.onConnect += OnConnect;
    }


    
    void OnConnect(int device_id)
    {
        numPlayers++;
        // Create a new avatar then reposition all of them
        GameObject avatarClone = Instantiate(avatar, Vector3.zero, Quaternion.identity, canvasTransform);
        avatarClone.GetComponentInChildren<TextMeshProUGUI>().text = "Player " + numPlayers;
        avatars.Add(avatarClone);
        playerAvatars[device_id] = avatarClone;
        
        RepositionList(avatars, 50);
        JObject message = new JObject();
        message["action"] = "requestInfo";
        message["questions"] = GetRandomQuestions();
        AirConsole.instance.Message(device_id, message);
        // Make sure there are at least two players to start a game with
        if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0)
        {
            if (AirConsole.instance.GetControllerDeviceIds().Count >= 3)
            {
                enoughPlayers = true;
            }
        }
        if (!playerDead.ContainsKey(device_id))
        {
            playerDead.Add(device_id, false);
        }
    }


    void OnMessage(int fromDeviceID, JToken data)
    {
        if(data ["action"] != null)
        {
            if(data["action"].ToString().Equals("fromDevice"))
            {
                outputText.text = "message from " + fromDeviceID + ", data: " + data;
            }
            
        }
        if(data ["input"] != null)
        {
            if (currentRound != -1)
            {
                GameObject answerClone = Instantiate(answer, Vector3.zero, Quaternion.identity, canvasTransform);
                answerClone.GetComponentInChildren<TextMeshProUGUI>().text = data["input"].ToString();
                answers.Add(answerClone);
                RepositionList(answers, Screen.height / 2);
               
                SendBroadcast(data["input"].ToString());
            }
            else
            {
                //if(currentRound != -1 && fromDeviceID == controllingID)
                //{
                //    //outputText.fontSize = 72;
                //    outputText.text += "\n" + data["input"].ToString();
                //}
            }
        }
        if (data["action"] != null)
        {
            switch (data["action"].ToString())
            {
                case "gameModeSelected":
                    int gameMode = (int)data["mode"];  // Assuming mode is an integer from 0 to 3
                    StartGameWithMode(gameMode);
                    HideGameModeSelection();
                    break;
            }
        }


        if (data["action"] != null && data["action"].ToString() == "vote" && !playerDead[fromDeviceID])
        {
            int votedPlayerId = (int)data["votedPlayerId"];
            if (!playerDead[votedPlayerId]) // Ensure both voter and voted are alive
            {
                votes[fromDeviceID] = votedPlayerId;
                CheckIfAllVotesAreIn();
            }
        }
    
            


        if (data["action"] != null && data["action"].ToString() == "submitInfo")
        {
            // Store the player's name and preferences
            string playerName = data["name"].ToString();
            playerNames[fromDeviceID] = playerName;


            List<string> preferences = new List<string>
            {
                data["answer1"].ToString(),
                data["answer2"].ToString()
            };
            playerPreferences[fromDeviceID] = preferences;


            // Update the player's display name in Unity
            UpdatePlayerName(fromDeviceID, playerName);


            // Confirm setup completion to the player
            JObject response = new JObject();
            response["action"] = "allSet";
            AirConsole.instance.Message(fromDeviceID, response);


            UpdateAndBroadcastPlayerData();
        }


        if (data["action"] != null && data["action"].ToString() == "assassinKill")
        {
            int targetPlayerId = data["playerId"].ToObject<int>();
            KillPlayer(targetPlayerId, fromDeviceID);
        }






        
        
    }


    // Start is called before the first frame update
    void Start()
    {
        enoughPlayers = false;
        answers = new List<GameObject>();
        avatars = new List<GameObject>();
        StartCoroutine(DelayedStart());
        highestAnswers = new List<int>();
        timerNum = roundTime;
        startTimer = false;
        currentRound = -1;
    }


    private IEnumerator DelayedStart()
    {
        // Wait for 13 seconds
        yield return new WaitForSeconds(13f);
    }
        // Update is called once per frame
        void Update()
    {
        // Only run the timer after the game has started and there are still more rounds
        if (startTimer && currentRound <= totalRounds)
        {
            timerNum -= Time.deltaTime;
            timerText.text = timerNum.ToString("F0");
            if (timerNum <= 0)
            {
                StartVoting();  


                // Call the method to initiate voting
                startTimer = false;
                timerNum = 10;


            }
           
        }
    }


    void SendBroadcast(JToken message)
    {
        // Assuming AirConsole.instance.Message can take a JToken directly
        foreach (int deviceID in AirConsole.instance.GetControllerDeviceIds())
        {
            AirConsole.instance.Message(deviceID, message);
        }
    }


    void SendMessageToDevice(int deviceID, JObject data)
    {
        if (!playerDead[deviceID])  // Check if player is not marked as dead
        {
            AirConsole.instance.Message(deviceID, data);
        }
        else
        {
            Debug.Log("Attempted to send message to a dead player: " + deviceID);
        }
    }


    public void SetupRound()
    {
        if (enoughPlayers)
        {
            // Switch out the main menu to display the gameplay objects
            currentRound = 0;
            joinGame.SetActive(false);
            startGameButton.SetActive(false);
            output.SetActive(true);
            SendBroadcast("SetupRound");
            AssignRoles();
            BroadcastRoles();
            




            // Initiate game mode selection by a randomly chosen player
            StartGameModeSelection();  // This should handle selecting the game mode
        }
    }


    void StartGameModeSelection()
    {
        List<int> deviceIds = AirConsole.instance.GetControllerDeviceIds();
        if (deviceIds.Count == 0) return;


        // Randomly select a player to choose the game mode
        int selectorIndex = UnityEngine.Random.Range(0, deviceIds.Count);
        int selectorDeviceId = deviceIds[selectorIndex];
        controllingID = selectorDeviceId;  // Store the ID of the player who controls the game mode selection


        // Send a message to all devices to update their UI, only the selected player gets the selection UI
        foreach (var deviceId in deviceIds)
        {
            if (deviceId == selectorDeviceId)
            {
                JObject message = new JObject();
                message["action"] = "chooseGameMode";
                AirConsole.instance.Message(deviceId, message);
            }
            else
            {
                JObject message = new JObject();
                message["action"] = "wait";
                AirConsole.instance.Message(deviceId, message);
            }
        }
    }


    void HideGameModeSelection()
    {
        List<int> deviceIds = AirConsole.instance.GetControllerDeviceIds();
        foreach (var deviceId in deviceIds)
        {
            JObject message = new JObject();
            message["action"] = "hideGameModeSelection";
            AirConsole.instance.Message(deviceId, message);
        }
    }


    void UpdatePlayerName(int deviceId, string playerName)
    {
        playerNames[deviceId] = playerName;
        if (playerAvatars.ContainsKey(deviceId))
        {
            playerAvatars[deviceId].GetComponentInChildren<TextMeshProUGUI>().text = playerName;
        }


    }




    void StartGameWithMode(int mode)
    {
        gameMode = mode;
        currentRound = 0;  // Reset or set the round to begin
        outputText.text = "Game Mode: " + GetGameModeName(mode);
        timer.SetActive(true);
        startTimer = true;  // Continue to setup the round as needed


    }


    void SendAssassinMenu(int assassinId)
    {
        var alivePlayers = playerNames
            .Where(p => p.Key != assassinId && !playerDead[p.Key])
            .Select(p => new { id = p.Key, name = p.Value })
            .ToList();


        JObject message = new JObject
        {
            ["action"] = "showAssassinMenu",  // Ensure this matches what your JS expects
            ["players"] = JToken.FromObject(alivePlayers),
            ["from"] = assassinId  // Include sender ID if needed for validation
        };


        AirConsole.instance.Message(assassinId, message);
    }


    string GetGameModeName(int mode)
    {
        switch (mode)
        {
            case 0:
                StartTruthAndLiesMode();
                return "Truth and Lies";
            case 1:
                StartPointBreakMode();
                return "Point Break";
            case 2: 
                StartPeakorPlummetMode();
                return "Peak or Plummet";


            case 3:
                StartCountMeInMode();
                return "Count Me In";
            default: return "Unknown Mode";
        }
    }

    void ProceedWithAssassinAction()
    {
        int assassinId = playerRoles.FirstOrDefault(x => x.Value == "Assassin").Key;
        SendAssassinMenu(assassinId); // Display the Assassin menu to allow them to choose a target
    }


    void AssignRoles()
    {
        List<int> deviceIds = AirConsole.instance.GetControllerDeviceIds();
        int totalPlayers = deviceIds.Count;


        if (totalPlayers < 2)
        {
            Debug.LogError("Not enough players to assign special roles!");
            return; // Ensure there are enough players for the roles you need.
        }


        // Randomly select an index for the leader and assassin
        int leaderIndex = UnityEngine.Random.Range(0, totalPlayers);
        int assassinIndex;
        do
        {
            assassinIndex = UnityEngine.Random.Range(0, totalPlayers);
        } while (assassinIndex == leaderIndex); // Ensure the assassin and leader are not the same player


        for (int i = 0; i < totalPlayers; i++)
        {
            string role = "Citizen"; // Default role
            if (i == leaderIndex)
            {
                role = "Leader";
            }
            else if (i == assassinIndex)
            {
                role = "Assassin";
            }


            playerRoles[deviceIds[i]] = role;
        }
    }


    void BroadcastRoles()
    {
        foreach (KeyValuePair<int, string> entry in playerRoles)
        {
            JObject message = new JObject();
            message["action"] = "assignRole";
            message["role"] = entry.Value;
            AirConsole.instance.Message(entry.Key, message);
        }
    }


    void StartRound()
    {
        
        // Update the header for the selected story type
        if (currentRound == 0)
        {
            switch(gameMode)
            {
               
            }
        }
        // Adding the answer to the main output
        else
        {
            outputText.text += "\n" ;
            // Repositioning the main text to *try* to keep everything on the screen
            outputText.transform.position += new Vector3(0, offset, 0);
        }
        // Reset the answers and voting
        for(int i = 0; i < answers.Count; i++)
        {
            Destroy(answers[i]);
        }
        answers.Clear();
        highestAnswers.Clear();
        votes.Clear();
        //outputText.fontSize = 36;
        currentRound++;
        timerNum = roundTime;
        // Show a different header for each round based on the story type
        
        controllingID = -1;
        SendBroadcast("Reset");
        SendBroadcast("SetupRound");
    }




    void CheckIfAllVotesAreIn()
    {
        var alivePlayerIDs = playerNames.Keys.Where(id => !playerDead[id]).ToList();

        if (votes.Keys.Count == alivePlayerIDs.Count && votes.Keys.All(id => alivePlayerIDs.Contains(id)))
        {
            ProcessVotingResult();
        }
    }


    void ProcessVotingResult()
    {
        if (votes.Count == 0)
        {
            outputText.text = "No votes cast.";
            return;
        }

        // Calculate the most voted player
        var mostVotedPlayerId = votes
            .GroupBy(v => v.Value)
            .OrderByDescending(g => g.Count())
            .First().Key;

        bool isAssassin = playerRoles[mostVotedPlayerId] == "Assassin";

        if (isAssassin)
        {
            outputText.text = $"The Assassin was caught. Game over. The players win!";
            EndGame();
        }
        else
        {
            outputText.text = $"Player {playerNames[mostVotedPlayerId]} was not the Assassin.";
            ProceedWithAssassinAction(); // This should also only affect alive players
        }

    }



    private int pendingAssassinKill = -1;
    void KillPlayer(int targetPlayerId, int assassinId)
    {

        playerDead[targetPlayerId] = true;
        SendGameOverScreen(targetPlayerId);
        string targetName = playerNames[targetPlayerId];

        if (playerRoles[targetPlayerId] == "Leader")
        {
            outputText.text = $"{targetName}, the Leader, has been killed. The Assassin wins!";
            Restart();
        }
        else
        {
            outputText.text = $"{targetName} has been killed.";
            UpdateGameAfterDeath(targetPlayerId);
            StartGameModeSelection();
        }
    }

    void EndGame()
    {
        // Here you can handle things like disabling player inputs, showing final scores, etc.
        
        // Optionally reset the game or go back to the main menu
        // This could involve reloading the scene, clearing data, etc.
        // Example: SceneManager.LoadScene("MainMenu");
    }

    void StartVoting()
    {
        votes.Clear();
        var alivePlayers = playerNames.Where(p => !playerDead[p.Key])
                                      .Select(p => new { id = p.Key, name = p.Value })
                                      .ToList();

        List<JObject> playerData = new List<JObject>();
        foreach (var player in alivePlayers)
        {
            playerData.Add(new JObject {
            {"id", player.id},
            {"name", player.name}
        });
        }

        // Broadcast voting options to all alive players
        JObject message = new JObject
        {
            ["action"] = "startVoting",
            ["players"] = new JArray(playerData)
        };
        BroadcastToAlivePlayers(message);
    }

    void BroadcastToAlivePlayers(JObject message)
    {
        foreach (var deviceId in AirConsole.instance.GetControllerDeviceIds().Where(id => !playerDead[id]))
        {
            AirConsole.instance.Message(deviceId, message);
        }
    }


    void SendQuestions(int assassinId, int leaderId)
    {
        JObject messageToAssassin = new JObject
        {
            ["action"] = "receiveQuestion",
            ["question"] = playerPreferences[leaderId][UnityEngine.Random.Range(0, playerPreferences[leaderId].Count)] // Randomly pick one of the leader's questions
        };
        AirConsole.instance.Message(assassinId, messageToAssassin);


        JObject messageToLeader = new JObject
        {
            ["action"] = "receiveQuestion",
            ["question"] = playerPreferences[assassinId][UnityEngine.Random.Range(0, playerPreferences[assassinId].Count)] // Randomly pick one of the assassin's questions
        };
        AirConsole.instance.Message(leaderId, messageToLeader);
    }


    void ConfirmAssassinKill(int targetPlayerId)
    {
        playerDead[targetPlayerId] = true;  // Now officially mark the player as dead
        string targetName = playerNames[targetPlayerId];
        // Now display the death message
        outputText.text = $"{targetName} has been killed.";
        // Further process like updating UI or game states
        UpdateGameAfterDeath(targetPlayerId);
    }



    IEnumerator DisplayResults(bool isAssassin)
    {


        yield return new WaitForSeconds(3); // Show results for 3 seconds
        if (isAssassin)
        {


            outputText.text = "The Assassin has been found. Game over.";
            yield return new WaitForSeconds(3);
            Restart();
        }
        else
        {
            
            yield return new WaitForSeconds(3);
            outputText.text = "Incorrect guess. The assassin still remains. The game continues...";
            yield return new WaitForSeconds(3);
            StartGameModeSelection(); // Restart game mode selection
        }
    }

    private void Restart()
    {
        AirConsole.instance.Broadcast(new { action = "reset" });
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void RepositionList(List<GameObject> list, float height)
    {
        // Evenly spaces out the players and the answers on the screen horizontally
        int i = 1;
        foreach(GameObject item in list)
        {
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.SetPositionAndRotation(new Vector3(((Screen.width / (list.Count + 1)) * i), height, 0), Quaternion.identity);
            i++;
        }
    }


    void UpdateGameAfterDeath(int playerId)
    {
        if(playerAvatars.ContainsKey(playerId)) {
            GameObject avatar = playerAvatars[playerId];
            Destroy(avatar);  // Remove the avatar from the scene
            playerAvatars.Remove(playerId);  // Clean up the dictionary
            
        }

    }

    private void SendGameOverScreen(int playerId)
    {
        JObject message = new JObject
        {
            ["action"] = "showGameOverScreen"
        };
        AirConsole.instance.Message(playerId, message);
    }


    void UpdateAndBroadcastPlayerData()
    {
        List<JObject> playerData = new List<JObject>();
        foreach (var kvp in playerNames)
        {
            playerData.Add(new JObject {
            {"id", kvp.Key},
            {"name", kvp.Value}
        });
        }


        JObject message = new JObject();
        message["action"] = "updatePlayerData";
        message["players"] = new JArray(playerData);


        AirConsole.instance.Broadcast(message);
    }


    void StartTruthAndLiesMode()
    {
        List<string> truthAndLiesPrompts = new List<string>()
        {       
            "Raise your hand if you’ve ever been to Europe.",
            "Raise your hand if you’ve ever skipped school.",
            "Raise your hand if you’ve ever seen a shooting star.",
            "Raise your hand if you’ve ever run a marathon.",
            "Raise your hand if you’ve ever cooked a meal for more than 10 people.",
            "Raise your hand if you’ve ever been on TV.",
            "Raise your hand if you’ve ever met a celebrity.",
            "Raise your hand if you’ve ever lived in another country.",
            "Raise your hand if you’ve ever gone skydiving.",
            "Raise your hand if you’ve ever learned to play a musical instrument.",
            "Raise your hand if you’ve ever forgotten a close friend's or family member's birthday.",
            "Raise your hand if you’ve ever been to a concert.",
            "Raise your hand if you’ve ever had a friendship that lasted more than 10 years.",
            "Raise your hand if you’ve ever donated blood.",
            "Raise your hand if you like sushi.",
            "Raise your hand if you like Marvel more than DC",
            "Raise your hand if you've ever been to Disneyland",
            "Raise your hand if you've ever tried sushi.",
            "Raise your hand if you've ever taken a cooking class.",
            "Raise your hand if you've ever participated in a talent show.",
            "Raise your hand if you've ever taken part in a food eating contest.",
            "Raise your hand if you've ever watched all the 'Star Wars' movies in order.",
            "Raise your hand if you've ever read all the books in the 'Harry Potter' series.",
            "Raise your hand if you've ever learned a dance from a music video.",
            "Raise your hand if you've ever collected action figures.",
            "Raise your hand if you've ever met a NBA player.",
            "Raise your hand if your favorite sports team has won a championship.",
            "Raise your hand if you think ranch is better than ketchup.",
            "Raise your hand if you've ever lined up for a limited edition sneaker or apparel release.",
            "Raise your hand if you've ever participated in a fantasy sports league",
            "Raise your hand if you've ever visited a filming location of a popular show or movie."
            };


       



        // Randomize prompts or select a specific one
        var selectedPrompt = truthAndLiesPrompts[UnityEngine.Random.Range(0, truthAndLiesPrompts.Count)];


        // Broadcast the selected prompt to all players except the assassin
        foreach (var deviceId in AirConsole.instance.GetControllerDeviceIds())
        {
            if (playerRoles[deviceId] != "Assassin")
            {
                JObject message = new JObject
                {
                    ["action"] = "displayPrompt",
                    ["prompt"] = selectedPrompt
                };
                AirConsole.instance.Message(deviceId, message);
            }
            else
            {
                // Send a different message to the assassin
                JObject assassinMessage = new JObject
                {
                    ["action"] = "displayPrompt",
                    ["prompt"] = "Blend In"
                };
                AirConsole.instance.Message(deviceId, assassinMessage);
            }
        }


        // Start a countdown before showing "GO!"
        StartCoroutine(ShowGoCountdown());
    }


    void StartPointBreakMode()
    {
        List<string> PointBreakPrompts = new List<string>()
        {
            "Point at the person you think would be the best secret agent in a spy movie.",
            "Point at the person who would survive the longest on a deserted island.",
            "Point at the person who would most likely to talk their way out of a tricky situation.",
            "Point at the person who would throw the best party.",
            "Point at the person with the longest hair.",
            "Point at the person who would survive the longest in a zombie apocalypse.",
            "Point at the person who is most likely to try exotic foods.",
            "Point at the person who is most likely to win at trivia night.",
            "Point at the person who is always taking photos",
            "Point at the person with the coolest shoes.",
            "Point at the person who looks like they could be a morning TV show host.",
            "Point at the person who is most likely to be found at a coffee shop.",
            "Point at the person with the neatest handwriting.",
            "Point at the person who is most likely to own a pet snake.",
            "Point at the person who is wearing the most jewelry.",
            "Point at the person who always seems to have the latest tech gadget.",
            "Point at the person most likely to go hiking every weekend.",
            "Point at the person who is always the first to try new restaurants.",
            "Point at the person who is most likely to finish a puzzle the fastest.",
            "Point at the person who is always telling jokes.",
            "Point at the person most likely to have a song for every situation.",
            "Point at the person who would be the best at organizing a group trip.",
            "Point at the person who seems like they could have been a pirate in another life."
        };
        // Randomize prompts or select a specific one
        var selectedPrompt = PointBreakPrompts[UnityEngine.Random.Range(0, PointBreakPrompts.Count)];


        // Broadcast the selected prompt to all players except the assassin
        foreach (var deviceId in AirConsole.instance.GetControllerDeviceIds())
        {
            if (playerRoles[deviceId] != "Assassin")
            {
                JObject message = new JObject
                {
                    ["action"] = "displayPrompt",
                    ["prompt"] = selectedPrompt
                };
                AirConsole.instance.Message(deviceId, message);
            }
            else
            {
                // Send a different message to the assassin
                JObject assassinMessage = new JObject
                {
                    ["action"] = "displayPrompt",
                    ["prompt"] = "Blend In"
                };
                AirConsole.instance.Message(deviceId, assassinMessage);
            }
        }


        // Start a countdown before showing "GO!"
        StartCoroutine(ShowGoCountdown());
    }

    


    void StartPeakorPlummetMode()
    {
        List<string> PeakorPlummetPrompts = new List<string>()
        {
            "Indicate higher or lower if you drink more than 2.5 cups of coffee per day.",
            "Indicate higher or lower if you have visited more than 5.5 countries..",
            "Indicate higher or lower if you spend more than 2.5 hours per day on social media.",
            "Indicate higher or lower if you exercise more than 4 times a week.",
            "Indicate higher or lower if you've ever watched more than 4.5 movies in a single day.",
            "Indicate higher or lower if you've ever spent more than 2 hours on a phone call.",
            "Indicate higher or lower if you've ever cooked meals for more than 10 people at once.",
            "Indicate higher or lower if you've ever collected more than 50 items of any collectible.",
            "Indicate higher or lower if you've ever slept for more than 12 hours in a stretch.",
            "Indicate higher or lower if you've ever visited more than 3 museums.",
            "Indicate higher or lower if 3 drinks get you buzzed.",
            "Indicate higher or lower if you've ever visited more than 3 museums in a day.",
            "Indicate higher or lower if you've ever attended more than 5 weddings in a year.",
            "Indicate higher or lower if you've ever done more than 20 push-ups in a row.",
            "Indicate higher or lower if you've ever watched more than 6 hours of YouTube in a single day.",
            "Indicate higher or lower if you've ever spent more than 4 hours cooking a single meal.",
            "Indicate higher or lower if you've ever had more than 6 different jobs in your life.",
            "Indicate higher or lower if you've ever run more than 5 miles at once.",
            "Indicate higher or lower if you've ever spent more than $200 on a single shopping trip."






        };
        // Randomize prompts or select a specific one
        var selectedPrompt = PeakorPlummetPrompts[UnityEngine.Random.Range(0, PeakorPlummetPrompts.Count)];


        // Broadcast the selected prompt to all players except the assassin
        foreach (var deviceId in AirConsole.instance.GetControllerDeviceIds())
        {
            if (playerRoles[deviceId] != "Assassin")
            {
                JObject message = new JObject
                {
                    ["action"] = "displayPrompt",
                    ["prompt"] = selectedPrompt
                };
                AirConsole.instance.Message(deviceId, message);
            }
            else
            {
                // Send a different message to the assassin
                JObject assassinMessage = new JObject
                {
                    ["action"] = "displayPrompt",
                    ["prompt"] = "Blend In"
                };
                AirConsole.instance.Message(deviceId, assassinMessage);
            }
        }


        // Start a countdown before showing "GO!"
        StartCoroutine(ShowGoCountdown());
    }


    void SendQuestionsToEachOther(int leaderId, int assassinId)
    {
        if (playerPreferences.ContainsKey(leaderId) && playerPreferences.ContainsKey(assassinId))
        {
            // Retrieve the questions
            List<string> leaderQuestions = playerPreferences[leaderId];
            List<string> assassinQuestions = playerPreferences[assassinId];


            // Construct messages
            JObject leaderMessage = new JObject
            {
                ["action"] = "receiveQuestions",
                ["questions"] = new JArray(assassinQuestions.ToArray())
            };


            JObject assassinMessage = new JObject
            {
                ["action"] = "receiveQuestions",
                ["questions"] = new JArray(leaderQuestions.ToArray())
            };


            // Send messages
            AirConsole.instance.Message(leaderId, leaderMessage);
            AirConsole.instance.Message(assassinId, assassinMessage);
        }
    }


    void StartCountMeInMode()
    {
        List<string> CountMeInPrompts = new List<string>()
        {
            "Hold up how many musical instruments you can play.",
            "Hold up how many concerts you've attended.",
            "Hold up the number of pets you have at home.",
            "Hold up how many countries you've visited.",
            "Hold up how many cars you've owned.",
            "Hold up the number of sports you've played.",
            "Hold up the number of times you've been on a plane.",
            "Hold up the number of tattoos you have.",
            "Hold up the number of times you've seen your favorite movie.",
            "Hold up the number of bicycles you own.",
            "Hold up the number of hours you typically sleep each night.",
            "Hold up the number of apps you use daily on your phone.",
            "Hold up how many times you've been to the ocean.",
            "Hold up the number of musicals you've seen.",
            "Hold up how many times you've volunteered for a charity.",
            "Hold up the number of live sports events you've attended in the last month.",
            "Hold up how many languages are spoken in your family.",
            "Hold up how many pairs of sunglasses you own.",
            "Hold up the number of times you eat out in a week.",
        };
        // Randomize prompts or select a specific one
        var selectedPrompt = CountMeInPrompts[UnityEngine.Random.Range(0, CountMeInPrompts.Count)];


        // Broadcast the selected prompt to all players except the assassin
        foreach (var deviceId in AirConsole.instance.GetControllerDeviceIds())
        {
            if (playerRoles[deviceId] != "Assassin")
            {
                JObject message = new JObject
                {
                    ["action"] = "displayPrompt",
                    ["prompt"] = selectedPrompt
                };
                AirConsole.instance.Message(deviceId, message);
            }
            else
            {
                // Send a different message to the assassin
                JObject assassinMessage = new JObject
                {
                    ["action"] = "displayPrompt",
                    ["prompt"] = "Blend In"
                };
                AirConsole.instance.Message(deviceId, assassinMessage);
            }
        }


        // Start a countdown before showing "GO!"
        StartCoroutine(ShowGoCountdown());
    }






    IEnumerator ShowGoCountdown()
    {
        yield return new WaitForSeconds(3); // Wait for 3 seconds
        outputText.text = "Look at your device";
        yield return new WaitForSeconds(10); // Show "GO!" for 10 seconds
        StartVoting();
    }


    JArray GetRandomQuestions()
    {
        List<string> questions = new List<string>
        {
            "What is your favorite animal?",
            "What is your favorite color?",
            "What is your favorite food?"
        };


        // Shuffle and pick two random questions
        var selectedQuestions = questions.OrderBy(q => Guid.NewGuid()).Take(2).ToList();
        return new JArray(selectedQuestions[0], selectedQuestions[1]);
        }
    
    }