using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise{

    /*public static float get2DNoiseFromId()
    {
        return 1f;
    }*/

    public static float Get2DPerlinOct(Vector2 position, float offset, float scale, int octaves = 1,float persistance = 1, float lacunacrity = 1)
    {
        if (scale <= 0) scale = 0.00001f;
        float noise = 0;

        float amplitude = 1;
        float frequency = 1;
        for(int i = 0;i< octaves; i++)
        {
            float sampleX = (position.x + 0.1f) / VoxelData.ChunkWidth * scale * frequency + offset;
            float sampleY = (position.y + 0.1f) / VoxelData.ChunkWidth * scale * frequency + offset;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            noise =+ perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunacrity;
        }

        noise = Mathf.InverseLerp(-1, 1, noise);
        return noise;

    }

    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f)/VoxelData.ChunkWidth * scale + offset, (position.y + 0.1f) / VoxelData.ChunkWidth * scale + offset);
    }

    public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
    {
        float x = (position.x + offset + 0.1f) *scale;
        float y = (position.y + offset + 0.1f) *scale;
        float z = (position.z + offset + 0.1f) *scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        float normalizedNoise = Mathf.InverseLerp(0.35f,0.65f,(AB + BC + AC + BA + CB + CA) / 6);

        if (normalizedNoise > threshold)
            return true;
        else
            return false;


    }
}
