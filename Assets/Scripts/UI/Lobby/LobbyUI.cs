using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    
    [FormerlySerializedAs("lobbyMenu")]
    public Transform inLobbyParent;
    
    [FormerlySerializedAs("lobbySearchMenu")]
    public Transform lobbySearchParent;
    public LobbyController lobbyController { get; set; }

    public LobbySearchMenu lobbySearchMenuUI;
    public InLobbyMenu inLobbyMenu;
    NetworkConnectionMenu netMenu;
    MatchManager matchManager;
    
    [Serializable]
    public class InLobbyMenu
    {
        LobbyUI ui;
        
        public TextMeshProUGUI playerListText;
        public Button startLobbyButton;
        public Button leaveLobby;
        public Button joinLobbyButton;
        public GameObject outOfLobbyGroup;
        public GameObject inLobbyGroup;
        public TMP_InputField ipField;
        public Button startMatchButton;
        public Button addAiPlayer;
        public Button goToMainMenu;
        public TextMeshProUGUI waitingForMatchMessage;
        public TMP_Dropdown aiDifficulty;
        
        public void Init(LobbyUI ui)
        {
            this.ui = ui;
            
            startLobbyButton.onClick.AddListener(ui.netMenu.StartHost);
            joinLobbyButton.onClick.AddListener(ui.StartClient);
            leaveLobby.onClick.AddListener(ui.netMenu.Disconnect);
            ipField.text = ui.netMenu.ipAddress;
            addAiPlayer.onClick.AddListener(ui.AddAIPlayer);
            startMatchButton.onClick.AddListener(ui.StartMatch);
        }

        public void Update()
        {
            string playerList = "";
            foreach (ulong clientsId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                playerList +=  $"Client ID: {clientsId} \n";
            }

            foreach (var aiPlayer in ui.matchManager.AiPlayersToSpawn)
            {
                playerList +=  $"AI {aiPlayer.name} \n";
            }
        
            playerListText.text = playerList;
            
            outOfLobbyGroup.SetActive(!NetworkManager.Singleton.IsConnectedClient);
            inLobbyGroup.SetActive(NetworkManager.Singleton.IsConnectedClient);
            addAiPlayer.gameObject.SetActive(NetworkManager.Singleton.IsHost);

            startMatchButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);
            waitingForMatchMessage.gameObject.SetActive(!NetworkManager.Singleton.IsHost);
        }
    }
    
    [Serializable]
    public class LobbySearchMenu
    {
        public GameObject lobbyListEntry;

        public Transform lobbyListParent;
        
        public Button refreshLobbyListButton;
        public Button createLobbyButton;

        LobbyUI ui;
        
        public void Init(LobbyUI ui)
        {
            this.ui = ui;
            refreshLobbyListButton.onClick.AddListener(RefreshLobbies);
            createLobbyButton.onClick.AddListener(CreateLobby);
        }
        
        public void ShowLobbies(List<Lobby> lobbies)
        {
            Utils.DestroyAllChildren(lobbyListParent);
            
            foreach (Lobby lobby in lobbies)
            {
                GameObject lobbyListEntryClone = Instantiate(lobbyListEntry, lobbyListParent);
                FlexibileUIListItem uiListItem = lobbyListEntryClone.GetComponent<FlexibileUIListItem>();
                uiListItem.GetButton(0).onClick.AddListener(() => ui.lobbyController.JoinLobbyByID(lobby));
                uiListItem.GetText(0).text = lobby.Name;
                uiListItem.GetText(1).text = lobby.Players.Count +"/"+lobby.MaxPlayers;
            }
        }

        public async void CreateLobby()
        {
            refreshLobbyListButton.gameObject.SetActive(false);
            createLobbyButton.gameObject.SetActive(false);

            await ui.lobbyController.CreateLobby();
            
            refreshLobbyListButton.gameObject.SetActive(true);
            createLobbyButton.gameObject.SetActive(true);

        }

        public async void RefreshLobbies()
        {
            refreshLobbyListButton.gameObject.SetActive(false);
            createLobbyButton.gameObject.SetActive(false);

            await ui.lobbyController.ListLobbies();
            
            refreshLobbyListButton.gameObject.SetActive(true);
            createLobbyButton.gameObject.SetActive(true);
        }
    }
    
    public void Awake()
    {
        lobbyController = FindFirstObjectByType<LobbyController>();
        netMenu = FindFirstObjectByType<NetworkConnectionMenu>();
        matchManager = FindFirstObjectByType<MatchManager>();
        lobbySearchMenuUI.Init(this);
        inLobbyMenu.Init(this);
    }

    void Start()
    {
        inLobbyMenu.goToMainMenu.onClick.AddListener(GameManager.Instance.GoToMainMenu);
        
        lobbySearchParent.gameObject.SetActive(false);
        
        OpenCloseMenu(GameManager.Instance.currentGameMode == GameMode.Battle);
    }

    public void Update()
    {
        inLobbyParent.gameObject.SetActive(lobbyController.IsInLobby() && (int)MatchManager.Instance.matchState.Value == (int)MatchManager.MatchState.PreMatch);
        lobbySearchParent.gameObject.SetActive(!lobbyController.IsInLobby() && (int)MatchManager.Instance.matchState.Value == (int)MatchManager.MatchState.PreMatch);
        
        if(inLobbyParent.gameObject.activeSelf)
            inLobbyMenu.Update();
    }
    

    public void StartClient()
    {
        netMenu.SetIP(inLobbyMenu.ipField.text);
        netMenu.StartClient();
    }

    public void StartMatch()
    {
        matchManager.StartMatch();
    }

    public void OpenCloseMenu(bool open)
    {
        inLobbyParent.gameObject.SetActive(open);
        lobbySearchParent.gameObject.SetActive(open);
    }

    public void AddAIPlayer()
    {
       matchManager.RegisterAiPlayer((AIPlayer.Difficulty) inLobbyMenu.aiDifficulty.value);
    }
}
