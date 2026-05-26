using System.Collections.Generic;
using UnityEngine;

public static class ChunkMeshBuilder
{
    public static void BuildMesh(
        ChunkVirtual chunk,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs,
        VoxelDataSO blockData)
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

                    Vector3Int voxelPosInt = new Vector3Int(x, y, z);
                    Vector3 voxelPos = new Vector3(x, y, z);

                    for (int face = 0; face < 6; face++)
                    {
                        Vector3Int neighbour =
                            voxelPosInt + VoxelData.faceChecks[face];

                        bool neighbourIsAir =
                            !chunk.IsInside(neighbour.x, neighbour.y, neighbour.z) ||
                            chunk.GetVoxel(neighbour.x, neighbour.y, neighbour.z) == VoxelType.Air;

                        if (!neighbourIsAir)
                            continue;

                        Vector2Int tile = GetTileForFace(voxelType, face, blockData);
                        Vector2[] faceUvs = GetFaceUVs(tile, blockData.atlasColumns, blockData.atlasRows);

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

    private static Vector2Int GetTileForFace(VoxelType voxelType, int face, VoxelDataSO blockData)
    {
        BlockTextureData data = blockData.GetBlockData(voxelType);

        if (face == 2) // Top
            return data.top;

        if (face == 3) // Bottom
            return data.bottom;

        return data.side;
    }

    private static Vector2[] GetFaceUVs(Vector2Int tilePosition, int atlasColumns, int atlasRows)
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