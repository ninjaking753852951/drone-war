using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public TextMeshProUGUI playerListText;
    public Button startLobbyButton;
    public Button leaveLobby;
    public Button joinLobbyButton;
    [FormerlySerializedAs("goToBuildMode")]
    public Button goToMainMenu;
    public GameObject outOfLobbyGroup;
    public GameObject inLobbyGroup;
    public TMP_InputField ipField;
    public Button startMatchButton;
    public TextMeshProUGUI waitingForMatchMessage;
    public GameObject lobbyMenu;
    public Button addAiPlayer;
    //MatchManager match;
    NetworkConnectionMenu netMenu;
    MatchManager matchManager;
    
    public void Awake()
    {
        //NetworkManager.Singleton.OnClientStarted += () => { match = FindFirstObjectByType<MatchManager>(); };
        startMatchButton.onClick.AddListener(StartMatch);
        netMenu = FindFirstObjectByType<NetworkConnectionMenu>();
        startLobbyButton.onClick.AddListener(netMenu.StartHost);
        joinLobbyButton.onClick.AddListener(StartClient);
        leaveLobby.onClick.AddListener(netMenu.Disconnect);
        ipField.text = netMenu.ipAddress;
        addAiPlayer.onClick.AddListener(AddAIPlayer);
        matchManager = FindFirstObjectByType<MatchManager>();
    }

    void Start()
    {
        goToMainMenu.onClick.AddListener(GameManager.Instance.GoToMainMenu);
        
        OpenCloseMenu(GameManager.Instance.currentGameMode == GameMode.Battle);
    }

    public void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.Tab))
            OpenCloseMenu(!lobbyMenu.activeSelf);*/
        
        if(!lobbyMenu.activeSelf) // lobby isnt open so it doesnt need to be updated
            return;
        
        //startMatchButton.gameObject.SetActive(NetworkManager.Singleton.IsServer && match.matchState == MatchManager.MatchState.PreMatch);

        //TODO Separate each player into their own ui group and add support for adding ai with different difficulty
        
        string playerList = "";
        foreach (ulong clientsId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            playerList +=  $"Client ID: {clientsId} \n";
        }

        foreach (var aiPlayer in matchManager.AiPlayers)
        {
            playerList +=  $"AI {aiPlayer.name} \n";
        }
        
        playerListText.text = playerList;
        
        
        outOfLobbyGroup.SetActive(!NetworkManager.Singleton.IsConnectedClient);
        inLobbyGroup.SetActive(NetworkManager.Singleton.IsConnectedClient);
        addAiPlayer.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        //startLobbyButton.gameObject.SetActive(!NetworkManager.Singleton.IsConnectedClient);
        //leaveLobby.gameObject.SetActive(NetworkManager.Singleton.IsConnectedClient);
        startMatchButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        waitingForMatchMessage.gameObject.SetActive(!NetworkManager.Singleton.IsHost);
    }

    public void StartClient()
    {
        netMenu.SetIP(ipField.text);
        netMenu.StartClient();
    }

    public void StartMatch()
    {
        matchManager.StartMatch();
    }

    public void OpenCloseMenu(bool open)
    {
        lobbyMenu.SetActive(open);
    }

    public void AddAIPlayer()
    {
       matchManager.RegisterAiPlayer();
    }
}
