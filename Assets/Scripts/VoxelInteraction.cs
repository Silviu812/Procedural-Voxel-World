using UnityEngine;

public class VoxelInteractor : MonoBehaviour
{
    [System.Serializable]
    public class Drop
    {
        public VoxelType voxelType;
        public ItemSO item;
    }

    public World world;
    public Camera playerCamera;
    public InventoryData inventory;

    [Header("Interaction")]
    [Range(1f, 12f)]
    public float reachDistance = 6f;

    public LayerMask voxelLayers = ~0;

    [Header("Drops")]
    public Drop[] drops;

    private CharacterController playerController;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (inventory == null)
            inventory = GetComponent<InventoryData>();

        if (inventory == null)
            inventory = InventoryData.Instance;

        playerController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (world == null ||
            playerCamera == null ||
            Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
            RemoveBlock();

        if (Input.GetMouseButtonDown(1))
            PlaceBlock();
    }

    private void RemoveBlock()
    {
        RaycastHit hit;

        if (!TryHitChunk(out hit))
            return;

        Vector3Int position = GetVoxelPosition(hit, -1f);

        VoxelType voxelType;
        if (!world.TryGetVoxelType(position, out voxelType))
            return;

        if (voxelType == VoxelType.Water)
            return;
        
        if (voxelType == VoxelType.Bedrock)
            return;

        VoxelType removedType;

        if (!world.RemoveVoxel(position, out removedType))
            return;

        ItemSO item = GetDropItem(removedType);

        if (item != null &&
            inventory != null &&
            !inventory.TryAddOne(item))
        {
            world.PlaceVoxel(position, removedType);
        }
    }

    private void PlaceBlock()
    {
        if (inventory == null)
            return;

        ItemSO item = inventory.GetSelectedItem();

        if (item == null || !item.canPlaceBlock)
            return;

        RaycastHit hit;

        if (!TryHitChunk(out hit))
            return;

        Vector3Int position = GetVoxelPosition(hit, 1f);

        if (OverlapsPlayer(position))
            return;

        if (world.PlaceVoxel(position, item.blockToPlace))
        {
            ItemSO removedItem;
            inventory.TryRemoveOneFromSelectedSlot(out removedItem);
        }
    }

    private bool TryHitChunk(out RaycastHit hit)
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (!Physics.Raycast(
            ray,
            out hit,
            reachDistance,
            voxelLayers,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        ChunkRenderer renderer =
            hit.collider.GetComponentInParent<ChunkRenderer>();

        return renderer != null && renderer.Chunk != null;
    }

    private Vector3Int GetVoxelPosition(RaycastHit hit, float direction)
    {
        Vector3 point = hit.point + hit.normal * direction * 0.01f;
        return Vector3Int.FloorToInt(point);
    }

    private bool OverlapsPlayer(Vector3Int position)
    {
        if (playerController == null)
            return false;

        Bounds blockBounds = new Bounds(
            (Vector3)position + Vector3.one * 0.5f,
            Vector3.one * 0.98f
        );

        return playerController.bounds.Intersects(blockBounds);
    }

    private ItemSO GetDropItem(VoxelType voxelType)
    {
        if (drops == null)
            return null;

        for (int i = 0; i < drops.Length; i++)
        {
            if (drops[i] != null && drops[i].voxelType == voxelType)
                return drops[i].item;
        }

        return null;
    }
}