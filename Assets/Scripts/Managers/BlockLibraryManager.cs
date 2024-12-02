using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityUtils;

public class BlockLibraryManager : Singleton<BlockLibraryManager>
{

    [SerializeField]
    BlockLibraryScriptableObject library;

    
    public List<BlockData> placeableBlocks;
    public List<BlockData> blocks;

    public BlockData coreBlock;
    
    new void Awake()
    {
        base.Awake();

        blocks = library.blocks.ToList();
        
        placeableBlocks = new List<BlockData>();

        foreach (BlockData block in blocks)
        {
            if(!block.isCore)
                placeableBlocks.Add(block);
        }

        coreBlock = CoreBlock();
    }
    
    BlockData CoreBlock()
    {
        foreach (BlockData block in blocks)
        {
            if(block.isCore)
                return block;
        }

        Debug.Log("NO CORE BLOCK IN LIBRARY");
        return null;
    }

    public List<BlockData> PlaceablesInCategory(BuildingManagerUI.PlaceableCategories targetCategory)
    {
        List<BlockData> targetPlaceables = new List<BlockData>();

        foreach (var placeable in placeableBlocks)
        {
            if (placeable.category == targetCategory)
            {
                targetPlaceables.Add(placeable);
            }
        }

        return targetPlaceables;
    }
}
