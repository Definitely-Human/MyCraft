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

    public int noiseId;
    public float rarity;

    [Header("Blocks Settings")]
    public int[] layerLength;
    public short[] layerBlock;
    public int allLayerLength;

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

    public short getIdFromDepth(int depth)
    {
        for(int i = 0;i < layerLength.Length;i++ )
        {
            if ((depth - layerLength[i]) >= 0)
                depth -= layerLength[i];
            else
                return layerBlock[i];
        }
        return 0;
    }

    private void OnEnable()
    {
        allLayerLength = 0;
        foreach(int layer in layerLength)
            allLayerLength += layer;
    }
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