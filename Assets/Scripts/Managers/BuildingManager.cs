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

        public Sprite thumbnail;

        public Placeable(BlockData block, MachineSaveLoadManager.SubAssemblySaveData subAssembly = null)
        {
            isSubAssembly = block == null;
            this.block = block;
            this.subAssembly = subAssembly;
            thumbnail = GenerateThumbnail();
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
        
        public float Cost()
        {
            if (isSubAssembly)
            {
                return 0;
            }
            else
            {
                return block.prefab.GetComponent<DroneBlock>().cost;
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

        Sprite GenerateThumbnail()
        {
            Vector3 posOffset = Vector3.down *1000;
            float boundsPadding = 0.1f;
            
            
            Camera thumbnailCamera = ThumbnailGenerator.instance.cam;

            GameObject obj = Spawn(posOffset, Quaternion.Euler(-35,35,0));
            Bounds bounds = Utils.CalculateBounds(obj);
            thumbnailCamera.transform.position = bounds.center + Vector3.back * 10; // Adjust distance
            thumbnailCamera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y) +boundsPadding;

            RenderTexture renderTexture = thumbnailCamera.targetTexture;
            RenderTexture.active = renderTexture;
            thumbnailCamera.Render();
            
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            texture = Utils.MakeColorTransparent(texture, Color.white);

            
            RenderTexture.active = null;
            
            obj.SetActive(false);
            Destroy(obj);
            
            Sprite thumbnailSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return thumbnailSprite;
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

        Vector3 forward = Vector3.Cross(hit.normal, Vector3.up);
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.Cross(hit.normal, Vector3.right);
        }

        Quaternion alignToNormal = Quaternion.LookRotation(forward, hit.normal);

        float localYRotation = rotationAngles[indicatorRotDirIndex];
        Quaternion localRotation = Quaternion.Euler(0, localYRotation, 0);

        buildingBlockIndicator.transform.rotation = alignToNormal * localRotation;
    }

    
    /*void UpdateIndicatorPosition(RaycastHit hit)
    {
        buildingBlockIndicator.transform.position = hit.collider.transform.position + hit.normal * 1f;

        Vector3 forwardVector = Vector3.forward;
        float dot = Vector3.Dot(forwardVector, hit.normal);
        if (dot == 1 || dot == -1)
            forwardVector = Vector3.up;
        
        buildingBlockIndicator.transform.rotation = Quaternion.LookRotation(forwardVector,hit.normal)
                                                    * Quaternion.Euler(new Vector3(0,rotationAngles[indicatorRotDirIndex],0));
    }*/

    void EnterBuildMode(bool loadSessionSave = false)
    {
        
        
        GameManager.Instance.EnterBuildMode();
        //AllPlaceables();
        
        Utils.DestroyAllDrones();
        MachineSaveLoadManager.instance.LoadAndSpawnMachine(MachineSaveLoadManager.instance.curSlot);// corresponds to active game save slot;

        SetNewCurrentBlock(new Placeable(BlockLibraryManager.Instance.blocks[0]));
        
    }
    
    public void SpawnDefaultMachine()
    {
        Placeable core = new Placeable(BlockLibraryManager.Instance.coreBlock);
        core.Spawn(spawnPoint, quaternion.identity);
    }
    
    void ExitBuildMode()
    {
        
        //MachineSaveLoadManager.instance.LoadAndSpawnMachine(MachineSaveLoadManager.instance.curSlot, true);
        MachineSaveLoadManager.Instance.SaveMachine(MachineSaveLoadManager.instance.curSlot);
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
        if (allPlaceables != null)
        {
            // Set initial position for displaying thumbnails
            float startX = 10; // Starting X position
            float startY = 10; // Starting Y position
            float thumbnailSize = 100; // Size of each thumbnail
            float padding = 10; // Space between thumbnails

            foreach (var placeable in allPlaceables)
            {
                Sprite thumbnail = placeable.thumbnail;

                if (thumbnail != null)
                {
                    // Convert Sprite to Texture for GUI
                    Texture2D texture = thumbnail.texture;

                    // Draw thumbnail on the GUI
                    GUI.DrawTexture(new Rect(startX, startY, thumbnailSize, thumbnailSize), texture);

                    // Update X position for next thumbnail
                    startX += thumbnailSize + padding;

                    // If it exceeds the screen width, move to the next row
                    if (startX + thumbnailSize > Screen.width)
                    {
                        startX = 10;
                        startY += thumbnailSize + padding;
                    }
                }
            }
        }
    }

}
