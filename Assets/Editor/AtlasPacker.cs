using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AtlasPacker : EditorWindow
{
    int blockSize = 16; // In pixels
    int atlasSizeInBlocks = 16;
    int atlasSize;

    Object[] rawTextures = new Object[256];
    List<Texture2D> sortedTextures = new List<Texture2D>();
    Texture2D atlas;

    [MenuItem ("My Craft/Atlas Packer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AtlasPacker));
    }

    private void OnGUI()
    {
        atlasSize = blockSize * atlasSizeInBlocks;
        GUILayout.Label("Texture Atlas Packer", EditorStyles.boldLabel);
        blockSize = EditorGUILayout.IntField("Block Size", blockSize);
        atlasSizeInBlocks = EditorGUILayout.IntField("Atlas Size In Blocks", atlasSizeInBlocks);
        GUILayout.Label(atlas); 
        if(GUILayout.Button("Load Textures"))
        {
            LoadTextures();
            PackAtlas();
            Debug.Log("Atlas Packer: Textures loaded.");
        }

        if(GUILayout.Button("Clear Textures"))
        {
            atlas = new Texture2D(atlasSize, atlasSize);
            Debug.Log("Atlas Packer: Textures cleared.");
        }
        if(GUILayout.Button("Save Atlas"))
        {
            byte[] bytes = atlas.EncodeToPNG();
            try
            {
                File.WriteAllBytes(Application.dataPath + "/Textures/Packed_Atlas.png",bytes);
                Debug.Log("Atles Packer: Saved Succesfully.");

            }
            catch(System.Exception e)
            {
                Debug.Log("Atles Packer: Coudn't save atlas to file.");
                Debug.Log("Atles Packer: Error Message: " + e.Message);
            }
        }
    }

    void LoadTextures()
    {
        sortedTextures.Clear();
        rawTextures = Resources.LoadAll("AtlasPacker", typeof(Texture2D));
        int index = 0;
        foreach (Object tex in rawTextures)
        {
            Texture2D t = (Texture2D)tex;
            if (t.width == blockSize && t.height == blockSize)
            {
                sortedTextures.Add(t);

            }
            else
                Debug.Log("Asset Packer: '" + tex.name + "' incorrect size. Texture not loaded.");
            index++;
        }
        Debug.Log("Atlas Packer:" + sortedTextures.Count + " succusfully loaded.");
    }

    void PackAtlas()
    {
        atlas = new Texture2D(atlasSize, atlasSize);
        Color[] pixels = new Color[atlasSize * atlasSize];

        for (int x = 0; x < atlasSize; x++)
        {
            for (int y = 0; y < atlasSize; y++)
            {
                //Get current block
                int currentBlockX = x / blockSize;
                int currentBlockY = y / blockSize;

                int index = currentBlockY * atlasSizeInBlocks + currentBlockX;

                //Get pixel position for individual texture
                int currentPixelX = x % blockSize;
                int currentPixelY = y % blockSize;

                if (index < sortedTextures.Count)
                    pixels[(atlasSize - y - 1) * atlasSize + x] = sortedTextures[index].GetPixel(currentPixelX, blockSize - currentPixelY - 1);
                else
                    pixels[(atlasSize - y - 1) * atlasSize + x] = new Color(0f, 0f, 0f, 0f);
            }
        }
        atlas.SetPixels(pixels);
        atlas.Apply();
    }

}
