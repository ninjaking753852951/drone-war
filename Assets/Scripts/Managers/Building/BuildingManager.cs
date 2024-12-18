using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityUtils;

public class BuildingManager : Singleton<BuildingManager>
{
    public Material indicatorMat;
    public Material badIndicatorMat;
    
    public GameObject buildingBlockIndicator;
    
    int indicatorRotDirIndex;
    
    readonly float[] rotationAngles = { 0, 90, 180, 270 };

    DroneController droneController;
    
    public Vector3 spawnPoint;

    List<IPlaceable> allPlaceables;
    [HideInInspector]
    public float totalCost;
    IPlaceable curPlaceable;

    public TankTrackBuilder tankTrack;

    public StepPlacementManager stepPlacementManager = new StepPlacementManager();
    bool isOnCompatibleBlock;
    
    void Start()
    {
        if(GameManager.Instance.currentGameMode == GameMode.Build)
            EnterBuildMode();
    }

    void Update()
    {
        switch (GameManager.Instance.currentGameMode)
        {
            case GameMode.Build:
                BuildUpdate();
                if(Input.GetKeyDown(KeyCode.Space))
                    ExitBuildMode();
                break;
            case GameMode.TestDrive:
                if(Input.GetKeyDown(KeyCode.Space))
                    EnterBuildMode();
                break;
            default:
                break;
        }
    }

    List<IPlaceable> AllPlaceables()
    {
        if (allPlaceables != null)
        {
            return allPlaceables;
        }

        allPlaceables = new List<IPlaceable>();
        
        foreach (var placeableBlock in BlockLibraryManager.instance.placeableBlocks)
        {
            allPlaceables.Add(placeableBlock);
        }
        
        List<MachineSaveLoadManager.SubAssemblySaveData> subAssemblies = MachineSaveLoadManager.instance.LoadAllSubAssemblies();
        foreach (var subAssembly in subAssemblies)
        {
            if(subAssembly == null)
                continue;
            allPlaceables.Add(subAssembly);
        }

        //allPlaceables.Add(tankTrack);
        
        return allPlaceables;
    }
    
    public List<IPlaceable> PlaceablesInCategory(BlockType targetCategory)
    {
        List<IPlaceable> targetPlaceables = new List<IPlaceable>();

        foreach (var placeable in AllPlaceables())
        {
            if(placeable.Category() == targetCategory)
                targetPlaceables.Add(placeable);
        }

        return targetPlaceables;
    }
    
    public void FindDroneController()
    {
        droneController = FindObjectOfType<DroneController>();
        totalCost = TotalCost();
    }
    
    void BuildUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Rotate the block
        if (Input.GetKeyDown(KeyCode.R))
        {
            indicatorRotDirIndex = (int)Mathf.Repeat(indicatorRotDirIndex + 1, 4);
        }
        
        //Place the block
        if (Input.GetButtonDown("Fire1") && buildingBlockIndicator.activeSelf && isOnCompatibleBlock)
        {
            Vector3 placePosition = buildingBlockIndicator.transform.position;
            
            if (curPlaceable is IStepPlaceable stepPlaceable)
            {
                if (!stepPlacementManager.IsActive)
                {
                    stepPlacementManager.StartPlacement(stepPlaceable);
                }
                stepPlacementManager.ProcessStep(placePosition);
            }
            else
            {
                curPlaceable.Spawn(placePosition, buildingBlockIndicator.transform.rotation);
                totalCost = TotalCost();
            }
            totalCost = TotalCost();
        }


        bool hasHitDroneBlock = false;
        if (Physics.Raycast(ray, out hit))
        {
            DroneBlock hitDroneBlock = hit.collider.GetComponentInParent<DroneBlock>();
            //DroneBlock hitDroneBlock = hit.collider.GetComponent<DroneBlock>();
            hasHitDroneBlock = hitDroneBlock != null;
            
            if (hasHitDroneBlock)
            {
                buildingBlockIndicator.SetActive(true);
                UpdateIndicatorPosition(hit, hitDroneBlock);
                
                // delete block functionality
                if (Input.GetKeyDown(KeyCode.X))
                {
                    Destroy(hitDroneBlock.gameObject);
                    totalCost = TotalCost();
                }
            }
        }
        
