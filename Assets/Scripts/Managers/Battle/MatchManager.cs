using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ImprovedTimers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityUtils;
using Random = UnityEngine.Random;

public class MatchManager : NetworkSingleton<MatchManager>
{

    public List<DroneSpawner> teams = new List<DroneSpawner>();
    //public List<TeamData, DroneSpawner> teams = new List<TeamData, DroneSpawner>();

    public List<Transform> spawnPoints = new List<Transform>();

    public GameObject playerSpawner;
    public GameObject aiSpawner;

    public int winner =-1;

    public readonly int playerID = 0;
    
    public TeamData defaultTeam;
    
    TeamData playerData;

    public MatchState matchState = MatchState.PreMatch;
    
    public MatchManagerUI matchManagerUI;
    
    CountdownTimer moneyTick;

    public List<AiPlayer> AiPlayers = new List<AiPlayer>();
    
    public struct AiPlayer
    {
        public string name;
        // add slot indices
        public AiPlayer(string name)
        {
            this.name = name;
        }
    }
    
    public enum MatchState
    {
        PreMatch, Match
    }
    
    [System.Serializable]
    public class TeamData
    {
        public Color colour;
        public bool isAI;
        public float budget;

        public float curIncome;

        public float incomeMultiplier = 1;

        public float clientID = 0;
        
        public void DeductMoney(float amount)
        {
            budget -= amount;
        }
        public void AddMoney(float amount)
        {
            budget += amount;
        }

        public bool CanAfford(float amount)
        {
            if (budget >= amount)
            {
                return true;
            }
            else
            {
                Debug.Log("CANT AFFORD $" + amount + " ONLY HAS $" + budget);
                return false;
            }
        }
    }
    

    public void PlayerJoined(ulong id)
    {
        
    }

    public TeamData PlayerData()
    {
        return playerData;
    }

    public DroneSpawner TeamSpawner(int teamID)
    {
        
        if (NetworkManager.Singleton.IsListening)
        {
            return teams.FirstOrDefault(x => x.teamID == teamID);
        }
        
        return teams[teamID];
    }

    public int RegisterTeam(DroneSpawner spawner)
    {
        if (!spawner.teamData.isAI)
            playerData = spawner.teamData;

        int curIndex = teams.Count;

        NetworkDroneSpawnerHelper networkDroneSpawnerHelper = spawner.GetComponent<NetworkDroneSpawnerHelper>();
        
        teams.Add(spawner);
        
        if (GameManager.Instance.IsOnlineAndClient() && networkDroneSpawnerHelper != null)
            return (int)networkDroneSpawnerHelper.playerClientID.Value;
        
        spawner.transform.position = spawnPoints[curIndex].position;
        spawner.transform.rotation = spawnPoints[curIndex].rotation;
        

        if (NetworkManager.Singleton.IsListening && networkDroneSpawnerHelper != null)
        {
            return (int)networkDroneSpawnerHelper.playerClientID.Value;
        }

        /*if (spawner.GetType() == typeof(AIDroneSpawner))
        {
            
        }*/
        
        return curIndex;
    }

    public void RegisterAiPlayer()
    {
        AiPlayers.Add(new AiPlayer("Bobby"+Random.Range(0,100)));
    }
    
    protected override void Awake()
    {
        base.Awake();
        DebugLogger.Instance.Log("MATCH MANAGER AWAKE");
        matchManagerUI.Init(this);
    }

    public bool IsPlayerOwned(DroneController controller)
    {
        return controller.curTeam == (int)NetworkManager.Singleton.LocalClientId;
    }

    void Update()
    {
        matchManagerUI.Update();
    }

    public void StartMatch()
    {
        StartMatchRPC();
        matchState = MatchState.Match;
        ClearTeams();
        winner = -1;
        ResetMapObjectives();
        
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.Log("ONLINE MATCH STARTED");
            AddNetworkPlayers();
        }
        else
        {
            AddLocalPlayer();
        }

        foreach (AiPlayer aiPlayer in AiPlayers)
        {
            AddAIPlayer(aiPlayer);
        }
        
