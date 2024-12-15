using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityUtils;
using Random = UnityEngine.Random;

public class MatchManager : Singleton<MatchManager>
{

    public List<DroneSpawner> teams = new List<DroneSpawner>();
    //public List<TeamData, DroneSpawner> teams = new List<TeamData, DroneSpawner>();

    public List<Transform> spawnPoints = new List<Transform>();

    public GameObject playerSpawner;
    public GameObject aiSpawner;
    public GameObject networkSpawner;
    
    public int winner =-1;

    public readonly int playerID = 0;

    public MatchManagerUI ui;

    public TeamData defaultTeam;
    
    TeamData playerData;
    
    [System.Serializable]
    public class TeamData
    {
        public Color colour;
        public bool isAI;
        public float budget;

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

        spawner.transform.position = spawnPoints[curIndex].position;
        spawner.transform.rotation = spawnPoints[curIndex].rotation;
        
        teams.Add(spawner);

        if (NetworkManager.Singleton.IsListening)
        {
            return (int)spawner.GetComponent<NetworkDroneSpawnerHelper>().playerClientID.Value;
        }
        
        return curIndex;
    }
    
    void Awake()
    {
        //playerData = PlayerData();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.currentGameMode == GameMode.Battle)
        {
            Instantiate(playerSpawner);
            InvokeRepeating(nameof(IncrementMoney),0,1);
            NetworkManager.Singleton.OnClientConnectedCallback += AddNetworkPlayer;
            NetworkManager.Singleton.OnServerStarted += StartMatch;
            NetworkManager.Singleton.OnClientStarted += StartMatch;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartMatch()
    {
        // on match start clear any existing players and add in new ones
        ClearTeams();
    }

    void ClearTeams()
    {
        Debug.Log("ONLINE MATCH STARTED");
        foreach (DroneSpawner team in teams)
        {
            Debug.Log("DESTROYING " + team.gameObject);
            Destroy(team.gameObject);
        }
        teams.Clear();
    }
    
    void AddAIPlayer()
    {
        Instantiate(aiSpawner);
    }

    void AddNetworkPlayer(ulong clientID)
    {
        if(!NetworkManager.Singleton.IsServer)
            return;
        
        Debug.Log(clientID);
        
        int curIndex = teams.Count;
        
        GameObject networkSpawnerClone = Instantiate(networkSpawner,spawnPoints[curIndex].position, spawnPoints[curIndex].rotation);
        NetworkObject netObj = networkSpawnerClone.GetComponent<NetworkObject>();
        
        NetworkDroneSpawnerHelper spawnerHelper = networkSpawnerClone.GetComponent<NetworkDroneSpawnerHelper>();

        spawnerHelper.Init(clientID);
    }

    void IncrementMoney()
    {
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
            }
        }
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

    void OnGUI()
    {
        
        
        if (GameManager.Instance.currentGameMode != GameMode.Battle)
            return;

        // Get the screen width and height
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // Define the style for the winner text
        GUIStyle winnerStyle = new GUIStyle
        {
            fontSize = 32,
            normal = { textColor = Color.yellow },
            alignment = TextAnchor.MiddleCenter
        };
        
        // Add a button at the top of the screen offset by 200 pixels
        Rect buttonRect = new Rect(screenWidth / 2 + 100, 40, 100, 30); // Centered horizontally, 200px from the top
        if (GUI.Button(buttonRect, "Add AI Player"))
        {
            AddAIPlayer();
        }
        
        // Check if the game is over and display the winner
        if (winner != -1)
        {
            string winningTeam = "";
            if (winner == 0)
            {
                winningTeam = "Player";
            }
            else
            {
                winningTeam = "AI";
            }
            string winnerText = $"Team {winningTeam} Wins!";
            Rect winnerRect = new Rect(screenWidth / 2 - 200, screenHeight / 2 - 50, 400, 100);
            GUI.Label(winnerRect, winnerText, winnerStyle);
        }
    }

}
