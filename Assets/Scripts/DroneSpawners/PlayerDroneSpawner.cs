using System.Collections;
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
            obj = Instantiate(spawner.spawnMachineButtonPrefab, spawner.spawnMachineUIParent);
            
            this.slot = slot;
            
            if(machineData != null)
                obj.transform.FindChildWithTag("UIIcon").GetComponent<Image>().sprite = machineData.GenerateThumbnail();
            
            obj.GetComponentInChildren<TextMeshProUGUI>().text = slot+".";
            obj.GetComponentInChildren<Button>().onClick.AddListener(call);

            //Update(saveLoadUI);
        }

        public void Update(PlayerDroneSpawner saveLoadUI)
        {

        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        battleMenu.SetActive(GameManager.Instance.currentGameMode == GameMode.Battle);
        
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
            MachineSaveData machineData = MachineSaveLoadManager.Instance.LoadMachine(slot);/*
            machineSpawnButtons.Add(new MachineSpawnButton(slot, machineData, this, () => SpawnMachine(slot)));*/
            machineSpawnButtons.Add(new MachineSpawnButton(slot, machineData, this, () => SpawnMachineCommand(slot)));
        }
    }

    void SpawnMachineCommand(int slot)
    {
        CommandManager commandManager = FindObjectOfType<CommandManager>();
        if (commandManager != null)
        {
            if (GameManager.Instance.IsOnlineAndClient())
            {
                Debug.Log("SENDING COMMAND NET");
                commandManager.AddCommandRPC(new CommandManager.Command(NetworkManager.Singleton.LocalClientId,slot).GenerateData());
            }
            else
            {
                Debug.Log("SENDING COMMAND LOCAL");
                commandManager.AddCommand(new CommandManager.Command((ulong)0, slot));
            }
        }
    }

    void UpdateUI()
    {
        playerBudgetText.text = "$" + MatchManager.Instance.PlayerBudget();
    }
    
}
