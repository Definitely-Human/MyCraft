using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class World : MonoBehaviour
{
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
    private Queue<ChunkCoord> biomeHeightMapsToFill = new Queue<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
    private bool isCreatingChunks;

    private Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    private Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();

    private bool applyingModifications = false;


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
        Debug.Log(VoxelData.seed);
        Random.InitState(VoxelData.seed);



        SetSpawnPosition();
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoorFromVector3(player.position);

        
    }

    private void OnEnable()
    {
        if (VoxelData.settings.enableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start();
        }
    }

    private void OnDisable()
    {
        if (VoxelData.settings.enableThreading)
            ChunkUpdateThread.Abort();
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoorFromVector3(player.position);

        //Only update chunks if the player has moved from the chunk they were previously on
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (!VoxelData.settings.enableThreading)
        {
            CallUpdates();
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
            CallUpdates();
        }
    }

    

    private void CallUpdates()
    {

        if (biomeHeightMapsToFill.Count > 0)
            FillBiomeHeightMaps();
        if (modifications.Count > 0 && !applyingModifications)
        {
            ApplyModifications();
        }
        if (chunksToUpdate.Count > 0)
            UpdateChunks();
    }

    private void GenerateWorld()
    {
        for(int x = (VoxelData.WorldWidthInChunks/2 ) - VoxelData.settings.ViewDistanceInChunks; x < (VoxelData.WorldWidthInChunks / 2) + VoxelData.settings.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldWidthInChunks / 2 ) - VoxelData.settings.ViewDistanceInChunks; z < (VoxelData.WorldWidthInChunks / 2) + VoxelData.settings.ViewDistanceInChunks; z++)
            {
                ChunkCoord coord = new ChunkCoord(x, z);
                AddToChunksToCreate(coord);
            }
        }

        


        player.position = spawnPosition;
        CheckViewDistance();
    }

    private void SetSpawnPosition()
    {
        Vector3 worldCentre = new Vector3(VoxelData.WorldCentre, VoxelData.ChunkHeight/2, VoxelData.WorldCentre);
        int height;
        SelectBiome(worldCentre, out height);
        spawnPosition = new Vector3(worldCentre.x,height+1,worldCentre.z);
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

        for (int x = coord.x - VoxelData.settings.ViewDistanceInChunks; x < coord.x + VoxelData.settings.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.settings.ViewDistanceInChunks; z < coord.z + VoxelData.settings.ViewDistanceInChunks; z++)
            {
                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);
                if (IsChunkInWorld(thisChunkCoord))
                {
                    if(chunks[x,z] == null)
                    {
                        AddToChunksToCreate(thisChunkCoord);
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

    private void FillBiomeHeightMaps()
    {
        while (biomeHeightMapsToFill.Count > 0)
        {
            ChunkCoord c = biomeHeightMapsToFill.Dequeue(); //Add lock to biomeHeightMaps
            chunks[c.x, c.z].FillBiomeAndHeight();
        }
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
                continue;
         
            while (queue.Count > 0)
            {

                VoxelMod v = queue.Dequeue();
                ChunkCoord c = GetChunkCoorFromVector3(v.position);

                if (!IsChunkInWorld(c)) continue;
                if (chunks[c.x, c.z] == null)
                {
                    AddToChunksToCreate(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);

                // Add chunks to UpdateChunks list

            }
        }

        applyingModifications = false;
    }

    private void AddToChunksToCreate(ChunkCoord c)
    {
        Chunk thisChunk = new Chunk(c, this);
        chunks[c.x, c.z] = thisChunk;

        biomeHeightMapsToFill.Enqueue(c);
        chunksToCreate.Add(c);
    }

    public BiomeAttributes SelectBiome(Vector3 pos,out int terrainHeight)
    {
        int solidGroundHeight = 64;
        float sumOfHeights = 0f;
        //int count = 0; 
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;
        BiomeAttributes biome;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale) * biomes[i].rarity;

            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            

        }
        for (int i = 0; i < biomes.Length; i++)
        {

            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale) * biomes[i].rarity;
            float heightNoise = Noise.Get2DPerlinOct(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale, 3, 0.5f, 2f) ;
            //float height = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * biomes[i].terrainHeight * weight;
            
            sumOfHeights += Mathf.Lerp(-biomes[i].terrainHeight/2, biomes[i].terrainHeight/2, heightNoise * Mathf.Pow(1 - (strongestWeight - weight), 2));


        }


        biome = biomes[strongestBiomeIndex];
        terrainHeight = Mathf.FloorToInt(Mathf.Abs(sumOfHeights) + solidGroundHeight);
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
        Chunk thisChunk = GetChunkFromVector3(pos);
        int terrainHeight;
        BiomeAttributes biome;
        if (thisChunk != null && thisChunk.isBiomeAndHeightPopulated)
        {
            terrainHeight = thisChunk.heightMap[Mathf.FloorToInt(pos.x) % VoxelData.ChunkWidth, Mathf.FloorToInt(pos.z) % VoxelData.ChunkWidth];
            biome = thisChunk.biomeMap[Mathf.FloorToInt(pos.x) % VoxelData.ChunkWidth, Mathf.FloorToInt(pos.z) % VoxelData.ChunkWidth];
        }
        else
        {
            biome = SelectBiome(pos, out terrainHeight);
        }
        // Basic terrain pass

        int depth = terrainHeight - yPos;
        short voxelValue = 0;

        if (depth < 0)
        {
            return 0;
        }
        else if (depth >= biome.allLayerLength)
        {
            voxelValue = 2;
        }
        else
        {
            voxelValue = biome.getIdFromDepth(depth);
        }
            

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
                //voxelValue = biome.subsurfaceBlock;
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



