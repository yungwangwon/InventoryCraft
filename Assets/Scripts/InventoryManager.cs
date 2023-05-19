using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using JetBrains.Annotations;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;

//인벤토리매니저
public class InventoryManager : MonoBehaviour
{
	[SerializeField] private GameObject itemCursor;
	[SerializeField] private GameObject slotsHolder;

	[SerializeField] private ItemClass itemToAdd;
	[SerializeField] private ItemClass itemToRemove;

	[SerializeField]
	private SlotClass[] startingItems;

	//저장되어있는 아이템
	private SlotClass[] items;

	//모든 슬롯
	private GameObject[] slots;

	//아이템 움직일때 필요한 슬롯c
	private SlotClass movingSlot;
	private SlotClass tempMovingSlot;
	private SlotClass originalSlot;
	private bool isMovingItem = false;
	private void Start()
	{
		//인벤토리 슬롯을 slotholer에 가지고있는 개수 만큼 할당
		slots = new GameObject[slotsHolder.transform.childCount];
		items = new SlotClass[slots.Length];
		for(int i =0; i < slots.Length; i++)
		{
			items[i] = new SlotClass();
		}
		for (int i = 0; i < startingItems.Length; i++)
		{
			items[i] = startingItems[i];
		}

		//Debug.Log(items.Length);
		for (int i = 0; i < slotsHolder.transform.childCount; i++)
			slots[i] = slotsHolder.transform.GetChild(i).gameObject;

		RefreshUI();
		Add(itemToAdd, 1);
		Remove(itemToRemove);
	}

	private void Update()
	{
		itemCursor.SetActive(isMovingItem);
		itemCursor.transform.position = Input.mousePosition;
		if (isMovingItem)
			itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;

		//마우스 클릭
		if (Input.GetMouseButtonDown(0))
		{	
			//지금 아이템이 움직이는중일때 (아이템을 선택한 상태일때)
			if(isMovingItem)
			{
				EndItemMove();
			}
			else
				BeginItemMove();
		}
	}
	#region Inventory Utils
	public void RefreshUI()
	{
		for (int i = 0; i < slots.Length; i++)
		{
			try
			{
				slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
				slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].GetItem().itemIcon;
				if (items[i].GetItem().isStackable)
					slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = items[i].GetNum()+ "";
				else
					slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";


			}
			catch 
			{
				slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
				slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
				slots[i].transform.GetChild(1).GetComponent< TextMeshProUGUI >().text = "";

			}
		}
	}

	//아이템 추가
	public bool Add(ItemClass item, int num)
	{
		//아이템이 중복 존재하는지 비교
		SlotClass slot = Contains(item);
		if (slot != null && slot.GetItem().isStackable) //중복이고 해당 아이템을 중복으로 가질수있을때
			slot.AddNum(1);
		else
		{
			for(int i =0; i < items.Length; i++)
			{
				if (items[i].GetItem() == null)
				{
					items[i].AddItem(item, num);
					break;
				}
			}
			// 인벤토리에있는 아이템이 최대슬롯의 용량을 넘긴지 검사
			//if (items.Count < slots.Length)
			//	items.Add(new SlotClass(item, 1));
			//else
			//	return false;
		}
		RefreshUI();
		return true;
	}
	//아이템 삭제
	public bool Remove(ItemClass item)
	{

		//아이템이 존재하는지 비교
		SlotClass temp = Contains(item);
		if (temp != null)   //존재 할때
		{
			if (temp.GetNum() > 1)
				temp.SubNum(1);
			else
			{
				int slotToRemoveIndex = 0;
				for(int i =0;i < items.Length; i++)
				{
					if (items[i].GetItem() == item)
					{
						slotToRemoveIndex = i;
						break;
					}
				}
				items[slotToRemoveIndex].Clear();
			}
		}
		else
			return false;

		RefreshUI();
		return true;
	}

	//추가하려는 아이템이 이미 인벤토리 안에 존재하면 그 아이템의 SlotClass를 반환
	public SlotClass Contains(ItemClass item)
	{
		for(int i =0;i< items.Length ; i++)
		{
			if (items[i].GetItem() == item)
				return items[i];
		}
		return null;
	}
	#endregion Inventory Utils

	#region Moving Stuff
	//가까운 슬롯을 선택해서 반환
	private SlotClass GetClosestSlot()
	{
		Debug.Log(Input.mousePosition);

		for(int i =0; i<slots.Length; i++)
		{
			if (Vector2.Distance(slots[i].transform.position, Input.mousePosition) <= 25)
				return items[i];
		}

		return null;
	}

	//아이템을 들었을때
	private bool BeginItemMove()
	{
		//마우스 포인터와 가까운 슬롯 선택
		originalSlot = GetClosestSlot();
		//슬롯이 존재하지않거나 슬롯속의 아이템이 존재하지않는다면
		if (originalSlot == null || originalSlot.GetItem() == null)
			return false;

		// movingslot에 선택된 슬롯을 복사한후 복사한 슬롯은 clear해줌
		movingSlot = new SlotClass(originalSlot);
		originalSlot.Clear();
		isMovingItem = true;
		RefreshUI();

		return true;

	}

	//아이템을 놓았을때
	private bool EndItemMove()
	{
		originalSlot = GetClosestSlot();
		if (originalSlot == null)
		{
			Add(movingSlot.GetItem(), movingSlot.GetNum());
			movingSlot.Clear();
		}
		else
		{
			if (originalSlot.GetItem() != null)
			{
				//이동하려는 아이템이 해당 슬롯아이템과 같은경우
				if (originalSlot.GetItem() == movingSlot.GetItem())
				{
					// 이동 슬롯의 아이템이 stackable일때
					if (originalSlot.GetItem().isStackable)
					{
						originalSlot.AddNum(movingSlot.GetNum());
						movingSlot.Clear();
					}
					else
						return false;
				}
				//이동하려는 슬롯에 아이템이 존재하지만 이동하려는 아이템이랑 다른 종류일경우
				else
				{
					tempMovingSlot = new SlotClass(originalSlot);
					originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetNum());
					movingSlot.AddItem(tempMovingSlot.GetItem(), tempMovingSlot.GetNum());

					RefreshUI();
					return true;
				}
			}
			else // 이동하려는 슬롯에 아이템이 없을경우
			{
				originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetNum());
				movingSlot.Clear();
			}
		}

		RefreshUI();
		isMovingItem = false;
		return true;
	}

	#endregion Moving Stuff
}
