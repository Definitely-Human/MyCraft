using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk 
{
    public ChunkCoord coord;
    public short[,,] voxelMap = new short[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public bool isVoxelMapPopulated = false;

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    public Vector3 globalChunkPos;

    private GameObject chunkObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    private int vertexIndex = 0;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<int> transparentTriangles = new List<int>();
    private Material[] materials = new Material[2];
    private List<Vector2> uvs = new List<Vector2>();

    private World world;

    private bool isActive;
    

    public bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public Chunk(ChunkCoord _coord,World _world,bool generateOnLoad)
    {
        coord = _coord;
        world = _world;
        IsActive = true;

        if (generateOnLoad)
            InitChunk();
    }

    

    public void InitChunk()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk " + coord.x + " " + coord.z;
        globalChunkPos = chunkObject.transform.position;

        PopulateVoxelMap();
        UpdateChunk();
    }

    Vector3Int VectFToI(Vector3 v)
    {
        return new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
    }

    bool IsVoxelInChunk(Vector3Int pos) {
        if( pos.x < 0 || pos.x >= VoxelData.ChunkWidth || pos.y < 0 || pos.y >= VoxelData.ChunkHeight || pos.z < 0 || pos.z >= VoxelData.ChunkWidth)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    bool CheckVoxel(Vector3 posF)
    {
        Vector3Int pos = VectFToI(posF);
        if (!IsVoxelInChunk(pos))
            return world.CheckIfVoxelTransparent(pos + globalChunkPos);
        return world.blockTypes[voxelMap[pos.x, pos.y, pos.z]].isTransparent;
    }
    public void EditVoxel(Vector3 _pos, short newId)
    {
        Vector3Int pos = Vector3Int.FloorToInt(_pos);
        pos.x -= Mathf.FloorToInt(chunkObject.transform.position.x); // Get IN Chunk position from global position
        pos.z -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[pos.x, pos.y, pos.z] = newId;

        //Update surrounding chunks
        UpdateSurroundingVoxels(pos.x, pos.y, pos.z);

        UpdateChunk();
    }

    void UpdateSurroundingVoxels(int x,int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (byte side = 0; side < 6; side++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[side]; 

            if(!IsVoxelInChunk(new Vector3Int((int)(currentVoxel.x), (int)(currentVoxel.y), (int)(currentVoxel.z)))){
                world.GetChunkFromVector3(currentVoxel + globalChunkPos).UpdateChunk();
            }
        }
    }

    public short GetVoxelFromGlVect3(Vector3 _pos)
    {
        Vector3Int pos = Vector3Int.FloorToInt(_pos);
        pos.x -= Mathf.FloorToInt(globalChunkPos.x); // Get IN Chunk position from global position
        pos.z -= Mathf.FloorToInt(globalChunkPos.z);



        return voxelMap[pos.x, pos.y, pos.z];
    }

    public void UpdateChunk()
    {
        while(modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position -= globalChunkPos;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = v.id;
        }
        ClearMeshData();
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (world.blockTypes[voxelMap[x, y, z]].isSolid)
                    {
                        UpdateMeshVoxel(new Vector3(x, y, z));
                    }
                }
            }
        }
        CreateMesh();
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
    }

   

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x,y,z] = world.GetVoxel(new Vector3(x,y,z)+globalChunkPos);
                }
            }
        }
        isVoxelMapPopulated = true;
    }

    void UpdateMeshVoxel(Vector3 pos)
    {
        short blockId = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        bool isTransparent = world.blockTypes[blockId].isTransparent;
        for (byte side = 0; side < 6; side++)
        {
            if (CheckVoxel(pos + VoxelData.faceChecks[side]) )
            {
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[side,0]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[side,1]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[side,2]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[side,3]] + pos);
                AddTexture(world.blockTypes[blockId].GetTextureId(side));

                if (!isTransparent)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                else
                {
                    transparentTriangles.Add(vertexIndex);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }


                vertexIndex+=4;
                
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);

        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }



    void AddTexture(int textureId)
    {
        float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizeBlockTextureSize;
        y *= VoxelData.NormalizeBlockTextureSize;

        y = 1f - y - VoxelData.NormalizeBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x , y + VoxelData.NormalizeBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizeBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizeBlockTextureSize, y + VoxelData.NormalizeBlockTextureSize));
    }


}

public class ChunkCoord
{
    public int x;
    public int z;

    
    public ChunkCoord(int _x =0, int _z = 0)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 _pos)
    {
        Vector3Int pos = Vector3Int.FloorToInt(_pos);
        x = pos.x / VoxelData.ChunkWidth;
        z = pos.z / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;
    }
}