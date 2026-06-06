using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Min(0)]
    public int renderDistance = 3;

    [Min(0.1f)]
    public float chunkCheckInterval = 0.5f;

    [Header("Slow Chunk Loading")]
    [Min(1)]
    public int chunksPerFrame = 1;

    [Header("Chunk Size")]
    public int chunkSizeX = 16;
    public int chunkSizeY = 100;
    public int chunkSizeZ = 16;

    [Header("References")]
    public GameObject chunkPrefab;
    public GenerateTerrain terrainGenerator;

    private Dictionary<Vector3Int, ChunkVirtual> chunks =
        new Dictionary<Vector3Int, ChunkVirtual>();

    private Dictionary<Vector3Int, ChunkRenderer> renderers =
        new Dictionary<Vector3Int, ChunkRenderer>();

    private Queue<Vector3Int> chunkLoadQueue =
        new Queue<Vector3Int>();

    private HashSet<Vector3Int> queuedChunks =
        new HashSet<Vector3Int>();

    private Vector3Int lastPlayerChunkPosition;
    private Coroutine chunkCheckCoroutine;
    private Coroutine chunkLoadCoroutine;
    private bool worldGenerated;

    public void GenerateWorld()
    {
        if (player == null || chunkPrefab == null || terrainGenerator == null)
        {
            Debug.LogError("World needs Player, Chunk Prefab and Terrain Generator references.");
            return;
        }

        if (chunkCheckCoroutine != null)
            StopCoroutine(chunkCheckCoroutine);

        if (chunkLoadCoroutine != null)
            StopCoroutine(chunkLoadCoroutine);

        ClearRenderedChunks();

        chunks.Clear();
        renderers.Clear();
        chunkLoadQueue.Clear();
        queuedChunks.Clear();

        lastPlayerChunkPosition = GetChunkWorldPosition(player.position);
        worldGenerated = true;

        QueueChunksAround(lastPlayerChunkPosition);
        StartChunkLoading();

        chunkCheckCoroutine = StartCoroutine(CheckPlayerChunkRoutine());
    }

    private IEnumerator CheckPlayerChunkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(chunkCheckInterval);

            if (!worldGenerated || player == null)
                continue;

            Vector3Int currentPlayerChunkPosition =
                GetChunkWorldPosition(player.position);

            if (currentPlayerChunkPosition == lastPlayerChunkPosition)
                continue;

            lastPlayerChunkPosition = currentPlayerChunkPosition;

            QueueChunksAround(currentPlayerChunkPosition);
            UnloadFarChunkRenderers(currentPlayerChunkPosition);
            StartChunkLoading();
        }
    }

    private void StartChunkLoading()
    {
        if (chunkLoadCoroutine == null)
            chunkLoadCoroutine = StartCoroutine(ProcessChunkLoadQueue());
    }

    private IEnumerator ProcessChunkLoadQueue()
    {
        while (chunkLoadQueue.Count > 0)
        {
            Vector3Int centerChunkPosition =
                GetChunkWorldPosition(player.position);

            int chunksLoadedThisFrame = 0;

            while (chunksLoadedThisFrame < chunksPerFrame &&
                   chunkLoadQueue.Count > 0)
            {
                Vector3Int chunkWorldPosition = chunkLoadQueue.Dequeue();
                queuedChunks.Remove(chunkWorldPosition);

                if (IsOutsideRenderDistance(chunkWorldPosition, centerChunkPosition))
                    continue;

                EnsureChunkData(chunkWorldPosition);
                EnsureChunkRenderer(chunkWorldPosition);
                RefreshNeighbourChunks(chunkWorldPosition);

                chunksLoadedThisFrame++;
            }

            yield return null;
        }

        chunkLoadCoroutine = null;
    }

    private Vector3Int GetChunkWorldPosition(Vector3 worldPosition)
    {
        int chunkX =
            Mathf.FloorToInt(worldPosition.x / chunkSizeX) * chunkSizeX;

        int chunkZ =
            Mathf.FloorToInt(worldPosition.z / chunkSizeZ) * chunkSizeZ;

        return new Vector3Int(chunkX, 0, chunkZ);
    }

    private void QueueChunksAround(Vector3Int centerChunkPosition)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector3Int chunkWorldPosition = new Vector3Int(
                    centerChunkPosition.x + x * chunkSizeX,
                    0,
                    centerChunkPosition.z + z * chunkSizeZ
                );

                if (renderers.ContainsKey(chunkWorldPosition))
                    continue;

                if (queuedChunks.Contains(chunkWorldPosition))
                    continue;

                positions.Add(chunkWorldPosition);
            }
        }

        positions.Sort((a, b) =>
        {
            int distanceA = GetChunkDistance(a, centerChunkPosition);
            int distanceB = GetChunkDistance(b, centerChunkPosition);

            return distanceA.CompareTo(distanceB);
        });

        for (int i = 0; i < positions.Count; i++)
        {
            chunkLoadQueue.Enqueue(positions[i]);
            queuedChunks.Add(positions[i]);
        }
    }

    private int GetChunkDistance(
        Vector3Int chunkPosition,
        Vector3Int centerChunkPosition)
    {
        int dx = Mathf.Abs(chunkPosition.x - centerChunkPosition.x) / chunkSizeX;
        int dz = Mathf.Abs(chunkPosition.z - centerChunkPosition.z) / chunkSizeZ;

        return dx * dx + dz * dz;
    }

    private void EnsureChunkData(Vector3Int chunkWorldPosition)
    {
        if (chunks.ContainsKey(chunkWorldPosition))
            return;

        ChunkVirtual chunk = new ChunkVirtual(
            chunkSizeX,
            chunkSizeY,
            chunkSizeZ,
            chunkWorldPosition
        );

        terrainGenerator.GenerateChunk(chunk);
        chunks.Add(chunkWorldPosition, chunk);
    }

    private void EnsureChunkRenderer(Vector3Int chunkWorldPosition)
    {
        ChunkVirtual chunk;

        if (!chunks.TryGetValue(chunkWorldPosition, out chunk))
            return;

        ChunkRenderer chunkRenderer;

        if (renderers.TryGetValue(chunkWorldPosition, out chunkRenderer))
        {
            if (chunkRenderer != null)
            {
                chunkRenderer.world = this;
                chunkRenderer.RenderChunk(chunk);
                return;
            }

            renderers.Remove(chunkWorldPosition);
        }

        GameObject chunkObject = Instantiate(
            chunkPrefab,
            chunkWorldPosition,
            Quaternion.identity
        );

        chunkRenderer = chunkObject.GetComponent<ChunkRenderer>();

        if (chunkRenderer == null)
            chunkRenderer = chunkObject.AddComponent<ChunkRenderer>();

        chunkRenderer.world = this;

        renderers.Add(chunkWorldPosition, chunkRenderer);
        chunkRenderer.RenderChunk(chunk);
    }

    private void UnloadFarChunkRenderers(Vector3Int centerChunkPosition)
    {
        List<Vector3Int> chunksToUnload = new List<Vector3Int>();

        foreach (KeyValuePair<Vector3Int, ChunkRenderer> pair in renderers)
        {
            if (pair.Value == null ||
                IsOutsideRenderDistance(pair.Key, centerChunkPosition))
            {
                chunksToUnload.Add(pair.Key);
            }
        }

        for (int i = 0; i < chunksToUnload.Count; i++)
        {
            Vector3Int chunkPosition = chunksToUnload[i];

            ChunkRenderer renderer;

            if (renderers.TryGetValue(chunkPosition, out renderer) &&
                renderer != null)
            {
                Destroy(renderer.gameObject);
            }

            renderers.Remove(chunkPosition);
        }
    }

    private bool IsOutsideRenderDistance(
        Vector3Int chunkPosition,
        Vector3Int centerChunkPosition)
    {
        int distanceX =
            Mathf.Abs(chunkPosition.x - centerChunkPosition.x) / chunkSizeX;

        int distanceZ =
            Mathf.Abs(chunkPosition.z - centerChunkPosition.z) / chunkSizeZ;

        return distanceX > renderDistance || distanceZ > renderDistance;
    }

    public bool RemoveVoxel(
        Vector3Int voxelWorldPosition,
        out VoxelType removedType)
    {
        removedType = VoxelType.Air;

        ChunkVirtual chunk;
        Vector3Int localPosition;

        if (!TryGetVoxel(voxelWorldPosition, out chunk, out localPosition))
            return false;

        removedType = chunk.GetVoxel(
            localPosition.x,
            localPosition.y,
            localPosition.z
        );

        if (removedType == VoxelType.Air)
            return false;

        chunk.SetVoxel(
            localPosition.x,
            localPosition.y,
            localPosition.z,
            VoxelType.Air
        );

        RefreshEditedChunk(chunk, localPosition);
        return true;
    }

    public bool PlaceVoxel(Vector3Int voxelWorldPosition, VoxelType type)
    {
        if (type == VoxelType.Air)
            return false;

        ChunkVirtual chunk;
        Vector3Int localPosition;

        if (!TryGetVoxel(voxelWorldPosition, out chunk, out localPosition))
            return false;

        VoxelType existingType = chunk.GetVoxel(
            localPosition.x,
            localPosition.y,
            localPosition.z
        );

        if (existingType != VoxelType.Air &&
            existingType != VoxelType.Water)
        {
            return false;
        }

        chunk.SetVoxel(
            localPosition.x,
            localPosition.y,
            localPosition.z,
            type
        );

        RefreshEditedChunk(chunk, localPosition);
        return true;
    }

    private bool TryGetVoxel(
        Vector3Int voxelWorldPosition,
        out ChunkVirtual chunk,
        out Vector3Int localPosition)
    {
        int chunkX =
            Mathf.FloorToInt((float)voxelWorldPosition.x / chunkSizeX) * chunkSizeX;

        int chunkZ =
            Mathf.FloorToInt((float)voxelWorldPosition.z / chunkSizeZ) * chunkSizeZ;

        Vector3Int chunkPosition = new Vector3Int(chunkX, 0, chunkZ);

        if (!chunks.TryGetValue(chunkPosition, out chunk))
        {
            localPosition = Vector3Int.zero;
            return false;
        }

        localPosition = voxelWorldPosition - chunkPosition;

        return chunk.IsInside(
            localPosition.x,
            localPosition.y,
            localPosition.z
        );
    }

    public bool TryGetVoxelType(Vector3Int voxelWorldPosition, out VoxelType type)
    {
        type = VoxelType.Air;

        ChunkVirtual chunk;
        Vector3Int localPosition;

        if (!TryGetVoxel(voxelWorldPosition, out chunk, out localPosition))
            return false;

        type = chunk.GetVoxel(
            localPosition.x,
            localPosition.y,
            localPosition.z
        );

        return true;
    }

    private void RefreshEditedChunk(
        ChunkVirtual chunk,
        Vector3Int localPosition)
    {
        RefreshChunk(chunk.worldPosition);

        if (localPosition.x == 0)
            RefreshChunk(chunk.worldPosition + Vector3Int.left * chunkSizeX);

        if (localPosition.x == chunkSizeX - 1)
            RefreshChunk(chunk.worldPosition + Vector3Int.right * chunkSizeX);

        if (localPosition.z == 0)
            RefreshChunk(chunk.worldPosition + new Vector3Int(0, 0, -chunkSizeZ));

        if (localPosition.z == chunkSizeZ - 1)
            RefreshChunk(chunk.worldPosition + new Vector3Int(0, 0, chunkSizeZ));
    }

    private void RefreshNeighbourChunks(Vector3Int chunkPosition)
    {
        RefreshChunk(chunkPosition + Vector3Int.left * chunkSizeX);
        RefreshChunk(chunkPosition + Vector3Int.right * chunkSizeX);
        RefreshChunk(chunkPosition + new Vector3Int(0, 0, -chunkSizeZ));
        RefreshChunk(chunkPosition + new Vector3Int(0, 0, chunkSizeZ));
    }

    private void RefreshChunk(Vector3Int chunkPosition)
    {
        ChunkVirtual chunk;
        ChunkRenderer renderer;

        if (chunks.TryGetValue(chunkPosition, out chunk) &&
            renderers.TryGetValue(chunkPosition, out renderer) &&
            renderer != null)
        {
            renderer.world = this;
            renderer.RenderChunk(chunk);
        }
    }

    private void ClearRenderedChunks()
    {
        foreach (ChunkRenderer renderer in renderers.Values)
        {
            if (renderer != null)
                Destroy(renderer.gameObject);
        }

        renderers.Clear();
    }

    private void OnDestroy()
    {
        if (chunkCheckCoroutine != null)
            StopCoroutine(chunkCheckCoroutine);

        if (chunkLoadCoroutine != null)
            StopCoroutine(chunkLoadCoroutine);
    }
}