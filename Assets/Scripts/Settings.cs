using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Settings 
{
    [Header("Game Data")]
    public string version;

    [Header("Performance")]
    public int ViewDistanceInChunks;
    public bool enableThreading;
    public bool enableAnimatedChunks;


    [Header("Controls")]
    [Range(0.3f,10f)]
    public float mouseSensitivity;

    [Header("World Gen")]
    public int worldSeed;
}
