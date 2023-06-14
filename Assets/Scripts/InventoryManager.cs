using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//인벤토리매니저
public class InventoryManager : MonoBehaviour
{
	[SerializeField] private GameObject canvas;


	[SerializeField] private CraftRecipeClass[] craftRecipes;
	private CraftRecipeClass succecssCraftRecipe;

	[SerializeField] private GameObject itemCursor;
	[SerializeField] private GameObject slotsHolder_Inventory;
	[SerializeField] private GameObject slotsHolder_Craft;
	[SerializeField] private GameObject slotsHolder_Equipment;


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

	//ToolTip관련 변수
	private SlotClass curSlot;
	[SerializeField] private GameObject ToolTip;
	[SerializeField] private TextMeshProUGUI title;
	[SerializeField] private TextMeshProUGUI info;

	private int resultCraftSlot;
	private void Start()
	{
		//인벤토리 슬롯을 slotholer에 가지고있는 개수 만큼 할당
		slots = new GameObject[slotsHolder_Inventory.transform.childCount +
			slotsHolder_Craft.transform.childCount + slotsHolder_Equipment.transform.childCount];
		items = new SlotClass[slots.Length];
		for (int i = 0; i < slots.Length; i++)
		{
			items[i] = new SlotClass();
		}
		for (int i = 0; i < startingItems.Length; i++)
		{
			items[i] = startingItems[i];
		}

		//slots 설정
		for (int i = 0; i < slotsHolder_Inventory.transform.childCount; i++)
		{
			slots[i] = slotsHolder_Inventory.transform.GetChild(i).gameObject;
			items[i].slotType = SlotClass.SlotType.inventory;
		}
		for (int i = 0; i < slotsHolder_Craft.transform.childCount; i++)
		{
			slots[i + slotsHolder_Inventory.transform.childCount] = slotsHolder_Craft.transform.GetChild(i).gameObject;
			items[i+ slotsHolder_Inventory.transform.childCount].slotType = SlotClass.SlotType.craft;
		}
		for (int i = 0; i < slotsHolder_Equipment.transform.childCount; i++)
		{
			slots[i + slotsHolder_Inventory.transform.childCount + slotsHolder_Craft.transform.childCount] 
				= slotsHolder_Equipment.transform.GetChild(i).gameObject;
			items[i + slotsHolder_Inventory.transform.childCount + slotsHolder_Craft.transform.childCount].slotType = SlotClass.SlotType.equipment;
		}

		resultCraftSlot = slotsHolder_Inventory.transform.childCount + slotsHolder_Craft.transform.childCount-1;
		RefreshUI();
	}

	private void Update()
	{
		// canvas 활성,비활성
		if (Input.GetKeyDown(KeyCode.I)) 
		{
			canvas.SetActive(!canvas.activeSelf);
		}

		if (!canvas.activeSelf)
			return;

		//현재 마우스 포인터 위치의 슬롯이 있다면
		curSlot = GetClosestSlot();
		if (curSlot != null && curSlot.GetItem() != null)
		{
			//아이템 툴팁 설정 csv 형식
			SetSkillInfo(curSlot);
			ToolTip.SetActive(true);
		}
		else
			ToolTip.SetActive(false);


		itemCursor.SetActive(isMovingItem);
		itemCursor.transform.position = Input.mousePosition;
		if (isMovingItem)
			itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;

		//마우스 클릭
		if (Input.GetMouseButtonDown(0))
		{
			//지금 아이템이 움직이는중일때 (아이템을 선택한 상태일때)
			if (isMovingItem)
			{
				EndItemMove();
			}
			else
				BeginItemMove();
		}
		//우클릭
		else if (Input.GetMouseButtonDown(1))
		{
			if (isMovingItem)
			{
				EndItemMove_Single();
			}
			else
				BeginItemMove_Half();
		}
	}

