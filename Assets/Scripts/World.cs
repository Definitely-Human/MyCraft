using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{

    public Transform player;
    public BiomeAttributes biome;

    [Header("World Data")]
    public Vector3 spawnPosition;
    public int seed;

    [Header("Textures")]
    public Material material;
    public Material transparentMaterial;
    public BlockType[] blockTypes;

    public GameObject debugScreen;

    private ChunkCoord playerLastChunkCoord;
    public ChunkCoord playerChunkCoord;


    private Chunk[,] chunks = new Chunk[VoxelData.WorldWidthInChunks, VoxelData.WorldWidthInChunks];

    private List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    private List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    private List<Chunk> chunksToUpdate = new List<Chunk>();
    private bool isCreatingChunks;

    private Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    private bool applyingModifications = false;

    private bool _inUI;

    public bool InUI {
        get => _inUI;
        set { 
            _inUI = value;
        }
    }

    private void Start()
    {
        Random.InitState(seed);
        spawnPosition = new Vector3(VoxelData.WorldWidthInVoxels / 2, 96, VoxelData.WorldWidthInVoxels / 2);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoorFromVector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoorFromVector3(player.position);

        //Only update chunks if the player has moved from the chunk they were previously on
        if(!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();



        if (modifications.Count > 0 && !applyingModifications)
        {
            StartCoroutine(ApplyModifications());   
        }

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if (chunksToUpdate.Count > 0)
            UpdateChunks(); 

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    private void GenerateWorld()
    {
        for(int x = (VoxelData.WorldWidthInChunks/2 ) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldWidthInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldWidthInChunks / 2 ) -VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldWidthInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }

        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            ChunkCoord c = GetChunkCoorFromVector3(v.position);

            if (!IsChunkInWorld(c)) continue;
            if (chunks[c.x,c.z] == null)
            {
                chunks[c.x, c.z] = new Chunk(c, this, true);
                activeChunks.Add(c);
            }

            chunks[c.x, c.z].modifications.Enqueue(v);

            if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
            {
                chunksToUpdate.Add(chunks[c.x, c.z]);
            }
        }

        while(chunksToUpdate.Count > 0)
        {
            chunksToUpdate[0].UpdateChunk();
            chunksToUpdate.RemoveAt(0);
        }


        player.position = spawnPosition;
    }


    ChunkCoord GetChunkCoorFromVector3(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(_pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(_pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];
    }

    private void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoorFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    if(chunks[x,z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!chunks[x, z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                    }
                    activeChunks.Add(new ChunkCoord(x, z));
                }
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }
        foreach(ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.z].IsActive = false;
            activeChunks.Remove(c);
        }
    }

   

    private bool IsChunkInWorld(ChunkCoord coord)
    {
        if(coord.x >= 0 && coord.x <= VoxelData.WorldWidthInChunks-1 && coord.z >= 0 && coord.z <= VoxelData.WorldWidthInChunks - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsVoxelInWorld(Vector3 pos)
    {
        if(pos.x >= 0 && pos.x < VoxelData.WorldWidthInVoxels && pos.y >= 0 && pos.y < VoxelData.WorldHeightInVoxels && pos.z >= 0 && pos.z < VoxelData.WorldWidthInVoxels)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckForVoxel(Vector3 _pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(_pos);

        if (!IsVoxelInWorld(_pos))
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlVect3(_pos)].isSolid;

        return blockTypes[GetVoxel(_pos)].isSolid;
    }

    public bool CheckIfVoxelTransparent(Vector3 _pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(_pos);

        if (!IsVoxelInWorld(_pos))
            return true;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlVect3(_pos)].isTransparent;

        return blockTypes[GetVoxel(_pos)].isTransparent;
    }

    void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunks[c.x, c.z].InitChunk();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;
        while (!updated && index < chunksToUpdate.Count)
        {
            if (chunksToUpdate[index].isVoxelMapPopulated)
            {
                chunksToUpdate[index].UpdateChunk();
                chunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else
            {
                index++;
            }
        }
    }

    IEnumerator ApplyModifications()
    {
        applyingModifications = true;
        int count = 0;
        while(modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            ChunkCoord c = GetChunkCoorFromVector3(v.position);

            if (!IsChunkInWorld(c)) continue;
            if (chunks[c.x, c.z] == null)
            {
                chunks[c.x, c.z] = new Chunk(c, this, true);
                activeChunks.Add(c);
            }

            chunks[c.x, c.z].modifications.Enqueue(v);

            if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
            {
                chunksToUpdate.Add(chunks[c.x, c.z]);
            }

            count++;
            if(count > 200)
            {
                count = 0;
                yield return null;
            }
        }

        applyingModifications = false;
    }

    public short GetVoxel(Vector3 pos)
    {

        int yPos = Mathf.FloorToInt(pos.y);

        // Immutable pass

        if (!IsVoxelInWorld(pos))
            return 0;

        if (yPos == 0)
            return 1;

        // Basic terrain pass

        int terrainHeight = Mathf.FloorToInt(Noise.Get2DPerling(new Vector2(pos.x,pos.z),0, biome.terrainScale) * biome.terrainHeight + biome.solidGroundHeight);
        short voxelValue = 0;

        if (yPos > terrainHeight)
        {
            return 0;
        }
        else if (yPos == terrainHeight)
        {
            voxelValue = 3;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = 4;
        }
        else
            voxelValue = 2;

        // Second pass

        if(voxelValue == 2){
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }

        // Tree Pass

        if(yPos == terrainHeight)
        {
            if (Noise.Get2DPerling(new Vector2(pos.x, pos.z), 0, biome.treeZoneScale) > biome.treeZoneThereshold)
            {
                voxelValue = 4;
                if(Noise.Get2DPerling(new Vector2(pos.x,pos.z),0,biome.treePlacementScale)> biome.treePlacementThereshold)
                {
                    Structure.MakeTree(pos, modifications, biome.minTreeSize, biome.maxTreeSize);
                }
            }
        }

        return voxelValue;


    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public Sprite icon;
    public bool isTransparent;

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

public class VoxelMod
{
    public Vector3 position;
    public short id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 _position, short _id)
    {
        position = _position;
        id = _id;
    }
}
