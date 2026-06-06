using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    [Tooltip("The component that calculates the surface height.")]
    public BaseAlgo baseAlgo;

    [Header("Water And Beach")]
    [Tooltip("All empty blocks at or below this height become water.")]
    [Min(0)]
    public int waterLevel = 23;

    [Tooltip("Ground this many blocks above water is sand instead of grass.")]
    [Range(0, 8)]
    public int beachHeight = 1;

    [Header("Ground Layers")]
    [Tooltip("Number of dirt or sand blocks below the visible surface.")]
    [Range(1, 12)]
    public int soilDepth = 4;

    public TreeSpawner treeSpawner;
    public StoneHandler stoneHandler;

    public void GenerateChunk(ChunkVirtual chunk)
    {
        if (baseAlgo == null)
        {
            Debug.LogError("GenerateTerrain needs a BaseAlgo reference in the Inspector.");
            return;
        }

        for (int x = 0; x < chunk.chunkSizeX; x++)
        {
            for (int z = 0; z < chunk.chunkSizeZ; z++)
            {
                int worldX = chunk.worldPosition.x + x;
                int worldZ = chunk.worldPosition.z + z;
                int surfaceHeight = baseAlgo.GetSurfaceHeight(worldX, worldZ);
                surfaceHeight = Mathf.Clamp(surfaceHeight, 0, chunk.chunkSizeY - 1);

                int isMountain = baseAlgo.CheckMountain(worldX, worldZ);

                if (isMountain > 0)
                {
                    for (int y = 0; y < chunk.chunkSizeY; y++)
                    {
                        chunk.SetVoxel(x, y, z, GetVoxelMountain(y, surfaceHeight));
                    }
                }
                else
                {
                    for (int y = 0; y < chunk.chunkSizeY; y++)
                    {
                        chunk.SetVoxel(x, y, z, GetVoxel(y, surfaceHeight));
                    }
                }
            }
        }
        treeSpawner.SpawnTrees(chunk);
    }

    private VoxelType GetVoxel(int y, int surfaceHeight)
    {
        if (y == 0) return VoxelType.Bedrock;
        if (y > surfaceHeight)
            return y <= waterLevel ? VoxelType.Water : VoxelType.Air;

        bool isBeach = surfaceHeight <= waterLevel + beachHeight;

        if (y == surfaceHeight)
            return isBeach ? VoxelType.Sand : VoxelType.Grass;

        int depthBelowSurface = surfaceHeight - y;
        if (depthBelowSurface <= soilDepth)
            return isBeach ? VoxelType.Sand : VoxelType.Dirt;

        return stoneHandler.StoneHandlerSpawner();
    }
    private VoxelType GetVoxelMountain(int y, int surfaceHeight)
    {
        if (y == 0) return VoxelType.Bedrock;
        if (y > surfaceHeight)
            return y <= waterLevel ? VoxelType.Water : VoxelType.Air;

        bool isBeach = surfaceHeight <= waterLevel + beachHeight;

        if (y == surfaceHeight)
            return isBeach ? VoxelType.Sand : stoneHandler.StoneHandlerSpawner();

        int depthBelowSurface = surfaceHeight - y;
        if (depthBelowSurface <= soilDepth)
            return isBeach ? VoxelType.Sand : stoneHandler.StoneHandlerSpawner();

        if (y > baseAlgo.MaxHeight - 20)
            return VoxelType.Snow;

        return stoneHandler.StoneHandlerSpawner();
    }
}