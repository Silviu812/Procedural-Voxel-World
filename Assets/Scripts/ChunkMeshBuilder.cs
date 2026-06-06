using System.Collections.Generic;
using UnityEngine;

public static class ChunkMeshBuilder
{
    public static void BuildMesh(
        ChunkVirtual chunk,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs,
        VoxelDataSO blockData,
        bool includeWater)
    {
        BuildMesh(
            chunk,
            vertices,
            triangles,
            uvs,
            blockData,
            includeWater,
            null
        );
    }

    public static void BuildMesh(
        ChunkVirtual chunk,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs,
        VoxelDataSO blockData,
        bool includeWater,
        World world)
    {
        for (int x = 0; x < chunk.chunkSizeX; x++)
        {
            for (int y = 0; y < chunk.chunkSizeY; y++)
            {
                for (int z = 0; z < chunk.chunkSizeZ; z++)
                {
                    VoxelType voxelType = chunk.GetVoxel(x, y, z);

                    if (voxelType == VoxelType.Air)
                        continue;

                    if (!includeWater && voxelType == VoxelType.Water)
                        continue;

                    Vector3Int voxelPosInt = new Vector3Int(x, y, z);
                    Vector3 voxelPos = new Vector3(x, y, z);

                    for (int face = 0; face < 6; face++)
                    {
                        Vector3Int neighbour =
                            voxelPosInt + VoxelData.faceChecks[face];

                        bool shouldDrawFace = ShouldDrawFace(
                            chunk,
                            neighbour,
                            voxelType,
                            includeWater,
                            world
                        );

                        if (!shouldDrawFace)
                            continue;

                        Vector2Int tile =
                            GetTileForFace(voxelType, face, blockData);

                        Vector2[] faceUvs = GetFaceUVs(
                            tile,
                            blockData.atlasColumns,
                            blockData.atlasRows
                        );

                        for (int i = 0; i < 6; i++)
                        {
                            int triIndex = VoxelData.voxelTris[face, i];

                            Vector3 vertex =
                                voxelPos + VoxelData.voxelVerts[triIndex];

                            vertices.Add(vertex);
                            triangles.Add(vertices.Count - 1);
                            uvs.Add(faceUvs[i]);
                        }
                    }
                }
            }
        }
    }

    private static bool ShouldDrawFace(
        ChunkVirtual chunk,
        Vector3Int neighbour,
        VoxelType currentType,
        bool includeWater,
        World world)
    {
        VoxelType neighbourType;

        if (chunk.IsInside(neighbour.x, neighbour.y, neighbour.z))
        {
            neighbourType = chunk.GetVoxel(
                neighbour.x,
                neighbour.y,
                neighbour.z
            );
        }
        else
        {
            if (world == null)
                return true;

            Vector3Int neighbourWorldPosition =
                chunk.worldPosition + neighbour;

            if (!world.TryGetVoxelType(neighbourWorldPosition, out neighbourType))
                return true;
        }

        if (neighbourType == VoxelType.Air)
            return true;

        if (!includeWater && neighbourType == VoxelType.Water)
            return true;

        if (includeWater)
        {
            if (currentType != VoxelType.Water &&
                neighbourType == VoxelType.Water)
            {
                return true;
            }

            if (currentType == VoxelType.Water &&
                neighbourType != VoxelType.Water)
            {
                return true;
            }
        }

        return false;
    }

    private static Vector2Int GetTileForFace(
        VoxelType voxelType,
        int face,
        VoxelDataSO blockData)
    {
        BlockTextureData data = blockData.GetBlockData(voxelType);

        if (face == 2)
            return data.top;

        if (face == 3)
            return data.bottom;

        return data.side;
    }

    private static Vector2[] GetFaceUVs(
        Vector2Int tilePosition,
        int atlasColumns,
        int atlasRows)
    {
        Vector2[] faceUvs = new Vector2[6];

        float tileWidth = 1f / atlasColumns;
        float tileHeight = 1f / atlasRows;

        float xMin = tilePosition.x * tileWidth;
        float xMax = xMin + tileWidth;

        int unityY = atlasRows - 1 - tilePosition.y;

        float yMin = unityY * tileHeight;
        float yMax = yMin + tileHeight;

        faceUvs[0] = new Vector2(xMin, yMin);
        faceUvs[1] = new Vector2(xMin, yMax);
        faceUvs[2] = new Vector2(xMax, yMin);

        faceUvs[3] = new Vector2(xMax, yMin);
        faceUvs[4] = new Vector2(xMin, yMax);
        faceUvs[5] = new Vector2(xMax, yMax);

        return faceUvs;
    }
}