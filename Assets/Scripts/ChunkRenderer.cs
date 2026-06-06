using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    public VoxelDataSO voxelData;
    public World world;

    public ChunkVirtual Chunk { get; private set; }

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh visualMesh;
    private Mesh colliderMesh;

    private void Awake()
    {
        EnsureComponents();
        EnsureMeshes();
    }

    public void RenderChunk(ChunkVirtual chunk)
    {
        if (chunk == null || voxelData == null)
            return;

        Chunk = chunk;

        EnsureComponents();
        EnsureMeshes();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        ChunkMeshBuilder.BuildMesh(
            chunk,
            vertices,
            triangles,
            uvs,
            voxelData,
            true,
            world
        );

        visualMesh.Clear();
        visualMesh.SetVertices(vertices);
        visualMesh.SetTriangles(triangles, 0);
        visualMesh.SetUVs(0, uvs);

        visualMesh.RecalculateNormals();
        visualMesh.RecalculateBounds();

        meshFilter.sharedMesh = visualMesh;

        List<Vector3> colliderVertices = new List<Vector3>();
        List<int> colliderTriangles = new List<int>();
        List<Vector2> colliderUvs = new List<Vector2>();

        ChunkMeshBuilder.BuildMesh(
            chunk,
            colliderVertices,
            colliderTriangles,
            colliderUvs,
            voxelData,
            false,
            world
        );

        colliderMesh.Clear();
        colliderMesh.SetVertices(colliderVertices);
        colliderMesh.SetTriangles(colliderTriangles, 0);
        colliderMesh.SetUVs(0, colliderUvs);

        colliderMesh.RecalculateNormals();
        colliderMesh.RecalculateBounds();

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = colliderMesh;
    }

    private void EnsureComponents()
    {
        if (!TryGetComponent(out meshFilter))
            meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer;

        if (!TryGetComponent(out meshRenderer))
            gameObject.AddComponent<MeshRenderer>();

        if (!TryGetComponent(out meshCollider))
            meshCollider = gameObject.AddComponent<MeshCollider>();
    }

    private void EnsureMeshes()
    {
        if (visualMesh == null)
        {
            visualMesh = new Mesh();
            visualMesh.name = "Chunk Visual Mesh";
            visualMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        if (colliderMesh == null)
        {
            colliderMesh = new Mesh();
            colliderMesh.name = "Chunk Collider Mesh";
            colliderMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
    }

    private void OnDestroy()
    {
        if (visualMesh != null)
            Destroy(visualMesh);

        if (colliderMesh != null)
            Destroy(colliderMesh);
    }
}