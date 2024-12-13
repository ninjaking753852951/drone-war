using System;
using System.Collections;
using System.Collections.Generic;
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
    [FormerlySerializedAs("buttonPrefab")]
    public GameObject categoryButtonPrefab;
    public GameObject itemButtonPrefab;
    public Transform placeableParent;

    public TextMeshProUGUI machineCost;
    
    public enum PlaceableCategories {Basic, TurretMounts, TurretBarrels, TurretCores, TurretModules, SubAssemblies, Power, Structure}
    PlaceableCategories[] placeableCategories = { PlaceableCategories.Basic, PlaceableCategories.Structure, PlaceableCategories.Power,PlaceableCategories.TurretMounts,
        PlaceableCategories.TurretBarrels, PlaceableCategories.TurretCores, PlaceableCategories.SubAssemblies, PlaceableCategories.TurretModules };
    
    void Awake()
    {
        builder = GetComponent<BuildingManager>();
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

    public void ShowCategory(PlaceableCategories categoryIndex)
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
            categoryButtonClone.GetComponentInChildren<Button>().onClick.AddListener(() => builder.SetNewCurrentBlock(placeable));
        }
    }
}
