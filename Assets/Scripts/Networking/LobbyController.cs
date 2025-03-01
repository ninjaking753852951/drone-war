using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImprovedTimers;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;

public class LobbyController : MonoBehaviour
{

    Lobby hostLobby;
    Lobby joinedLobby;
    
    CountdownTimer heartBeatTimer;

    LobbyUI lobbyUI;

    RelayController relayController;
    
    public bool IsInLobby() => joinedLobby != null;
    
    void Awake()
    {
        relayController = FindFirstObjectByType<RelayController>();
        lobbyUI = FindFirstObjectByType<LobbyUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        heartBeatTimer = new CountdownTimer(15);
        heartBeatTimer.OnTimerStop += HandleLobbyHeartbeat;
        heartBeatTimer.Start();
        
        //await UnityServices.InitializeAsync();

        //AuthenticationService.Instance.SignedIn += () => { Debug.Log("Signed in anonymously"); };
        
        //await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async void HandleLobbyHeartbeat()
    {
        heartBeatTimer.Reset();
        heartBeatTimer.Start();
        
        if (hostLobby != null)
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
    
    public async Task CreateLobby()
    {

        string joinCode = await relayController.CreateRelay();
        
        string lobbyName = "my lobby";
        lobbyName += " (" + System.DateTime.Now.Hour%12 + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second+")";
        int maxPlayers = 4;

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode)}
                }
            };
            
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);

            hostLobby = lobby;
            OnJoinedLobby(hostLobby);
            
            Debug.Log("Made lobby with name " + lobby.Name + " and player cap of " + lobby.MaxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
        
            Debug.Log("lobbies found " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " max player:" + lobby.MaxPlayers + " cur players count:" +  + lobby.Players.Count);
            }
            
            lobbyUI.lobbySearchMenuUI.ShowLobbies(queryResponse.Results);
        }
        catch (LobbyServiceException e)
        {
            Console.WriteLine(e);
        }
    }

    /*async void JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        }
        catch (LobbyServiceException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }*/
    
    public async void JoinLobbyByID(Lobby lobbyToJoin)
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyToJoin.Id);
            OnJoinedLobby(lobby);
        }
        catch (LobbyServiceException e)
        {
            Console.WriteLine(e);
        }
    }

    void OnJoinedLobby(Lobby lobby)
    {
        joinedLobby = lobby;
        
        if (joinedLobby.Data != null && joinedLobby.Data.ContainsKey("RelayJoinCode"))
        {
            string joinCode = joinedLobby.Data["RelayJoinCode"].Value;

            if(relayController.hostAllocation == null)
                relayController.JoinRelay(joinCode);
        }
    }
    
    void OnGUI()
    {
        return;
        
        // Define the width and height of the buttons and input field
        float buttonWidth = 100f;
        float buttonHeight = 50f;
        float inputFieldWidth = 200f;
        float inputFieldHeight = 30f;

        // Calculate the starting position to center the buttons and input field
        float startX = (Screen.width - (buttonWidth * 2 + 10)) / 2; // 10 is the spacing between buttons
        float startY = (Screen.height - buttonHeight) / 2;

        // Create the first button
        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Create Lobby"))
        {
            CreateLobby();
        }

        // Create the input field
        //inputText = GUI.TextField(new Rect(startX, startY - inputFieldHeight - 10, inputFieldWidth, inputFieldHeight), inputText);

        // Create the second button next to the first one
        if (GUI.Button(new Rect(startX + buttonWidth + 10, startY, buttonWidth, buttonHeight), "Query lobbies"))
        {
            ListLobbies();
            //JoinRelay(inputText); // Call the method and pass the input text
        }
        
        if (GUI.Button(new Rect(startX + 2*buttonWidth + 10, startY, 2*buttonWidth, buttonHeight), "Join lobbies"))
        {
            //JoinLobby();
            //JoinRelay(inputText); // Call the method and pass the input text
        }
        
        // create a button to join lobby
    }
}
