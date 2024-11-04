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
    
    //List<GameObject> blocks;
    GameObject buildingBlockIndicator;

    [HideInInspector]
    public bool isBuilding = true;
    
    int curSelectedBlockIndex;

    int indicatorRotDirIndex;

    Vector3[] indicatorRotDirs = { Vector3.up, Vector3.right, Vector3.forward };

    DroneController droneController;


    public Vector3 spawnPoint;
    
    BlockData curBlock;
    
    void Start()
    {
        droneController = SpawnBlock(BlockLibraryManager.Instance.coreBlock, spawnPoint, quaternion.identity).GetComponent<DroneController>();
        
        SetNewCurrentBlock(BlockLibraryManager.Instance.blocks[0]);
    }

    void Update()
    {
        if(isBuilding)
            BuildUpdate();

    }

    public void FindDroneController()
    {
        droneController = FindObjectOfType<DroneController>();
    }
    
    void BuildUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetKeyDown(KeyCode.R))
        {
            indicatorRotDirIndex = (int)Mathf.Repeat(indicatorRotDirIndex + 1, 3);
        }
        
        if(Input.GetButtonDown("Fire1") && buildingBlockIndicator.activeSelf)
            SpawnBlock(curBlock);
        
        if(Input.GetKeyDown(KeyCode.Space))
            ExitBuildMode();

        if (Physics.Raycast(ray, out hit))
        {
            DroneBlock droneBlock = hit.collider.GetComponent<DroneBlock>();

            if (droneBlock != null)
            {
                buildingBlockIndicator.SetActive(true);
                UpdateIndicatorPosition(hit);
                
                // delete block functionality
                if(Input.GetKeyDown(KeyCode.X))
                    Destroy(droneBlock.gameObject);
            }
            else
            {
                buildingBlockIndicator.SetActive(false);
            }
        }
        else
        {
            buildingBlockIndicator.SetActive(false);
        }
    }

    void UpdateIndicatorPosition(RaycastHit hit)
    {
        buildingBlockIndicator.transform.position = hit.collider.transform.position + hit.normal * 1f;
        buildingBlockIndicator.transform.rotation = Quaternion.LookRotation(hit.normal, indicatorRotDirs[indicatorRotDirIndex]);
    }

    void ExitBuildMode()
    {
        buildingBlockIndicator.SetActive(false);
        isBuilding = false;
        DeployMachine();
    }

    void DeployMachine()
    {
        droneController.gameObject.GetComponent<DroneBlock>().Init();
        droneController.Deploy(true);
    }

    GameObject SpawnBlock(BlockData block)
    {
        return SpawnBlock(block, buildingBlockIndicator.transform.position, buildingBlockIndicator.transform.rotation);
    }
    
    GameObject SpawnBlock(BlockData block, Vector3 pos, quaternion rot)
    {
        GameObject blockClone = Instantiate( block.prefab, pos, rot);
        // give the block its correct identity so the saver knows what block this block is
        blockClone.GetComponent<DroneBlock>().blockIdentity = block;
        return blockClone;
    }

    void SetNewCurrentBlock(BlockData currentBlock)
    {
        curBlock = currentBlock;
        
        if(buildingBlockIndicator != null)
            Destroy(buildingBlockIndicator);

        buildingBlockIndicator = Instantiate(currentBlock.prefab);

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
    }

    void OnGUI()
    {
        List<BlockData> blocks = BlockLibraryManager.Instance.placeableBlocks;
        

        
        if (blocks.Count > 0)
        {
            GUI.Label(new Rect(Screen.width - 200, 10, 190, 30), "Selected Block: " + curBlock.prefab.name);
            for (int i = 0; i < blocks.Count; i++)
            {
                if (GUI.Button(new Rect(Screen.width - 200, 50 + i * 30, 190, 30), blocks[i].prefab.name))
                {
                    SetNewCurrentBlock(blocks[i]);
                    curSelectedBlockIndex = i;
                }
            }
        }
    }
}
