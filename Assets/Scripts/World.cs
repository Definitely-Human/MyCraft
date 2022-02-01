using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class World : MonoBehaviour
{
    public Settings settings;
    public Transform player;
    public BiomeAttributes[] biomes;

    [Header("World Data")]
    public Vector3 spawnPosition;

    [Header("Textures")]
    public Material material;
    public Material transparentMaterial;
    public Block[] blockTypes;

    [Header("UI")]
    public GameObject debugScreen;
    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;

    private ChunkCoord playerLastChunkCoord;
    public ChunkCoord playerChunkCoord;


    private Chunk[,] chunks = new Chunk[VoxelData.WorldWidthInChunks, VoxelData.WorldWidthInChunks];

    private List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    private List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
    private bool isCreatingChunks;

    private Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    private Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();

    private bool applyingModifications = false;

    private string settingsPath = "/Settings/settings.cfg";

    private bool _inUI;

    public bool InUI {
        get => _inUI;
        set { 
            _inUI = value;
            HandleInventory();

        }
    }

    

    

    private void Start()
    {
        Random.InitState(settings.worldSeed);

        //string jsonExport = JsonUtility.ToJson(settings);
        //Debug.Log(jsonExport);
        //File.WriteAllText(Application.dataPath + "/Settings/settings.cfg", jsonExport);
        string jsonImport = File.ReadAllText(Application.dataPath + settingsPath);
        settings = JsonUtility.FromJson<Settings>(jsonImport);
        
        spawnPosition = new Vector3(VoxelData.WorldWidthInVoxels / 2, 96, VoxelData.WorldWidthInVoxels / 2);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoorFromVector3(player.position);

        
    }

    private void OnEnable()
    {
        if (settings.enableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start();
        }
    }

    private void OnDisable()
    {
        if (settings.enableThreading)
            ChunkUpdateThread.Abort();
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoorFromVector3(player.position);

        //Only update chunks if the player has moved from the chunk they were previously on
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (!settings.enableThreading)
        {
            if (modifications.Count > 0 && !applyingModifications)
            {
                ApplyModifications();
            }
            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if(chunksToDraw.Count > 0)

            {
                if (chunksToDraw.Peek().IsEditable)
                {
                    chunksToDraw.Dequeue().CreateMesh();
                }
            }

        

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    private void ThreadedUpdate()
    {
        while (true)
        {
            if (modifications.Count > 0 && !applyingModifications)
            {
                ApplyModifications();
            }
            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }
    }

    

    private void GenerateWorld()
    {
        for(int x = (VoxelData.WorldWidthInChunks/2 ) - settings.ViewDistanceInChunks; x < (VoxelData.WorldWidthInChunks / 2) + settings.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldWidthInChunks / 2 ) - settings.ViewDistanceInChunks; z < (VoxelData.WorldWidthInChunks / 2) + settings.ViewDistanceInChunks; z++)
            {
                ChunkCoord coord = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(coord, this);
                chunksToCreate.Add(coord);
            }
        }

        


        player.position = spawnPosition;
        CheckViewDistance();
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

        activeChunks.Clear();

        for (int x = coord.x - settings.ViewDistanceInChunks; x < coord.x + settings.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - settings.ViewDistanceInChunks; z < coord.z + settings.ViewDistanceInChunks; z++)
            {
                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);
                if (IsChunkInWorld(thisChunkCoord))
                {
                    if(chunks[x,z] == null)
                    {
                        chunks[x, z] = new Chunk(thisChunkCoord, this);
                        chunksToCreate.Add(thisChunkCoord);
                    }
                    else if (!chunks[x, z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                    }
                    activeChunks.Add(thisChunkCoord);
                }
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(thisChunkCoord))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }
        foreach(ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.z].IsActive = false;
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

    public bool IsVoxelInWorld(Vector3 pos)
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

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlVect3(_pos).id].isSolid;

        return blockTypes[GetVoxel(_pos)].isSolid;
    }

    public VoxelState GetVoxelState(Vector3 _pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(_pos);

        if (!IsVoxelInWorld(_pos))
            return null;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
            return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlVect3(_pos);

        return new VoxelState(GetVoxel(_pos));
    }

    void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);

        chunks[c.x, c.z].InitChunk();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        lock (ChunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count)
            {
                if (chunksToUpdate[index].IsEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    if(!activeChunks.Contains(chunksToUpdate[index].coord))
                        activeChunks.Add(chunksToUpdate[index].coord);
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    void ApplyModifications()
    {
        applyingModifications = true;
        while(modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();
            if (queue == null)
                Debug.Log("null");
         
            while (queue.Count > 0)
            {

                VoxelMod v = queue.Dequeue();
                ChunkCoord c = GetChunkCoorFromVector3(v.position);

                if (!IsChunkInWorld(c)) continue;
                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);

                

            }
        }

        applyingModifications = false;
    }

    private BiomeAttributes SelectBiome(Vector3 pos,out int terrainHeight)
    {
        int solidGroundHeight = 64;
        float sumOfHeights = 0f;
        int count = 0;
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;
        BiomeAttributes biome;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            float height = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * biomes[i].terrainHeight * weight;

            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }

        }


        biome = biomes[strongestBiomeIndex];
        sumOfHeights /= count;
        terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);
        return biome;
    }

    public short GetVoxel(Vector3 pos)
    {

        int yPos = Mathf.FloorToInt(pos.y);

        // Immutable pass

        if (!IsVoxelInWorld(pos))
            return 0;

        if (yPos == 0)
            return 1;

        // Biome Selection pass
        int terrainHeight = 0;
        BiomeAttributes biome = SelectBiome(pos,out terrainHeight);
        
        // Basic terrain pass

        
        short voxelValue = 0;

        if (yPos > terrainHeight)
        {
            return 0;
        }
        else if (yPos == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = biome.subsurfaceBlock;
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

        if(yPos == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.MajorFloraZoneScale) > biome.MajorFloraZoneThereshold)
            {
                voxelValue = biome.subsurfaceBlock;
                if(Noise.Get2DPerlin(new Vector2(pos.x,pos.z),0, biome.MajorFloraPlacementScale) > biome.MajorFloraPlacementThereshold)
                {
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex,pos, biome.minSize, biome.maxSize));
                }
            }
        }

        return voxelValue;


    }

    private void HandleInventory()
    {
        if (_inUI)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            creativeInventoryWindow.SetActive(true);
            cursorSlot.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            creativeInventoryWindow.SetActive(false);
            cursorSlot.SetActive(false);
        }
    }
}



