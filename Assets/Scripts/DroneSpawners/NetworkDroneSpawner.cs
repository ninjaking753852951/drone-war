using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class NetworkDroneSpawner : DroneSpawner
{
      
    
    List<MachineSpawnButton> machineSpawnButtons = new List<MachineSpawnButton>();

    public GameObject battleMenu;
    
    public TextMeshProUGUI playerBudgetText;
    
    public Transform spawnMachineUIParent;
    public GameObject spawnMachineButtonPrefab;
    
    public class MachineSpawnButton
    {
        GameObject obj;
        int slot;
        
        public MachineSpawnButton(int slot, MachineSaveData machineData, NetworkDroneSpawner spawner, UnityAction call)
        {
            obj = Instantiate(spawner.spawnMachineButtonPrefab, spawner.spawnMachineUIParent);
            
            this.slot = slot;
            
            if(machineData != null)
                obj.transform.FindChildWithTag("UIIcon").GetComponent<Image>().sprite = machineData.GenerateThumbnail();
            
            obj.GetComponentInChildren<TextMeshProUGUI>().text = slot+".";
            obj.GetComponentInChildren<Button>().onClick.AddListener(call);

            //Update(saveLoadUI);
        }

        public void Update(NetworkDroneSpawner saveLoadUI)
        {

        }
    }
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //Init(0, playerID.Value);
    }


    public void Init()
    {
        battleMenu.SetActive(GameManager.Instance.currentGameMode == GameMode.Battle);
        
        CameraController.Instance.TeleportCamera(transform.position, transform.rotation.eulerAngles);
        
        if (GameManager.Instance.currentGameMode == GameMode.Battle)
            BuildUI();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
    }
    
    void BuildUI()
    {
        // Spawn machine buttons 
        for (int i = 1; i < 10; i++)
        {
            int slot = i;
            MachineSaveData machineData = MachineLibrary.Instance.FetchMachine(slot);/*
            machineSpawnButtons.Add(new MachineSpawnButton(slot, machineData, this, () => SpawnMachine(slot)));*/
            machineSpawnButtons.Add(new MachineSpawnButton(slot, machineData, this, () => SpawnMachineCommand(slot)));
        }
    }

    void SpawnMachineCommand(int slot)
    {
        MachineSaveData machineData = MachineSaveLoadManager.Instance.LoadMachine(slot);
        
        if(!teamData.CanAfford(machineData.totalCost))
            MessageDisplay.Instance.DisplayMessage("INSUFFICIENT FUNDS!");
        
        CommandManager commandManager = FindObjectOfType<CommandManager>();
        if (commandManager != null)
        {
            if (GameManager.Instance.IsOnlineAndClient())
            {
                //Debug.Log("SENDING COMMAND NET");
                commandManager.AddCommandRPC(new CommandManager.Command(NetworkManager.Singleton.LocalClientId,slot).GenerateData());
            }
            else
            {
                //Debug.Log("SENDING COMMAND LOCAL");
                commandManager.AddCommand(new CommandManager.Command((ulong)0, slot));
            }
        }
    }

    void UpdateUI()
    {
        if(MatchManager.Instance.PlayerData() != null)
            playerBudgetText.text = "$" + teamData.budget;
    }
}
