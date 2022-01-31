using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDropHandler : MonoBehaviour
{

    [SerializeField] private UIItemSlot cursorSlot = null;
    [SerializeField] private GraphicRaycaster m_Raycaster = null;
    [SerializeField] private EventSystem m_EventSystem = null;
    private ItemSlot cursorItemSlot;
    private PointerEventData m_PointerEventData;
    private World world;
    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if (!world.InUI)
            return;

        cursorSlot.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick(UIItemSlot clickedSlot)
    {
        if (clickedSlot == null)
            return;
        if (!cursorSlot.HasItem && !clickedSlot.HasItem)
            return;

        if (!cursorSlot.HasItem && clickedSlot.HasItem)
        {
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            return;
        }
        if (cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            return;
        }
        if (cursorSlot.HasItem && clickedSlot.HasItem)
        {
            if(cursorItemSlot.stack.Id != clickedSlot.itemSlot.stack.Id)
            {
                ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();

                clickedSlot.itemSlot.InsertStack(oldCursorSlot);
                if(!clickedSlot.itemSlot.isCreative)
                    cursorSlot.itemSlot.InsertStack(oldSlot);
            }
        }

    }

    private UIItemSlot CheckForSlot()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        List<RaycastResult> result = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, result);

        foreach(RaycastResult res in result)
        {
            if(res.gameObject.tag == "UIItemSlot")
            {
                return res.gameObject.GetComponent<UIItemSlot>();
            }
        }
        return null;
    }

}