        if(buildingBlockIndicator != null)
            buildingBlockIndicator.SetActive(hasHitDroneBlock);
    }

    float TotalCost()
    {
        List<DroneBlock> droneBlocks = FindObjectsOfType<DroneBlock>().ToList();
        float cost = 0;
        foreach (var droneBlock in droneBlocks)
        {
            cost += droneBlock.cost;
        }

        return cost;
    }
    
    void UpdateIndicatorPosition(RaycastHit hit, DroneBlock hitBlock)
    {
        Vector3 hitPoint = hit.point;

        Vector3 placePoint = hitPoint + hit.normal * 0.5f;

        BlockType parentBlock = hitBlock.blockIdentity.Category();
        BlockType childBlock = curPlaceable.Category();

        isOnCompatibleBlock = BlockLibraryManager.Instance.blockRules.IsCombinationTrue(parentBlock, childBlock);
        if(isOnCompatibleBlock)
        {
            SetIndicatorColour(indicatorMat);
        }
        else
        {
            SetIndicatorColour(badIndicatorMat);
        }

        placePoint = Utils.SnapToGrid(placePoint, hitBlock.gridSize, hit.collider.transform.position, hit.collider.transform.rotation);
        
        //buildingBlockIndicator.transform.position = hit.collider.transform.position + hit.normal * 1f;
        buildingBlockIndicator.transform.position = placePoint;

        Vector3 forward = Vector3.Cross(hit.normal, hit.collider.transform.up);
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.Cross(hit.normal, hit.collider.transform.right);
        }

        Quaternion alignToNormal = Quaternion.LookRotation(forward, hit.normal);

        float localYRotation = rotationAngles[indicatorRotDirIndex];
        Quaternion localRotation = Quaternion.Euler(0, localYRotation, 0);

        buildingBlockIndicator.transform.rotation = alignToNormal * localRotation;
    }

    void EnterBuildMode()
    {
        GameManager.Instance.EnterBuildMode();
        
        Utils.DestroyAllDrones();
        MachineSaveLoadManager.instance.LoadAndSpawnMachine(MachineSaveLoadManager.instance.curSlot);// corresponds to active game save slot;
        SetNewCurrentBlock(BlockLibraryManager.Instance.blocks[0]);
    }
    
    public void SpawnDefaultMachine()
    {
        BlockLibraryManager.Instance.coreBlock.Spawn(spawnPoint, quaternion.identity);
    }
    
    void ExitBuildMode()
    {
        MachineSaveLoadManager.Instance.SaveMachine(MachineSaveLoadManager.instance.curSlot);
        GameManager.Instance.ExitBuildMode();
        buildingBlockIndicator.SetActive(false);
        DeployMachine();
    }

    void DeployMachine()
    {
        droneController.Deploy();
    }

    void SetIndicatorColour(Material mat)
    {
        List<Renderer> rends = buildingBlockIndicator.GetComponentsInChildren<Renderer>().ToList();
        foreach (Renderer rend in rends)
        {
            rend.material = mat;
        }
    }

    public void SetNewCurrentBlock(IPlaceable placeable)
    {
        curPlaceable = placeable;
        
        if(buildingBlockIndicator != null)
            Destroy(buildingBlockIndicator);

        if (curPlaceable is IStepPlaceable stepPlaceable)
        {
            buildingBlockIndicator = stepPlaceable.SpawnMarker(Vector3.zero, quaternion.identity);
        }
        else
        {
            buildingBlockIndicator = placeable.Spawn(Vector3.zero, quaternion.identity);
        }
        

        
        List<Collider> indicatorColliders = buildingBlockIndicator.GetComponentsInChildren<Collider>().ToList();
        foreach (Collider indicatorCollider in indicatorColliders)
        {
            indicatorCollider.enabled = false;
        }
        
        List<Rigidbody> indicatorRigidbodies = buildingBlockIndicator.GetComponentsInChildren<Rigidbody>().ToList();
        foreach (Rigidbody indicatorRigidbody in indicatorRigidbodies)
        {
            indicatorRigidbody.isKinematic = true;
            indicatorRigidbody.useGravity = false;
        }
        
        List<DroneBlock> childBlocks = buildingBlockIndicator.GetComponentsInChildren<DroneBlock>().ToList();
        foreach (DroneBlock childBlock in childBlocks)
        {
            Destroy(childBlock);
        }

        SetIndicatorColour(indicatorMat);

        // get the one on the parent as well as the children
        DroneBlock block = buildingBlockIndicator.GetComponent<DroneBlock>();
        if(block != null)
            Destroy(block);
    }
}
