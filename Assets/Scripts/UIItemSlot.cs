using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class UIItemSlot : MonoBehaviour
{
	public bool isLinked = false;
	public ItemSlot itemSlot;
	public Image slotImage;
	public Image slotIcon;
	public TextMeshPro slotAmmount;

	public bool HasItem
    {
        get
        {
			if (itemSlot == null)
				return false;
			else 
				return true;
        }
    }

	public UIItemSlot(){
		
	}
    
}
