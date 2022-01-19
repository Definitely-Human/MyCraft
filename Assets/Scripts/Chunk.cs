using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    void Start()
    {
        for(int y = 0; y < VoxelData.ChunkHeight; y++){
            for (int x = 0; x < VoxelData.ChunkHeight; x++)
            {
                for (int z = 0; z < VoxelData.ChunkHeight; z++)
                {

                }
            }
        }
        AddVoxelDataToChunk(transform.position);
        
        
        
        
        CreateMesh();
       

        
    }

    void AddVoxelDataToChunk(Vector3 pos)
    {
        for (int side = 0; side < 6; side++)
        {
            for (int vert = 0; vert < 6; vert++)
            {
                int triangleIndex = VoxelData.voxelTris[side, vert];
                vertices.Add(VoxelData.voxelVerts[triangleIndex] + pos);
                triangles.Add(vertexIndex);

                uvs.Add(VoxelData.voxelUvs[vert]);

                vertexIndex++;
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();


        meshFilter.mesh = mesh;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
