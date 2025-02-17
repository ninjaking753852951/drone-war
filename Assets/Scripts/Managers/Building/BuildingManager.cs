using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BuildTools;
using Interfaces;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using BuildTools;

public class BuildingManager : UnityUtils.Singleton<BuildingManager>
{
    public BuildingManagerUI ui;
    
    public Material indicatorMat;
    public Material badIndicatorMat;

    public LayerMask placementMask;
    
    GameObject buildingBlockIndicator;
    
    int indicatorRotDirIndex;
    readonly float[] rotationAngles = { 0, 90, 180, 270 };
    
    public Vector3 spawnPoint;
    
    List<IPlaceable> allPlaceables;
    public float totalCost { get; private set; }
    IPlaceable curPlaceable;

    bool isOnCompatibleBlock;
    public BuildingBlockSelector blockSelector { get; private set; }

    [FormerlySerializedAs("placeBlockSfx")]
    [Header("Building Effects")]
    public VFXData placeBlockVfx;
    
    [Header("Build Tools")]
    public Tool moveTool;
    public RotateTool rotateTool;
    public BaseTool deleteMachineTool;
    public BaseTool placeTool;
    public ToolMode curTool { get; set; }
    ToolMode oldTool;
    Camera cam;
    public enum ToolMode
    {
        Place, Move, Rotate, DeleteMachine
    }

    protected override void Awake()
    {
        base.Awake();
        ui = FindFirstObjectByType<BuildingManagerUI>();
        blockSelector = GetComponent<BuildingBlockSelector>();
        InitTools();
        SetBuildTool(ToolMode.Place);
    }

    void InitTools()
    {
        moveTool.Init((this));
        rotateTool.Init(this);
        deleteMachineTool.Init(this);
        placeTool.Init(this);
    }
    
    void Start()
    {
        cam = Camera.main;
        if(GameManager.Instance.currentGameMode == GameMode.Build)
            EnterBuildMode();
    }
    
    void Update()
    {
        if(UIManager.instance.IsOpen())
            return;
        
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
    
    void BuildUpdate()
    {

        switch (curTool) // update tool
        {
            case ToolMode.Place:
                PlacementUpdate();
                break;
            case ToolMode.Move:
                moveTool.Update();
                if(buildingBlockIndicator != null)
                    buildingBlockIndicator.SetActive(false);
                break;
            case ToolMode.Rotate:
                rotateTool.Update();
                if(buildingBlockIndicator != null)
                    buildingBlockIndicator.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void DisableAllBuildTools()
    {
        SetActivePlacementMode(false);
        moveTool.SetActive(false);
        rotateTool.SetActive(false);
        deleteMachineTool.SetActive(false);
        placeTool.SetActive(false);
    }

    void PlacementUpdate()
    {

        if(cam == null || Utils.IsCursorOutsideCameraFrustum(cam))
            return;
        
        // Rotate the block
        if (Input.GetKeyDown(KeyCode.R))
        {
            indicatorRotDirIndex = (int)Mathf.Repeat(indicatorRotDirIndex + 1, 4);
        }
        
        //Place the block
        if (Input.GetButtonDown("Fire1") && buildingBlockIndicator.activeSelf && isOnCompatibleBlock)
        {
            
            Vector3 placePosition = buildingBlockIndicator.transform.position;
            
            VFXManager.instance.Spawn(placeBlockVfx,  placePosition, quaternion.identity, false);
            
            if (curPlaceable is IStepPlaceable stepPlaceable)
            {
                stepPlaceable.OnStepCompleted(placePosition);
            }
            else
            {
                curPlaceable.Spawn(placePosition, buildingBlockIndicator.transform.rotation);
            }
            UpdateTotalCost();
        }


        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hasHitDroneBlock = false;
        if (Physics.Raycast(ray, out hit, 100, placementMask))
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
                    UpdateTotalCost();
                }
            }
        }
        
        if(buildingBlockIndicator != null)
            buildingBlockIndicator.SetActive(hasHitDroneBlock);
    }

    void UpdateTotalCost()
    {
        totalCost = TotalCost();
    }

    float TotalCost()
    {
        List<DroneBlock> droneBlocks = FindObjectsOfType<DroneBlock>().ToList();
        float cost = 0;
        foreach (var droneBlock in droneBlocks)
        {
            cost += droneBlock.stats.QueryStat(Stat.Cost);
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
        PhysParent parent = FindFirstObjectByType<PhysParent>();
        if(parent != null)
            DestroyImmediate(parent.gameObject);

        MachineSaveLoadManager.instance.LoadMachine(MachineSaveLoadManager.instance.curSlot).Spawn(network: false);
        UpdateTotalCost();
        SetNewCurrentBlock(BlockLibraryManager.Instance.blocks[0]);
    }

    public void DeleteMachineTool(bool delete)
    {
        if (delete)
        {
            Utils.DestroyAllDrones();
            new MachineSaveData().Spawn(network: false);   
        }
        SetBuildTool(ToolMode.Place);
    }
    
    void ExitBuildMode()
    {
        MachineSaveLoadManager.Instance.SaveMachine(MachineSaveLoadManager.instance.curSlot);
        GameManager.Instance.ExitBuildMode();
        buildingBlockIndicator.SetActive(false);
        moveTool.toolGizmo.gameObject.SetActive(false);
        rotateTool.toolGizmo.gameObject.SetActive(false);
        
        Utils.DestroyAllDrones();
        
        MachineSaveLoadManager.Instance.LoadMachine(MachineSaveLoadManager.instance.curSlot).Spawn(network: false, deploy:true);
    }

    void SetIndicatorColour(Material mat)
    {
        List<Renderer> rends = buildingBlockIndicator.GetComponentsInChildren<Renderer>().ToList();
        foreach (Renderer rend in rends)
        {
            if(!rend.transform.CompareTag("IgnoreMaterialOverride"))
                rend.material = mat;
        }
    }

    public void SetBuildTool(ToolMode mode)
    {
        curTool = mode;
        DisableAllBuildTools();
        
        switch (curTool)
        {
            case ToolMode.Place:
                placeTool.SetActive(true);
                break;
            case ToolMode.Move:
                moveTool.SetActive(true);
                break;
            case ToolMode.Rotate:
                rotateTool.SetActive(true);
                break;
            case ToolMode.DeleteMachine:
                deleteMachineTool.SetActive(true);
                UIManager.Instance.confirmationBox.Create("Delete Machine?").AddListener(DeleteMachineTool);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void SetActivePlacementMode(bool active)
    {
        if(buildingBlockIndicator != null)
            buildingBlockIndicator.SetActive(active);
    }
    
    public void SetNewCurrentBlock(IPlaceable placeable)
    {
        curTool = ToolMode.Place;
        
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
