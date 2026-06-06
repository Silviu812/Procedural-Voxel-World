using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public BaseAlgo baseAlgo;

    public void SpawnTrees(ChunkVirtual chunk)
    {
        for (int x = 3; x < chunk.chunkSizeX - 3; x++)
        {
            for (int z = 3; z < chunk.chunkSizeZ - 3; z++)
            {
                int worldX = chunk.worldPosition.x + x;
                int worldZ = chunk.worldPosition.z + z;
                int surfaceHeight = baseAlgo.GetSurfaceHeight(worldX, worldZ);
                surfaceHeight = Mathf.Clamp(surfaceHeight, 0, chunk.chunkSizeY - 1);

                chunk.GetVoxel(x, surfaceHeight, z);
                if (chunk.GetVoxel(x, surfaceHeight, z) == VoxelType.Grass)
                {
                    
                    if (Random.value < 0.01f)
                    {
                        chunk.SetVoxel(x, surfaceHeight + 1, z, VoxelType.Wood);
                        chunk.SetVoxel(x, surfaceHeight + 2, z, VoxelType.Wood);
                        chunk.SetVoxel(x, surfaceHeight + 3, z, VoxelType.Wood);
                        chunk.SetVoxel(x, surfaceHeight + 4, z, VoxelType.Leaves);
                        chunk.SetVoxel(x+1, surfaceHeight + 4, z, VoxelType.Leaves);
                        chunk.SetVoxel(x-1, surfaceHeight + 4, z, VoxelType.Leaves);
                        chunk.SetVoxel(x, surfaceHeight + 4, z+1, VoxelType.Leaves);
                        chunk.SetVoxel(x, surfaceHeight + 4, z-1, VoxelType.Leaves);
                        chunk.SetVoxel(x+1, surfaceHeight + 4, z+1, VoxelType.Leaves);
                        chunk.SetVoxel(x-1, surfaceHeight + 4, z-1, VoxelType.Leaves);
                        chunk.SetVoxel(x+1, surfaceHeight + 4, z-1, VoxelType.Leaves);
                        chunk.SetVoxel(x-1, surfaceHeight + 4, z+1, VoxelType.Leaves);
                        chunk.SetVoxel(x+1, surfaceHeight + 3, z, VoxelType.Leaves);
                        chunk.SetVoxel(x-1, surfaceHeight + 3, z, VoxelType.Leaves);
                        chunk.SetVoxel(x, surfaceHeight + 3, z+1, VoxelType.Leaves);
                        chunk.SetVoxel(x, surfaceHeight + 3, z-1, VoxelType.Leaves);
                        chunk.SetVoxel(x+1, surfaceHeight + 3, z+1, VoxelType.Leaves);
                        chunk.SetVoxel(x-1, surfaceHeight + 3, z-1, VoxelType.Leaves);
                        chunk.SetVoxel(x+1, surfaceHeight + 3, z-1, VoxelType.Leaves);
                        chunk.SetVoxel(x-1, surfaceHeight + 3, z+1, VoxelType.Leaves);
                        chunk.SetVoxel(x, surfaceHeight + 5, z, VoxelType.Leaves);
                    }
                }
            }
        }
    }
}