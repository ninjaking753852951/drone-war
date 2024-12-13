using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityUtils;

public class MatchManager : Singleton<MatchManager>
{

    public List<TeamData> teams = new List<TeamData>();

    public int winner =-1;

    public readonly int playerID = 0;
    
    TeamData playerData;
    
    [System.Serializable]
    public class TeamData
    {
        public Color colour;
        public bool isAI;
        public float budget;

        public float incomeMultiplier = 1;
        
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

    TeamData PlayerData()
    {
        foreach (var team in teams)
        {
            if (!team.isAI)
            {
                return team;
            }
        }

        return null;
    }

    public float PlayerBudget()
    {
        return playerData.budget;
    }

    public int TeamID(TeamData team)
    {
        return 0;
    }

    public DroneSpawner TeamSpawner(int teamID)
    {
        return null;
    }

    void Awake()
    {
        playerData = PlayerData();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if(GameManager.Instance.currentGameMode == GameMode.Battle)
            InvokeRepeating(nameof(IncrementMoney),0,1);
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (teams[teamID] != null)
        {
            return teams[teamID];
        }
        else
        {
            Debug.LogError("NO TEAM MATCHING INDEX " + teamID);
            return null;
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
