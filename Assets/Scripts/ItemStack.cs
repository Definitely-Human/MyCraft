using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack 
{
    private short id;
    private int ammount;

    public short Id
    {
        get => id;
        set => id = value;
    }

    public int Ammount
    {
        get => ammount;
        set => ammount = value;
    }
}
