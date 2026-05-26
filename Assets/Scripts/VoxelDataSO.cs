using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/Block Data")]
public class VoxelDataSO : ScriptableObject
{
    public int atlasColumns = 9;
    public int atlasRows = 10;

    public List<BlockTextureData> blocks = new List<BlockTextureData>();

    public BlockTextureData GetBlockData(VoxelType voxelType)
    {
        foreach (BlockTextureData block in blocks)
        {
            if (block.voxelType == voxelType)
                return block;
        }
        return null;
    }
}

[Serializable]
public class BlockTextureData
{
    public VoxelType voxelType;

    public Vector2Int top;
    public Vector2Int bottom;
    public Vector2Int side;

    public bool isSolid = true;
}