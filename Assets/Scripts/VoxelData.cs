using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{

    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 256;
    public static readonly int WorldWidthInChunks = 24;

    public static int ViewDistanceInChunks = 6;

    public static int WorldWidthInVoxels
    {
        get { return WorldWidthInChunks * ChunkWidth; }
    }

    public static int WorldHeightInVoxels
    {
        get { return ChunkHeight; }
    }

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalizeBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] voxelVerts = new Vector3[8] {
        new Vector3(0.0f,0.0f,0.0f),
        new Vector3(1.0f,0.0f,0.0f),
        new Vector3(1.0f,1.0f,0.0f),
        new Vector3(0.0f,1.0f,0.0f),
        new Vector3(0.0f,0.0f,1.0f),
        new Vector3(1.0f,0.0f,1.0f),
        new Vector3(1.0f,1.0f,1.0f),
        new Vector3(0.0f,1.0f,1.0f),

    };

    public static readonly int[,] voxelTris = new int[6, 4]{

        {0,3,1,2 }, // Back face
        {5,6,4,7 }, // Front face
        {3,7,2,6 }, // Top face
        {1,5,0,4 }, // Bottom face
        {4,7,0,3 }, // Left face
        {1,2,5,6 }, // Right face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2(0.0f,0.0f),
        new Vector2(0.0f,1.0f),
        new Vector2(1.0f,0.0f),
        new Vector2(1.0f,1.0f),
    };

    public static readonly Vector3Int[] faceChecks = new Vector3Int[6]
    {
        new Vector3Int (0, 0, -1),
        new Vector3Int (0, 0, 1),
        new Vector3Int (0, 1, 0),
        new Vector3Int (0, -1, 0),
        new Vector3Int (-1, 0, 0),
        new Vector3Int (1, 0, 0),
    };
}
