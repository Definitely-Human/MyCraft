using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure 
{
    public static Queue<VoxelMod> GenerateMajorFlora(int index, Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        switch (index)
        {
            case 0:
                return MakeTree(position, minTrunkHeight, maxTrunkHeight);
            case 1:
                return MakeCactus(position, minTrunkHeight, maxTrunkHeight);

        }

        return new Queue<VoxelMod>();
    }

    public static Queue<VoxelMod> MakeTree (Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        if (position.x % 2 == 1 || position.z % 2 == 1)
            return queue;
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 6));



        for (int x = -2;x < 3; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                for (int z = -2; z < 3; z++)
                {
                    if (x == 0 && z == 0 && y < height) continue;
                    queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height -2 + y, position.z + z), 11));
                }
            }
        }
        return queue;
    }

    public static Queue<VoxelMod> MakeCactus(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 2502f, 2f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 12));

        queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + height, position.z), 13));


        return queue;
    }
}
