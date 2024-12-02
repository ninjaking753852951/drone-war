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
    
    TeamData playerData;
    
    [System.Serializable]
    public class TeamData
    {
        public Color colour;
        public bool isAI;
        public float budget;

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

    float PlayerBudget()
    {
        return playerData.budget;
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
                Team(objective.currentOwner.Value).AddMoney(objective.income);
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

        // Define the style for the budget text
        GUIStyle budgetStyle = new GUIStyle
        {
            fontSize = 18,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };

        // Define the budget text
        string budgetText = $"Player Budget: ${PlayerBudget():0}";

        // Calculate the position and size of the budget text box
        Rect budgetRect = new Rect(screenWidth / 2 - 150, screenHeight - 50, 300, 30);

        // Display the budget text
        GUI.Label(budgetRect, budgetText, budgetStyle);

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
