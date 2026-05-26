using UnityEngine;

public class ChunkVirtual
{
    public int chunkSizeX;
    public int chunkSizeY;
    public int chunkSizeZ;

    public VoxelType[,,] voxels;

    public Vector3Int worldPosition;

    public ChunkVirtual(int sizeX, int sizeY, int sizeZ, Vector3Int worldPosition)
    {
        this.chunkSizeX = sizeX;
        this.chunkSizeY = sizeY;
        this.chunkSizeZ = sizeZ;
        this.worldPosition = worldPosition;

        voxels = new VoxelType[chunkSizeX, chunkSizeY, chunkSizeZ];
    }

    public VoxelType GetVoxel(int x, int y, int z)
    {
        return voxels[x, y, z];
    }

    public void SetVoxel(int x, int y, int z, VoxelType type)
    {
        voxels[x, y, z] = type;
    }
    public bool IsInside(int x, int y, int z)
    {
        return x >= 0 && x < chunkSizeX &&
               y >= 0 && y < chunkSizeY &&
               z >= 0 && z < chunkSizeZ;
    }
}