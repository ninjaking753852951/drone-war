using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class PlayerDroneSpawner : DroneSpawner
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
        
        public MachineSpawnButton(int slot, MachineSaveData machineData, PlayerDroneSpawner spawner, UnityAction call)
        {
            //Debug.Log("New machine spawn button battle");
            
            obj = Instantiate(spawner.spawnMachineButtonPrefab, spawner.spawnMachineUIParent);
            
            this.slot = slot;

            string cost = "N/A";

            if (machineData != null)
            {
                cost = "$" + machineData.totalCost;
                obj.transform.FindChildWithTag("UIIcon").GetComponent<Image>().sprite = machineData.GenerateThumbnail();
                obj.GetComponentInChildren<Button>().onClick.AddListener(call);
            }
            
            obj.GetComponentInChildren<TextMeshProUGUI>().text =cost;

            //Update(saveLoadUI);
        }

        public void Update(PlayerDroneSpawner saveLoadUI)
        {

        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        battleMenu.SetActive(false);
    }
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if(!NetworkManager.Singleton.IsListening)
            Init();
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
        if (teamID == (int)NetworkManager.Singleton.LocalClientId)
        {
            UpdateUI();
        }
    }
    
    void BuildUI()
    {
        // Spawn machine buttons 
        for (int i = 0; i < 10; i++)
        {
            int slot = i;
            MachineSaveData machineData = MachineLibraryManager.Instance.FetchMachine(slot);
            if(machineData == null)
                continue;
            machineSpawnButtons.Add(new MachineSpawnButton(slot, machineData, this, () => SpawnMachineCommand(slot)));
        }
    }

    void SpawnMachineCommand(int slot)
    {
        MachineSaveData machineData = MachineSaveLoadManager.Instance.LoadMachine(slot);
        
        
        if(!teamData.CanAfford(machineData.totalCost))
            ScreenMessageDisplay.Instance.DisplayMessage("INSUFFICIENT FUNDS!");
        
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
            playerBudgetText.text = "$" + (int)teamData.budget + " + " + (int)teamData.curIncome + "/s";
    }
}
