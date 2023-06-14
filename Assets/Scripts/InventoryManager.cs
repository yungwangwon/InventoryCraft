using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//�κ��丮�Ŵ���
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

	//����Ǿ��ִ� ������
	private SlotClass[] items;

	//��� ����
	private GameObject[] slots;

	//������ �����϶� �ʿ��� ����c
	private SlotClass movingSlot;
	private SlotClass tempMovingSlot;
	private SlotClass originalSlot;
	private bool isMovingItem = false;

	//ToolTip���� ����
	private SlotClass curSlot;
	[SerializeField] private GameObject ToolTip;
	[SerializeField] private TextMeshProUGUI title;
	[SerializeField] private TextMeshProUGUI info;

	private int resultCraftSlot;
	private void Start()
	{
		//�κ��丮 ������ slotholer�� �������ִ� ���� ��ŭ �Ҵ�
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

		//slots ����
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
		// canvas Ȱ��,��Ȱ��
		if (Input.GetKeyDown(KeyCode.I)) 
		{
			canvas.SetActive(!canvas.activeSelf);
		}

		if (!canvas.activeSelf)
			return;

		//���� ���콺 ������ ��ġ�� ������ �ִٸ�
		curSlot = GetClosestSlot();
		if (curSlot != null && curSlot.GetItem() != null)
		{
			//������ ���� ���� csv ����
			SetSkillInfo(curSlot);
			ToolTip.SetActive(true);
		}
		else
			ToolTip.SetActive(false);


		itemCursor.SetActive(isMovingItem);
		itemCursor.transform.position = Input.mousePosition;
		if (isMovingItem)
			itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;

		//���콺 Ŭ��
		if (Input.GetMouseButtonDown(0))
		{
			//���� �������� �����̴����϶� (�������� ������ �����϶�)
			if (isMovingItem)
			{
				EndItemMove();
			}
			else
				BeginItemMove();
		}
		//��Ŭ��
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

	void SetSkillInfo(SlotClass slot) // ��ų���� �ҷ�����(csv)
	{
		List<Dictionary<string, object>> data_Dialog = CSVReader.Read("Item_Info");

		for (int i = 0; i < data_Dialog.Count; i++)
		{
			if ((string)data_Dialog[i]["Name"] == slot.GetItem().itemName)
			{
				//�̸�, ���� �Է�
				title.text = slot.GetItem().itemName;
				info.text = data_Dialog[i]["Information"].ToString();
				//��ġ ����
				ToolTip.transform.position = Input.mousePosition;
				//Debug.Log(data_Dialog[i]["Information"].ToString());
			}
		}
	}

	//����
	public bool Craft()
	{
		int i;
		int flag = 0;

		for (i = 0; i < craftRecipes.Length; i++)   //������ ����ŭ �ݺ�
		{
			for (int j = 0; j < 9; j++)
			{	
				//������ ���� �����۰� ���۴���� ������ ��
				if (craftRecipes[i].inputItems[j].GetItem() == items[slotsHolder_Inventory.transform.childCount + j].GetItem())
				{
					flag++;
				}
				else
					break;
			}

			if (flag == 9)	//����
			{
				items[resultCraftSlot] = new SlotClass(craftRecipes[i].outputItem.GetItem(), 1);

				RefreshUI();

				Debug.Log("���� ����");
				succecssCraftRecipe = craftRecipes[i];
				return true;
			}
			else  //����
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
			//���չ� �˻�
			if (succecssCraftRecipe.inputItems[i].GetItem() != null)
			{
				//���չ��ӿ� �ִ� �������� �����ϰ��
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

	//������ �߰�
	public bool Add(ItemClass item, int num = 1)
	{
		//�������� �ߺ� �����ϴ��� ��
		SlotClass slot = Contains(item);
		if (slot != null && slot.GetItem().isStackable && slot.GetNum() < 64) //�ߺ��̰� �ش� �������� �ߺ����� ������������
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
			// �κ��丮���ִ� �������� �ִ뽽���� �뷮�� �ѱ��� �˻�
			//if (items.Count < slots.Length)
			//	items.Add(new SlotClass(item, 1));
			//else
			//	return false;
		}
		RefreshUI();
		return true;
	}
	//������ ����
	public bool Remove(ItemClass item)
	{

		//�������� �����ϴ��� ��
		SlotClass temp = Contains(item);
		if (temp != null)   //���� �Ҷ�
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

	//�߰��Ϸ��� �������� �̹� �κ��丮 �ȿ� �����ϸ� �� �������� SlotClass�� ��ȯ
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
	//����� ������ �����ؼ� ��ȯ
	private SlotClass GetClosestSlot()
	{
		for (int i = 0; i < slots.Length; i++)
		{
			if (Vector2.Distance(slots[i].transform.position, Input.mousePosition) <= 15)
				return items[i];
		}

		return null;
	}

	//�������� �������
	private bool BeginItemMove()
	{
		//���콺 �����Ϳ� ����� ���� ����
		originalSlot = GetClosestSlot();
		//������ ���������ʰų� ���Լ��� �������� ���������ʴ´ٸ�
		if (originalSlot == null || originalSlot.GetItem() == null)
			return false;

		//craftSlot�϶�
		if (originalSlot == items[resultCraftSlot])
			EndCraft();

		// movingslot�� ���õ� ������ �������� ������ ������ clear����
		movingSlot = new SlotClass(originalSlot);
		originalSlot.Clear();
		isMovingItem = true;
		RefreshUI();
		Craft();
		return true;

	}

	private bool BeginItemMove_Half()
	{
		//���콺 �����Ϳ� ����� ���� ����
		originalSlot = GetClosestSlot();
		//������ ���������ʰų� ���Լ��� �������� ���������ʴ´ٸ�
		if (originalSlot == null || originalSlot.GetItem() == null)
			return false;

		// movingslot�� ���õ� ������ ���ݾ��� ���� ���� �׸�ŭ�� originalslot���� ����
		movingSlot = new SlotClass(originalSlot.GetItem(), Mathf.CeilToInt(originalSlot.GetNum() / 2.0f));
		originalSlot.SubNum(Mathf.CeilToInt(originalSlot.GetNum() / 2.0f));

		if (originalSlot.GetNum() == 0)
			originalSlot.Clear();

		isMovingItem = true;
		RefreshUI();
		Craft();
		return true;

	}

	//�������� ��������
	private bool EndItemMove()
	{
		originalSlot = GetClosestSlot();

		//�������� ���� �������, ���� ��� �����ϰ��
		if (originalSlot == null || originalSlot == items[resultCraftSlot])
		{
			Add(movingSlot.GetItem(), movingSlot.GetNum());
			movingSlot.Clear();
		}
		//�������� ������ ������ ���â�̶��
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
				//�̵��Ϸ��� �������� �ش� ���Ծ����۰� �������
				if (originalSlot.GetItem() == movingSlot.GetItem())
				{
					// �̵� ������ �������� stackable�϶�
					if (originalSlot.GetItem().isStackable)
					{
						originalSlot.AddNum(movingSlot.GetNum());
						movingSlot.Clear();
					}
					else
						return false;
				}
				//�̵��Ϸ��� ���Կ� �������� ���������� �̵��Ϸ��� �������̶� �ٸ� �����ϰ��
				else
				{
					tempMovingSlot = new SlotClass(originalSlot);
					originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetNum());
					movingSlot.AddItem(tempMovingSlot.GetItem(), tempMovingSlot.GetNum());

					RefreshUI();
					return true;
				}
			}
			else // �̵��Ϸ��� ���Կ� �������� �������
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

	//�������� 1�� ����
	private bool EndItemMove_Single()
	{
		//���콺 �����Ϳ� ����� ���� ����
		originalSlot = GetClosestSlot();

		//�������� ������ ���հ�� �����̰ų� null��
		if (originalSlot == null || originalSlot == items[resultCraftSlot])
		{
			Add(movingSlot.GetItem(), movingSlot.GetNum());
			movingSlot.Clear();
			isMovingItem = false;
			return false;
		}

		//�������� ������ ���â�̰� �������¾������� ���������� �ƴҰ��
		if (originalSlot.slotType == SlotClass.SlotType.equipment && movingSlot.GetItem().GetType() != typeof(EquipmentClass))
			return false;


		//�������� ���Կ� �������� ����
		if (originalSlot.GetItem() == null)
			originalSlot.AddItem(movingSlot.GetItem(), 1);
		//�������� ���Կ� �������� �ְ� ���� ��������
		else if (originalSlot.GetItem() != null && originalSlot.GetItem() == movingSlot.GetItem())
			originalSlot.AddNum(1);
		//�������� ���Կ� �������� ������ �ٸ� ��������
		else if (originalSlot.GetItem() != null && originalSlot.GetItem() != movingSlot.GetItem())
			return false;

		movingSlot.SubNum(1);   //����ִ� ������ 1����


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
