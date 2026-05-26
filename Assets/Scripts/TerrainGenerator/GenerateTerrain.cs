using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    [Header("Height Settings")]
    public int baseHeight = 10;
    public int heightAmplitude = 90;

    [Header("Noise Settings")]
    public float frequency = 0.01f;
    public int octaves = 6;
    public float persistence = 0.55f;
    public float lacunarity = 2f;

    [Header("Seed")]
    public int seed = 12345;

    private Vector2 seedOffset;

    private void Awake()
    {
        System.Random random = new System.Random(seed);

        seedOffset = new Vector2(
            random.Next(-100000, 100000),
            random.Next(-100000, 100000)
        );
    }

    public void GenerateChunk(ChunkVirtual chunk)
    {
        for (int x = 0; x < chunk.chunkSizeX; x++)
        {
            for (int z = 0; z < chunk.chunkSizeZ; z++)
            {
                int worldX = chunk.worldPosition.x + x;
                int worldZ = chunk.worldPosition.z + z;

                float noise = OctavePerlin(worldX, worldZ);

                int surfaceHeight = baseHeight + Mathf.FloorToInt(noise * heightAmplitude);

                for (int y = 0; y < chunk.chunkSizeY; y++)
                {
                    if (y > surfaceHeight)
                    {
                        chunk.SetVoxel(x, y, z, VoxelType.Air);
                    }
                    else if (y == surfaceHeight)
                    {
                        chunk.SetVoxel(x, y, z, VoxelType.Grass);
                    }
                    else if (y >= surfaceHeight - 4)
                    {
                        chunk.SetVoxel(x, y, z, VoxelType.Dirt);
                    }
                    else
                    {
                        chunk.SetVoxel(x, y, z, VoxelType.Stone);
                    }
                }
            }
        }
    }

    private float OctavePerlin(float x, float z)
    {
        float total = 0f;

        float amplitude = 1f;
        float currentFrequency = frequency;

        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x * currentFrequency + seedOffset.x;
            float sampleZ = z * currentFrequency + seedOffset.y;

            float noise = Mathf.PerlinNoise(sampleX, sampleZ);

            total += noise * amplitude;
            maxValue += amplitude;

            amplitude *= persistence;
            currentFrequency *= lacunarity;
        }

        return total / maxValue;
    }
}