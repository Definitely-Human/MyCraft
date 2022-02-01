using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes: ScriptableObject {
    public string biomeName;

    [Header("Noise Settings")]
    public float offset;
    public float scale;

    public int terrainHeight;
    public float terrainScale;

    public short surfaceBlock;
    public short subsurfaceBlock;

    [Header("Major Flora")]
    public int majorFloraIndex;
    public float MajorFloraZoneScale = 1.3f;
    [Range(0.1f,1f)]
    public float MajorFloraZoneThereshold = 0.6f;
    public float MajorFloraPlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float MajorFloraPlacementThereshold = 0.8f;
    public bool placeMajorFlora = true;

    public int maxSize = 12;
    public int minSize = 5;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string lodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}