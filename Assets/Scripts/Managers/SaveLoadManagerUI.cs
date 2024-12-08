using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadManagerUI : MonoBehaviour
{

    public GameObject machineButtonPrefab;
    public Transform machineSelectParent;
    
    MachineSaveLoadManager saveLoad;

    List<MachineSelectionButton> machineSelectionButtons = new List<MachineSelectionButton>();

    class MachineSelectionButton
    {
        GameObject obj;
        int slot;
        
        Image selectionImage;
        
        public MachineSelectionButton(int slot, MachineSaveLoadManager.MachineSaveData machineData, SaveLoadManagerUI saveLoadUI)
        {
            obj = Instantiate(saveLoadUI.machineButtonPrefab, saveLoadUI.machineSelectParent);
            
            this.slot = slot;
            
            if(machineData != null)
                obj.transform.FindChildWithTag("UIIcon").GetComponent<Image>().sprite = machineData.GenerateThumbnail();
            
            selectionImage = obj.transform.FindChildWithTag("UISelection").GetComponent<Image>();
            obj.GetComponentInChildren<TextMeshProUGUI>().text = "Machine Spawn " + slot;
            obj.GetComponentInChildren<Button>().onClick.AddListener(() => saveLoadUI.SwitchMachineButton(slot));

            Update(saveLoadUI);
        }

        public void Update(SaveLoadManagerUI saveLoadUI)
        {
            selectionImage.enabled = saveLoadUI.saveLoad.curSlot == slot;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        saveLoad = GetComponent<MachineSaveLoadManager>();
        BuildUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void BuildUI()
    {
        for (int i = 1; i < 10; i++)
        {
            MachineSaveLoadManager.MachineSaveData machineData = saveLoad.LoadMachine(i);
            machineSelectionButtons.Add(new MachineSelectionButton(i, machineData, this));
        }
    }
    

    void UpdateUI()
    {
        foreach (MachineSelectionButton machineSelectionButton in machineSelectionButtons)
        {
            machineSelectionButton.Update(this);
        }
    }

    void SwitchMachineButton(int slot)
    {
        saveLoad.SwitchMachines(slot);
        UpdateUI();
    }
}
