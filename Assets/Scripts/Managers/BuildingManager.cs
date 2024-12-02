using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityUtils;

public class BuildingManager : Singleton<BuildingManager>
{
    public Material indicatorMat;
    
    public GameObject buildingBlockIndicator;
    
    int indicatorRotDirIndex;
    
    readonly float[] rotationAngles = { 0, 90, 180, 270 };

    DroneController droneController;
    
    public Vector3 spawnPoint;

    List<Placeable> allPlaceables;
    [HideInInspector]
    public float totalCost;
    Placeable curPlaceable;

    public class Placeable
    {
        public bool isSubAssembly;
        public BlockData block;
        public MachineSaveLoadManager.SubAssemblySaveData subAssembly;

        public Placeable(BlockData block, MachineSaveLoadManager.SubAssemblySaveData subAssembly = null)
        {
            isSubAssembly = block == null;
            this.block = block;
            this.subAssembly = subAssembly;
        }

        public string PlaceableName()
        {
            if (isSubAssembly)
            {
                return "Sub-Assembly ";
            }
            else
            {
                return block.prefab.name;
            }
        }

        public GameObject Spawn(Vector3 pos, Quaternion rot)
        {
            if (isSubAssembly)
            {
                GameObject subAssemblyParent = new GameObject("SubAssemblyParent");
        
                foreach (MachineSaveLoadManager.BlockSaveData blockSaveData in subAssembly.blocks)
                {
                    if(blockSaveData.blockID >= BlockLibraryManager.Instance.blocks.Count)
                        continue;
                
                    BlockData blockData = BlockLibraryManager.Instance.blocks[blockSaveData.blockID];
                    GameObject newBlock = Instantiate(blockData.prefab, blockSaveData.pos, Quaternion.Euler(blockSaveData.eulerRot));

                    newBlock.transform.parent = subAssemblyParent.transform;
                
                    DroneBlock droneBlock = newBlock.GetComponent<DroneBlock>();
                    droneBlock.blockIdentity = blockData;

                }

                subAssemblyParent.transform.position += pos;
                subAssemblyParent.transform.rotation = rot;
                return subAssemblyParent;
            }
            else
            {
                GameObject blockClone = Instantiate(block.prefab, pos, rot);
                blockClone.GetComponent<DroneBlock>().blockIdentity = block;
                return blockClone;
            } 
        }
    }
    
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
                    EnterBuildMode(true);
                
