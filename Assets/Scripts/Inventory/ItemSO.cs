using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStackSize = 64;
    public bool canPlaceBlock;
    public VoxelType blockToPlace = VoxelType.Dirt;
}