using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeInvertory : MonoBehaviour
{

    public GameObject slotPrefab;
    private World world;
    private List<ItemSlot> slots = new List<ItemSlot>();

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        for(short i = 1; i< world.blockTypes.Length; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab,transform);

            ItemStack stack = new ItemStack(i, 64);
            ItemSlot slot = new ItemSlot(newSlot.GetComponent<UIItemSlot>(), stack);
            slot.isCreative = true;
            slots.Add(slot);
        }
        for(short i = (short)world.blockTypes.Length; i < 55; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, transform);
            ItemSlot slot = new ItemSlot(newSlot.GetComponent<UIItemSlot>());
            slots.Add(slot);
        }
    }

    void Update()
    {
        
    }
}
