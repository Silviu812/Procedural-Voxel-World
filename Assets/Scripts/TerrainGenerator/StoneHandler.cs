using UnityEngine;

public class StoneHandler : MonoBehaviour
{
    [Range(0.1f , 20f)]
    public float coalChance = 5f;
    [Range(0.1f, 20f)]
    public float ironChance = 1f;
    [Range(0.1f, 20f)]
    public float goldChance = 0.5f;
    [Range(0.1f, 10f)]
    public float diamondChance = 0.1f;
    [Range(0.1f, 10f)]
    public float emeraldChance = 0.1f;

    private float stoneChance => 100f - coalChance - ironChance - goldChance - diamondChance - emeraldChance;

    public VoxelType StoneHandlerSpawner()
    {
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < coalChance)
            return VoxelType.Coal; // Coal

        if (randomValue < coalChance + ironChance)
            return VoxelType.Iron; // Iron

        if (randomValue < coalChance + ironChance + goldChance)
            return VoxelType.Gold; // Gold

        if (randomValue < coalChance + ironChance + goldChance + diamondChance)
            return VoxelType.Diamond; // Diamond

        if (randomValue < coalChance + ironChance + goldChance + diamondChance + emeraldChance)
            return VoxelType.Emerald; // Emerald
        else
            return VoxelType.Stone; // Stone
    }
}