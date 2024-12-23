using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockLibrary", menuName = "ScriptableObjects/BlockLibrary", order = 1)]
public class BlockLibraryScriptableObject : ScriptableObject
{

    public List<BlockData> blocks;

}