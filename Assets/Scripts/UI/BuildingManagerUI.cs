using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BuildingManagerUI : MonoBehaviour
{

    BuildingManager builder;

    public GameObject menuGameObject;

    public Transform categoryParent;
    public GameObject categoryButtonPrefab;
    public GameObject itemButtonPrefab;
    public Transform placeableParent;
    public Button backButton;

    public BlockDescriptionUI blockDescriptionUI;
    
    public TextMeshProUGUI machineCost;

    BlockType[] placeableCategories = {  BlockType.Structure, BlockType.Movement, BlockType.Power ,BlockType.TurretMounts, BlockType.TurretCores,
        BlockType.TurretBarrels, BlockType.TurretModules };

    public List<BuildToolUI> buildToolUis = new List<BuildToolUI>();
    
    [System.Serializable]
    public class BuildToolUI
    {
        public Button mainButton;
        public Transform optionsMenu;
        public Transform highlight;
        public BuildingManager.ToolMode mode;
        BuildingManagerUI ui;

        public void Init(BuildingManagerUI ui)
        {
            this.ui = ui;
            /*switch (mode) // TODO replace awkward integration in future
            {
                case BuildingManager.ToolMode.Move:
                    ui.builder.moveTool.ui = this;
                    break;
                case BuildingManager.ToolMode.Rotate:
                    ui.builder.rotateTool.ui = this;
                    break;
            }*/
            
            mainButton.onClick.AddListener(ButtonPress);
            SetClosed();
        }

        public void ButtonPress()
        {
            ui.builder.SetBuildTool(mode);
        }

        public void SetOpen()
        {
            Debug.Log("SET Open");
            optionsMenu.gameObject.SetActive(true);
            highlight.gameObject.SetActive(true);
        }
        
        public void SetClosed()
        {
            optionsMenu.gameObject.SetActive(false);
            highlight.gameObject.SetActive(false);
        }
    }
    
    void Awake()
    {
        builder = FindFirstObjectByType<BuildingManager>();
        
        foreach (BuildToolUI buildToolUi in buildToolUis)
        {
            buildToolUi.Init(this);
            /*if(buildToolUi.mode == BuildingManager.ToolMode.Place)
                buildToolUi.SetOpen();*/
        }
    }

    public BuildToolUI FetchBuildToolUI(BuildingManager.ToolMode mode)
    {
        return buildToolUis.FirstOrDefault(x => x.mode == mode);
    }
    
    
    void BuildUI()
    {
        foreach (var t in placeableCategories)
        {
            GameObject categoryButtonClone = Instantiate(categoryButtonPrefab, categoryParent);
            categoryButtonClone.GetComponentInChildren<TextMeshProUGUI>().text = t.ToString();
            categoryButtonClone.GetComponentInChildren<Button>().onClick.AddListener(() => ShowCategory(t));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        BuildUI();

        backButton.onClick.AddListener(GameManager.Instance.GoToMainMenu);
        
        GameManager.Instance.onEnterBuildMode.AddListener(() => SetActiveBuildMenu(true));
        GameManager.Instance.onExitBuildMode.AddListener(() => SetActiveBuildMenu(false));
        SetActiveBuildMenu(GameManager.Instance.currentGameMode == GameMode.Build);
    }

    // Update is called once per frame
    void Update()
    {
        machineCost.text = "$" + builder.totalCost;
    }

    void SetActiveBuildMenu( bool active)
    {
        menuGameObject.SetActive(active);
    }

    public void ShowCategory(BlockType categoryIndex)
    {
        Utils.DestroyAllChildren(placeableParent);
        
        List<IPlaceable> placeables =
            builder.PlaceablesInCategory(categoryIndex);
        
        
        foreach (var placeable in placeables)
        {
            GameObject categoryButtonClone = Instantiate(itemButtonPrefab, placeableParent);
            //Debug.Log(categoryButtonClone.transform.FindChildWithTag("ItemIcon"));
            categoryButtonClone.transform.FindChildWithTag("UIIcon").GetComponent<Image>().sprite = placeable.Thumbnail();
            categoryButtonClone.GetComponentInChildren<TextMeshProUGUI>().text = "$"+placeable.Cost();
            categoryButtonClone.GetComponentInChildren<Button>().onClick.AddListener(() => SetCurrentBlock(placeable));
        }
    }

    public void ShowDescription(IPlaceable placeable)
    {
        blockDescriptionUI.SetBlockName(placeable.PlaceableName());
        blockDescriptionUI.DeactivateAllStats();
        foreach (var stat in placeable.Stats().statEntries)
        {
            blockDescriptionUI.SetStatUI(stat.stat, stat.value);
        }
    }

    public void SetCurrentBlock(IPlaceable placeable)
    {
        ShowDescription(placeable);
        builder.SetNewCurrentBlock(placeable);
    }
}