	#region Inventory Utils
	//UI Update
	public void RefreshUI()
	{
		for (int i = 0; i < slots.Length; i++)
		{
			try
			{
				slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
				slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].GetItem().itemIcon;
				if (items[i].GetItem().isStackable)
					slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = items[i].GetNum() + "";
				else
					slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";


			}
			catch
			{
				slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
				slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
				slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";

			}
		}

	}

	void SetSkillInfo(SlotClass slot) // 스킬정보 불러오기(csv)
	{
		List<Dictionary<string, object>> data_Dialog = CSVReader.Read("Item_Info");

		for (int i = 0; i < data_Dialog.Count; i++)
		{
			if ((string)data_Dialog[i]["Name"] == slot.GetItem().itemName)
			{
				//이름, 정보 입력
				title.text = slot.GetItem().itemName;
				info.text = data_Dialog[i]["Information"].ToString();
				//위치 설정
				ToolTip.transform.position = Input.mousePosition;
				//Debug.Log(data_Dialog[i]["Information"].ToString());
			}
		}
	}

	//제작
	public bool Craft()
	{
		int i;
		int flag = 0;

		for (i = 0; i < craftRecipes.Length; i++)   //레시피 수만큼 반복
		{
			for (int j = 0; j < 9; j++)
			{	
				//레시피 속의 아이템과 제작대속의 아이템 비교
				if (craftRecipes[i].inputItems[j].GetItem() == items[slotsHolder_Inventory.transform.childCount + j].GetItem())
				{
					flag++;
				}
				else
					break;
			}

			if (flag == 9)	//성공
			{
				items[resultCraftSlot] = new SlotClass(craftRecipes[i].outputItem.GetItem(), 1);

				RefreshUI();

				Debug.Log("제작 성공");
				succecssCraftRecipe = craftRecipes[i];
				return true;
			}
			else  //실패
				flag = 0;

		}

		items[resultCraftSlot].Clear();
		succecssCraftRecipe = null;
		RefreshUI();
		return false;
	}

	private void EndCraft()
	{
		int tmp = slotsHolder_Inventory.transform.childCount;
		for (int i = 0; i < 9; i++)
		{	
			//조합법 검사
			if (succecssCraftRecipe.inputItems[i].GetItem() != null)
			{
				//조합법속에 있는 아이템이 도구일경우
				if (succecssCraftRecipe.inputItems[i].GetItem().GetType() == typeof(ToolClass))
				{
					items[tmp + i].Clear();
				}
				else
				{
					items[tmp + i].SubNum(succecssCraftRecipe.inputItems[i].GetNum());
					if (items[tmp + i].GetNum() == 0)
						items[tmp + i].Clear();
				}
			}
		}
	}

	//아이템 추가
	public bool Add(ItemClass item, int num = 1)
	{
		//아이템이 중복 존재하는지 비교
		SlotClass slot = Contains(item);
		if (slot != null && slot.GetItem().isStackable && slot.GetNum() < 64) //중복이고 해당 아이템을 중복으로 가질수있을때
			slot.AddNum(num);
		else
		{
			for (int i = 0; i < items.Length; i++)
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
				for (int i = 0; i < items.Length; i++)
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
		for (int i = 0; i < slotsHolder_Inventory.transform.childCount; i++)
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
		for (int i = 0; i < slots.Length; i++)
		{
			if (Vector2.Distance(slots[i].transform.position, Input.mousePosition) <= 15)
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

		//craftSlot일때
		if (originalSlot == items[resultCraftSlot])
			EndCraft();

		// movingslot에 선택된 슬롯을 복사한후 복사한 슬롯은 clear해줌
		movingSlot = new SlotClass(originalSlot);
		originalSlot.Clear();
		isMovingItem = true;
		RefreshUI();
		Craft();
		return true;

	}

	private bool BeginItemMove_Half()
	{
		//마우스 포인터와 가까운 슬롯 선택
		originalSlot = GetClosestSlot();
		//슬롯이 존재하지않거나 슬롯속의 아이템이 존재하지않는다면
		if (originalSlot == null || originalSlot.GetItem() == null)
			return false;

		// movingslot에 선택된 슬롯의 절반양을 복사 한후 그만큼을 originalslot에서 빼줌
		movingSlot = new SlotClass(originalSlot.GetItem(), Mathf.CeilToInt(originalSlot.GetNum() / 2.0f));
		originalSlot.SubNum(Mathf.CeilToInt(originalSlot.GetNum() / 2.0f));

		if (originalSlot.GetNum() == 0)
			originalSlot.Clear();

		isMovingItem = true;
		RefreshUI();
		Craft();
		return true;

	}

	//아이템을 놓았을때
	private bool EndItemMove()
	{
		originalSlot = GetClosestSlot();

		//놓으려는 곳이 비었을때, 조합 결과 슬롯일경우
		if (originalSlot == null || originalSlot == items[resultCraftSlot])
		{
			Add(movingSlot.GetItem(), movingSlot.GetNum());
			movingSlot.Clear();
		}
		//놓으려는 슬롯의 유형이 장비창이라면
		else if(originalSlot.slotType == SlotClass.SlotType.equipment)
		{
			if(movingSlot.GetItem().GetType() == typeof(EquipmentClass))
			{
				originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetNum());
				movingSlot.Clear();
			}
			else
			{
				return false;
			}
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
		Craft();
		isMovingItem = false;

		return true;
	}

	//아이템을 1개 놓기
	private bool EndItemMove_Single()
	{
		//마우스 포인터와 가까운 슬롯 선택
		originalSlot = GetClosestSlot();

		//놓으려는 슬롯이 조합결과 슬롯이거나 null값
		if (originalSlot == null || originalSlot == items[resultCraftSlot])
		{
			Add(movingSlot.GetItem(), movingSlot.GetNum());
			movingSlot.Clear();
			isMovingItem = false;
			return false;
		}

		//놓으려는 슬롯이 장비창이고 놓으려는아이템이 장비아이템이 아닐경우
		if (originalSlot.slotType == SlotClass.SlotType.equipment && movingSlot.GetItem().GetType() != typeof(EquipmentClass))
			return false;


		//놓으려는 슬롯에 아이템이 없음
		if (originalSlot.GetItem() == null)
			originalSlot.AddItem(movingSlot.GetItem(), 1);
		//놓으려는 슬롯에 아이템이 있고 같은 아이템임
		else if (originalSlot.GetItem() != null && originalSlot.GetItem() == movingSlot.GetItem())
			originalSlot.AddNum(1);
		//놓으려는 슬롯에 아이템이 있지만 다른 아이템임
		else if (originalSlot.GetItem() != null && originalSlot.GetItem() != movingSlot.GetItem())
			return false;

		movingSlot.SubNum(1);   //들고있는 아이템 1감소


		if (movingSlot.GetNum() < 1)
		{
			isMovingItem = false;
			movingSlot.Clear();
		}
		else
			isMovingItem = true;


		RefreshUI();
		Craft();
		return true;
	}

	#endregion Moving Stuff
}
