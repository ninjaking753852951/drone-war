using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;
using UnityUtils;

public class BlockLibraryManager : Singleton<BlockLibraryManager>
{

    [SerializeField]
    //BlockLibraryScriptableObject library;

    public List<BlockLibraryScriptableObject> libraries;
    
    public List<IPlaceable> placeableBlocks;
    public List<IPlaceable> blocks;

    
    
    public BlockData coreBlock;

    public TankTrackBuilder tankTrack;

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
        placeableBlocks = new List<IPlaceable>();

        foreach (IPlaceable block in blocks)
        {
            placeableBlocks.Add(block);
        }

        coreBlock = CoreBlock();
    }

    List<IPlaceable> CompileLibraries()
    {
        List<IPlaceable> blocks = new List<IPlaceable>();
        
        foreach (BlockLibraryScriptableObject library in libraries)
        {
            foreach (BlockData block in library.blocks)
            {
                blocks.Add(block);
            }
        }
        
        blocks.Add(tankTrack);
        
        Debug.Log(blocks.Count);
        
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

    public List<IPlaceable> PlaceablesInCategory(BlockType targetCategory)
    {
        List<IPlaceable> targetPlaceables = new List<IPlaceable>();

        foreach (var placeable in placeableBlocks)
        {
            if (placeable.Category() == targetCategory)
            {
                targetPlaceables.Add(placeable);
            }
        }

        return targetPlaceables;
    }

    public IPlaceable BlockData(int id)
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
