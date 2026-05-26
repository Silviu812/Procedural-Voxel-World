using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int worldSize = 1;

    public int chunkSizeX = 16;
    public int chunkSizeY = 100;
    public int chunkSizeZ = 16;

    public GameObject chunkPrefab;
    public GenerateTerrain terrainGenerator;

    private Dictionary<Vector3Int, ChunkVirtual> chunks = new Dictionary<Vector3Int, ChunkVirtual>();

    private void Start()
    {
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        chunks.Clear();

        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                Vector3Int chunkWorldPosition = new Vector3Int(
                    x * chunkSizeX,
                    0,
                    z * chunkSizeZ
                );

                ChunkVirtual chunk = new ChunkVirtual(
                    chunkSizeX,
                    chunkSizeY,
                    chunkSizeZ,
                    chunkWorldPosition
                );

                terrainGenerator.GenerateChunk(chunk);

                chunks.Add(chunkWorldPosition, chunk);

                GameObject chunkObject = Instantiate(
                    chunkPrefab,
                    chunkWorldPosition,
                    Quaternion.identity
                );

                ChunkRenderer chunkRenderer = chunkObject.GetComponent<ChunkRenderer>();
                chunkRenderer.RenderChunk(chunk);
            }
        }

        Debug.Log("World generated with " + chunks.Count + " chunks.");
    }
}