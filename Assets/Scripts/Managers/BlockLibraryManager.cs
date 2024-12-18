using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityUtils;

public class BlockLibraryManager : Singleton<BlockLibraryManager>
{

    [SerializeField]
    //BlockLibraryScriptableObject library;

    public List<BlockLibraryScriptableObject> libraries;
    
    public List<BlockData> placeableBlocks;
    public List<BlockData> blocks;

    public BlockData coreBlock;

    public BlockAdoptionRules blockRules;
    
    [System.Serializable]
    public class BlockAdoptionRules
    {
        [Header("Block Adoption Matrix")]
        public List<EnumMatrix> combinations = new List<EnumMatrix>();

        // Method to check if a combination is true
        public bool IsCombinationTrue(BlockType first, BlockType second)
        {
            foreach (var combo in combinations)
            {
                if ((combo.parentBlock == first && combo.childBlock == second))// Symmetrical
                {
                    return combo.canPlace;
                }
            }

            // Default to false if not specified
            return false;
        }
        
        [System.Serializable]
        public class EnumMatrix
        {
            public BlockType parentBlock;
            public BlockType childBlock;
            public bool canPlace;
        }
    }
    
    new void Awake()
    {
        base.Awake();

        //blocks = library.blocks.ToList();
        blocks = CompileLibraries();
        
        // maybe convert blockdata (from the library) to a factory so that block datas can be edited at runtime for thumbnails
        placeableBlocks = new List<BlockData>();

        foreach (BlockData block in blocks)
        {
            if(!block.isCore)
                placeableBlocks.Add(block);
        }

        coreBlock = CoreBlock();
    }

    List<BlockData> CompileLibraries()
    {
        List<BlockData> blocks = new List<BlockData>();
        
        foreach (BlockLibraryScriptableObject library in libraries)
        {
            foreach (BlockData block in library.blocks)
            {
                blocks.Add(block);
            }
        }
        return blocks;
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

    public List<BlockData> PlaceablesInCategory(BlockType targetCategory)
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

    public BlockData BlockData(int id)
    {
        if (id < Instance.blocks.Count && id >= 0)
        {
            return blocks[id];
        }
        else
        {
            Debug.LogWarning("Block not found in block library");
            return null;
        }
    }
}
