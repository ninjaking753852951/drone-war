using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public TextMeshProUGUI playerListText;
    public Button startLobbyButton;
    public Button leaveLobby;
    public Button joinLobbyButton;
    public Button goToBuildMode;
    public GameObject outOfLobbyGroup;
    public GameObject inLobbyGroup;
    public TMP_InputField ipField;
    public Button startMatchButton;
    public TextMeshProUGUI waitingForMatchMessage;
    public GameObject lobbyMenu;
    //MatchManager match;
    NetworkConnectionMenu netMenu;    
    
    public void Awake()
    {
        //NetworkManager.Singleton.OnClientStarted += () => { match = FindFirstObjectByType<MatchManager>(); };
        startMatchButton.onClick.AddListener(StartMatch);
        netMenu = FindFirstObjectByType<NetworkConnectionMenu>();
        startLobbyButton.onClick.AddListener(netMenu.StartHost);
        joinLobbyButton.onClick.AddListener(StartClient);
        leaveLobby.onClick.AddListener(netMenu.Disconnect);
        ipField.text = netMenu.ipAddress;
        goToBuildMode.onClick.AddListener(GameManager.Instance.SwitchToBuildMode);
        
    }

    void Start()
    {
        OpenCloseMenu(GameManager.Instance.currentGameMode == GameMode.Battle);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
            OpenCloseMenu(!lobbyMenu.activeSelf);
        
        if(!lobbyMenu.activeSelf) // lobby isnt open so it doesnt need to be updated
            return;
        
        //startMatchButton.gameObject.SetActive(NetworkManager.Singleton.IsServer && match.matchState == MatchManager.MatchState.PreMatch);

        string playerList = "";
        foreach (ulong clientsId in  NetworkManager.Singleton.ConnectedClientsIds)
        {
            playerList +=  $"Client ID: {clientsId} \n";
        }
        playerListText.text = playerList;
        
        
        outOfLobbyGroup.SetActive(!NetworkManager.Singleton.IsConnectedClient);
        inLobbyGroup.SetActive(NetworkManager.Singleton.IsConnectedClient);
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
        FindFirstObjectByType<MatchManager>().StartMatch();
    }

    public void OpenCloseMenu(bool open)
    {
        lobbyMenu.SetActive(open);
    }
}