                break;
            default:
                break;

        }

    }

    public List<Placeable> AllPlaceables()
    {
        if (allPlaceables != null)
        {
            return allPlaceables;
        }

        allPlaceables = new List<Placeable>();
        
        foreach (var placeableBlocks in BlockLibraryManager.instance.placeableBlocks)
        {
            allPlaceables.Add(new Placeable(placeableBlocks));
        }
        
        List<MachineSaveLoadManager.SubAssemblySaveData> subAssemblies = MachineSaveLoadManager.instance.LoadAllSubAssemblies();


        foreach (var subAssembly in subAssemblies)
        {
            if(subAssembly == null)
                continue;
            allPlaceables.Add(new Placeable(null, subAssembly));
        }

        return allPlaceables;
    }
    
    public List<Placeable> PlaceablesInCategory(BuildingManagerUI.PlaceableCategories targetCategory)
    {
        List<Placeable> targetPlaceables = new List<Placeable>();

        foreach (var placeable in AllPlaceables())
        {
            if (placeable.isSubAssembly)
            {
                if (targetCategory == BuildingManagerUI.PlaceableCategories.SubAssemblies)
                    targetPlaceables.Add(placeable);
            }
            else
            {
                if (placeable.block.category == targetCategory)
                    targetPlaceables.Add(placeable);
            }
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
        if (Input.GetButtonDown("Fire1") && buildingBlockIndicator.activeSelf)
        {
            curPlaceable.Spawn(buildingBlockIndicator.transform.position, buildingBlockIndicator.transform.rotation);
            totalCost = TotalCost();
        }


        bool hasHitDroneBlock = false;
        if (Physics.Raycast(ray, out hit))
        {
            DroneBlock hitDroneBlock = hit.collider.GetComponent<DroneBlock>();
            hasHitDroneBlock = hitDroneBlock != null;
            
            if (hasHitDroneBlock)
            {
                buildingBlockIndicator.SetActive(true);
                UpdateIndicatorPosition(hit);
                
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
    
    void UpdateIndicatorPosition(RaycastHit hit)
    {
        buildingBlockIndicator.transform.position = hit.collider.transform.position + hit.normal * 1f;

        Vector3 forwardVector = Vector3.forward;
        float dot = Vector3.Dot(forwardVector, hit.normal);
        if (dot == 1 || dot == -1)
            forwardVector = Vector3.up;
        
        buildingBlockIndicator.transform.rotation = Quaternion.LookRotation(forwardVector,hit.normal)
                                                    * Quaternion.Euler(new Vector3(0,rotationAngles[indicatorRotDirIndex],0));
    }

    void EnterBuildMode(bool loadSessionSave = false)
    {
        GameManager.Instance.EnterBuildMode();
        
        List<DroneController> activeDrones = FindObjectsOfType<DroneController>().ToList();

        foreach (var drone in activeDrones)
        {
            Destroy(drone.gameObject);
        }
        activeDrones.Clear();
        
        if (loadSessionSave)
        {
            LoadAndSpawnMachine(-1);// corresponds to active game save slot
        }
        else
        {
            Placeable core = new Placeable(BlockLibraryManager.Instance.coreBlock);
            core.Spawn(spawnPoint, quaternion.identity);
            FindDroneController();
        }
        
        SetNewCurrentBlock(new Placeable(BlockLibraryManager.Instance.blocks[0]));
        
    }

    void LoadAndSpawnMachine(int slot)
    {
        Utils.DestroyAllDrones();
        MachineSaveLoadManager.MachineSaveData machineSaveData = MachineSaveLoadManager.Instance.LoadMachine(slot);
        MachineSaveLoadManager.Instance.SpawnMachine(machineSaveData);
        FindDroneController();
    }
    
    void ExitBuildMode()
    {
        MachineSaveLoadManager.Instance.SaveMachine(-1);
        GameManager.Instance.ExitBuildMode();
        buildingBlockIndicator.SetActive(false);
        DeployMachine();
    }

    void DeployMachine()
    {
        droneController.Deploy(true);
    }

    public void SetNewCurrentBlock(Placeable placeable)
    {
        curPlaceable = placeable;
        
        if(buildingBlockIndicator != null)
            Destroy(buildingBlockIndicator);

        if (curPlaceable.isSubAssembly)// spawn sub assembly
        {
            buildingBlockIndicator = curPlaceable.Spawn(Vector3.zero, quaternion.identity);
        }
        else
        {
            buildingBlockIndicator = Instantiate(placeable.block.prefab);
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
        
        List<Renderer> rends = buildingBlockIndicator.GetComponentsInChildren<Renderer>().ToList();
        foreach (Renderer rend in rends)
        {

            rend.material = indicatorMat;

        }
        
        List<DroneBlock> childBlocks = buildingBlockIndicator.GetComponentsInChildren<DroneBlock>().ToList();
        foreach (DroneBlock childBlock in childBlocks)
        {
            Destroy(childBlock);
        }

        // get the one on the parent as well as the children
        DroneBlock block = buildingBlockIndicator.GetComponent<DroneBlock>();
        if(block != null)
            Destroy(block);
    }



    void OnGUI()
    {
        
        return;
        
        if(GameManager.Instance.currentGameMode != GameMode.Build)
            return;
        
        List<BlockData> blocks = BlockLibraryManager.Instance.placeableBlocks;

        
        if (blocks.Count > 0)
        {
            string curSelectedBlockName = "Sub Assembly";
            if (curPlaceable != null)
                curSelectedBlockName = curPlaceable.PlaceableName();
            GUI.Label(new Rect(Screen.width - 200, 10, 190, 30), "Selected Block: " +curSelectedBlockName);
            for (int i = 0; i < blocks.Count; i++)
            {
                if (GUI.Button(new Rect(Screen.width - 200, 50 + i * 30, 190, 30), blocks[i].prefab.name))
                {
                    SetNewCurrentBlock(new Placeable(blocks[i]));
                }
            }
            
            //TODO STICK WITH ONGUI FOR NOW. UPON CLICKING THE SUB ASSEMBLY BUTTON OPEN UP A MENU FULL OF ALL THE SUB ASSEMBLIES 
            if (GUI.Button(new Rect(Screen.width - 200, 50 + blocks.Count * 30, 190, 30), "Sub Assembly"))
            {
                SetNewCurrentBlock(new Placeable(null, MachineSaveLoadManager.instance.LoadSubAssembly(0)));
            }
        }
    }
}
