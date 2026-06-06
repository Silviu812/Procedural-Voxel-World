using UnityEngine;

public class BaseAlgo : MonoBehaviour
{
    public int seed = 0;

    [Header("Plains")]
    public float plainScale = 0.04f;
    public int plainHeightMultiplier = 6;
    public int baseHeight = 8;

    [Header("Mountains")]
    public float mountainMaskScale = 0.015f;
    public float mountainNoiseScale = 0.08f;
    public int mountainHeightMultiplier = 35;
    public float mountainThreshold = 0.65f;

    public int MaxHeight => baseHeight + plainHeightMultiplier + mountainHeightMultiplier;

    public int GetSurfaceHeight(int x, int z)
    {
        float plainNoise = Mathf.PerlinNoise(
            (x + seed) * plainScale,
            (z + seed) * plainScale
        );

        int plainHeight = Mathf.FloorToInt(plainNoise * plainHeightMultiplier) + baseHeight;

        float mountainMask = Mathf.PerlinNoise(
            (x + seed + 1000) * mountainMaskScale,
            (z + seed + 1000) * mountainMaskScale
        );

        int mountainExtraHeight = 0;

        if (mountainMask > mountainThreshold)
        {
            float mountainNoise = Mathf.PerlinNoise(
                (x + seed + 2000) * mountainNoiseScale,
                (z + seed + 2000) * mountainNoiseScale
            );

            float mountainStrength = Mathf.InverseLerp(
                mountainThreshold,
                1f,
                mountainMask
            );

            mountainExtraHeight = Mathf.FloorToInt(
                mountainNoise * mountainHeightMultiplier * mountainStrength
            );
        }

        return plainHeight + mountainExtraHeight;
    }
    public int CheckMountain (int x, int z)
    {
        float mountainMask = Mathf.PerlinNoise(
            (x + seed + 1000) * mountainMaskScale,
            (z + seed + 1000) * mountainMaskScale
        );

        if (mountainMask > mountainThreshold)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}