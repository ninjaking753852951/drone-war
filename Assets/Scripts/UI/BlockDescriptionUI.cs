using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BlockDescriptionUI : MonoBehaviour
{

    public List<StatEntryUI> stats;

    public Transform statListParent;
    public GameObject statEntryUIPrefab;

    public TextMeshProUGUI blockDescription;

    public TextMeshProUGUI blockNameText;
    
    [System.Serializable]
    public class StatEntryUI
    {
        public Stat stat;
        public Sprite icon;
        public string suffix;

        Transform uiParent;
        TextMeshProUGUI valueText;
        Image iconImage;
        
        public void Init(GameObject ui)
        {
            uiParent = ui.transform;
            valueText = ui.GetComponentInChildren<TextMeshProUGUI>();
            iconImage = ui.transform.FindChildWithTag("UIIcon").GetComponent<Image>();
            iconImage.sprite = icon;
            Deactivate();
        }

        public void Activate(float value)
        {
            uiParent.gameObject.SetActive(true);
            valueText.text = value + suffix;
        }
        
        public void Deactivate()
        {
            uiParent.gameObject.SetActive(false);
        }
    }

    void Awake()
    {
        foreach (StatEntryUI stat in stats)
        {
            stat.Init(Instantiate(statEntryUIPrefab, statListParent));
        }
    }

    public void DeactivateAllStats()
    {
        foreach (StatEntryUI stat in stats)
        {
            stat.Deactivate();
        }
    }

    public void SetStatUI(Stat stat, float value)
    {
        StatEntryUI statEntryUI = stats.FirstOrDefault(x => x.stat == stat);

        if (statEntryUI != null)
        {
            statEntryUI.Activate(value);
        }
        else
        {
            Debug.LogWarning("Tried to display stat description but UI element not found");
        }
    }

    public void SetBlockName(string name)
    {
        blockNameText.text = name;
    }

    public void SetDescription(string desc)
    {
        blockDescription.text = desc;
    }
        
        
}