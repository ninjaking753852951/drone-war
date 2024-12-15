using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SaveLoadManagerUI : MonoBehaviour
{

    public GameObject machineButtonPrefab;
    public Transform machineSelectParent;
    
    MachineSaveLoadManager saveLoad;

    List<MachineSelectionButton> machineSelectionButtons = new List<MachineSelectionButton>();

    public class MachineSelectionButton
    {
        GameObject obj;
        int slot;
        
        Image selectionImage;
        
        public MachineSelectionButton(int slot, MachineSaveData machineData, SaveLoadManagerUI saveLoadUI, UnityAction call)
        {
            obj = Instantiate(saveLoadUI.machineButtonPrefab, saveLoadUI.machineSelectParent);
            
            this.slot = slot;
            
            if(machineData != null)
                obj.transform.FindChildWithTag("UIIcon").GetComponent<Image>().sprite = machineData.GenerateThumbnail();
            
            selectionImage = obj.transform.FindChildWithTag("UISelection").GetComponent<Image>();
            obj.GetComponentInChildren<TextMeshProUGUI>().text = "Machine Spawn " + slot;
            obj.GetComponentInChildren<Button>().onClick.AddListener(call);

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
        for (int i = 0; i < 10; i++)
        {
            int slot = i;
            MachineSaveData machineData = MachineLibrary.Instance.FetchMachine(i);
            machineSelectionButtons.Add(new MachineSelectionButton(slot, machineData, this, () => SwitchMachineButton(slot)));
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
