using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes: ScriptableObject {
    public string biomeName;

    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;

    [Header("Trees")]
    public float treeZoneScale = 1.3f;
    [Range(0.1f,1f)]
    public float treeZoneThereshold = 0.6f;
    public float treePlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float treePlacementThereshold = 0.8f;

    public int maxTreeSize = 12;
    public int minTreeSize = 5;

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