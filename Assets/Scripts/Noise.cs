using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise{

    /*public static float get2DNoiseFromId()
    {
        return 1f;
    }*/

    private static Vector2 Offset2D(Vector2 position,float offset)
    {
        position.x += (offset + VoxelData.seed + 0.1f);
        position.y += (offset + VoxelData.seed + 0.1f);
        return position;
    }

    public static float Get2DPerlinOct(Vector2 position, float offset, float scale, int octaves = 1,float persistance = 1, float lacunacrity = 1)
    {
        if (scale <= 0) scale = 0.00001f;


        position = Offset2D(position,offset);
        float noise = 0;

        float amplitude = 1;
        float frequency = 1;
        for(int i = 0;i< octaves; i++)
        {
            float sampleX = (position.x) / VoxelData.ChunkWidth * scale * frequency;
            float sampleY = (position.y) / VoxelData.ChunkWidth * scale * frequency;

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
        if (scale <= 0) scale = 0.00001f;
        position = Offset2D(position, offset); 
        return Mathf.PerlinNoise(position.x / VoxelData.ChunkWidth * scale, position.y / VoxelData.ChunkWidth * scale);
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