        moneyTick = new CountdownTimer(1);
        moneyTick.OnTimerStop += IncrementMoney;
        moneyTick.Start();
    }

    [Rpc(SendTo.Everyone)]
    public void StartMatchRPC()
    {
        Debug.Log("START MATCH");
        FindFirstObjectByType<LobbyUI>().OpenCloseMenu(false);
    }

    void ResetMapObjectives()
    {
        List<MapObjectivePoint> objectives = FindObjectsOfType<MapObjectivePoint>().ToList();
        
        foreach (var objective in objectives)
        {
            objective.Reset();
        }
    }
    
    void EndMatch()
    {
        //TODO
        matchState = MatchState.PreMatch;
        moneyTick.Dispose();
        ClearTeams();
        Utils.DestroyAllDrones();
    }

    public void ClearTeams()
    {
        foreach (DroneSpawner team in teams)
        {
            if(team == null)
                continue;
            
            Debug.Log("DESTROYING " + team.gameObject);

            if (NetworkManager.Singleton.IsListening)
            {
                NetworkObject netObj = team.GetComponent<NetworkObject>();
                netObj.Despawn(false);
            }
            
            Destroy(team.gameObject);
        }
        teams.Clear();
    }
    
    void AddAIPlayer(AiPlayer aiPlayer)
    {
        int curIndex = 1;
        
        
        GameObject aiSpawnerClone = Instantiate(aiSpawner, spawnPoints[curIndex].position, spawnPoints[curIndex].rotation);
    }

    void AddLocalPlayer() => Instantiate(playerSpawner);

    void AddNetworkPlayers()
    {
        List<ulong> players = NetworkManager.Singleton.ConnectedClientsIds.ToList();

        foreach (ulong clientID in players)
        {
            AddNetworkPlayer(clientID);
        }
    }
    
    void AddNetworkPlayer(ulong clientID)
    {
        if(!NetworkManager.Singleton.IsServer)
            return;
        
        int curIndex = (int)clientID;
        
        GameObject networkSpawnerClone = Instantiate(playerSpawner, spawnPoints[curIndex].position, spawnPoints[curIndex].rotation);
        NetworkObject netObj = networkSpawnerClone.GetComponent<NetworkObject>();
        
        NetworkDroneSpawnerHelper spawnerHelper = networkSpawnerClone.GetComponent<NetworkDroneSpawnerHelper>();

        spawnerHelper.Init(clientID);
    }

    void IncrementMoney()
    {
        
        if(GameManager.Instance.IsOnlineAndClient())
            return;
        
        List<MapObjectivePoint> objectives = FindObjectsOfType<MapObjectivePoint>().ToList();

        int[] heldObjectives = new int[teams.Count];
        
        foreach (var objective in objectives)
        {
            if (objective.currentOwner.HasValue)
            {
                heldObjectives[objective.currentOwner.Value]++;
                Team(objective.currentOwner.Value).AddMoney(objective.income * Team(objective.currentOwner.Value).incomeMultiplier);
            }
        }

        //victory check
        for (var i = 0; i < heldObjectives.Length; i++)
        {
            var teamScore = heldObjectives[i];
            if (teamScore == objectives.Count)
            {
                Debug.Log("Team " +i+ " Wins!");
                winner = i;
                EndMatch();
                return;
            }
        }
        
        moneyTick.Reset();
        moneyTick.Start();
    }

    public TeamData Team(int teamID)
    {
        if (teams.Count > teamID)
        {
            return teams[teamID].teamData;
        }
        else
        {
            //Debug.LogError("NO TEAM MATCHING INDEX " + teamID);
            return defaultTeam;
        }
    }

    void OnDisable()
    {
        if(moneyTick != null)
            moneyTick.Dispose();
    }

    bool IsOnlineMatch() => NetworkManager.Singleton.IsListening;

    void OnGUI()
    {
        if (GameManager.Instance.currentGameMode != GameMode.Battle)
            return;

        // Get the screen width and height
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Label to indicate whether it's a local or online match
        GUIStyle matchTypeStyle = new GUIStyle
        {
            fontSize = 18,
            normal = { textColor = Color.white },
            alignment = TextAnchor.UpperLeft
        };
        //GUI.Label(new Rect(10, 10, 300, 30), IsOnlineMatch() ? "Match Type: Online" : "Match Type: Local", matchTypeStyle);
        
        

        // Define the style for the winner text
        GUIStyle winnerStyle = new GUIStyle
        {
            fontSize = 32,
            normal = { textColor = Color.yellow },
            alignment = TextAnchor.MiddleCenter
        };

        // Check if the game is over and display the winner
        if (winner != -1)
        {
            string winningTeam = winner == playerID ? "Player" : $"AI {winner}";
            winningTeam = "" + winner;
            string winnerText = $"Team {winningTeam} Wins!";
            Rect winnerRect = new Rect(screenWidth / 2 - 200, screenHeight / 2 - 50, 400, 100);
            GUI.Label(winnerRect, winnerText, winnerStyle);
        }
    }

}