using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
