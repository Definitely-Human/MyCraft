using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public bool renderNeighborFaces;
    public float transparency;
    public Sprite icon;

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
