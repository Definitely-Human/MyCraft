using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Settings 
{
    [Header("Game Data")]
    public string version = "0.0.1";

    [Header("Performance")]
    public int ViewDistanceInChunks = 10;
    public bool enableThreading = true;
    public bool enableAnimatedChunks = false;


    [Header("Controls")]
    [Range(0.3f,10f)]
    public float mouseSensitivity = 3.5f;

    
}
