using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Block", menuName = "Minecraft/Block", order = 0)]
public class Block : ScriptableObject
{
    [Header("Game Data")]
    public string blockName;
    public Sprite icon;

    [Header("Render Settings")]
    public bool isSolid;
    public bool renderNeighborFaces;
    public float transparency;

    [Header("Texture Values")]
    public short backfaceTexture;
    public short frontfaceTexture;
    public short topfaceTexture;
    public short bottomfaceTexture;
    public short leftfaceTexture;
    public short rightfaceTexture;
    // Back, Front, Top, Bottom, Left, Right

    public short GetTextureId(short faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backfaceTexture;
            case 1:
                return frontfaceTexture;
            case 2:
                return topfaceTexture;
            case 3:
                return bottomfaceTexture;
            case 4:
                return leftfaceTexture;
            case 5:
                return rightfaceTexture;
            default:
                Debug.Log("BlockType.GetTextureId: (Invalid face Index.)");
                return 0;
        }
    }

}
