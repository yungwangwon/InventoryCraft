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

//�κ��丮�Ŵ���
public class InventoryManager : MonoBehaviour
{
	[SerializeField] private List<CraftRecipeClass> craftRecipes = new List<CraftRecipeClass>();

	[SerializeField] private GameObject itemCursor;
	[SerializeField] private GameObject slotsHolder_Inventory;
	[SerializeField] private GameObject slotsHolder_Craft;

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
	private void Start()
	{
		//�κ��丮 ������ slotholer�� �������ִ� ���� ��ŭ �Ҵ�
		slots = new GameObject[slotsHolder_Inventory.transform.childCount + 
			slotsHolder_Craft.transform.childCount];
		items = new SlotClass[slots.Length];
		for(int i =0; i < slots.Length; i++)
		{
			items[i] = new SlotClass();
		}
		for (int i = 0; i < startingItems.Length; i++)
		{
			items[i] = startingItems[i];
		}

		//
		for (int i = 0; i < slotsHolder_Inventory.transform.childCount; i++)
			slots[i] = slotsHolder_Inventory.transform.GetChild(i).gameObject;
		for (int i = 0; i < slotsHolder_Craft.transform.childCount; i++)
			slots[i + slotsHolder_Inventory.transform.childCount] = slotsHolder_Craft.transform.GetChild(i).gameObject;

		RefreshUI();
		//Add(itemToAdd, 1);
		//Remove(itemToRemove);
	}

	private void Update()
	{
		itemCursor.SetActive(isMovingItem);
		itemCursor.transform.position = Input.mousePosition;
		if (isMovingItem)
			itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;

		//���콺 Ŭ��
		if (Input.GetMouseButtonDown(0))
		{	
			//���� �������� �����̴����϶� (�������� ������ �����϶�)
			if(isMovingItem)
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

	//����
	public bool Craft()
	{
		int i;
		for(i = 0; i< craftRecipes.Count; i++)
		{
			for(int j = 0; j<9;j++)
			{
				if (craftRecipes[i].inputItems[j].GetItem() == items[15 + j].GetItem())
					continue;
				else
					return false;
			}

		}

		items[items.Length-1] = new SlotClass(craftRecipes[i-1].outputItem.GetItem(), 1);

		RefreshUI();


		return true;
	}

	//������ �߰�
	public bool Add(ItemClass item, int num)
	{
		//�������� �ߺ� �����ϴ��� ��
		SlotClass slot = Contains(item);
		if (slot != null && slot.GetItem().isStackable) //�ߺ��̰� �ش� �������� �ߺ����� ������������
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

	//�߰��Ϸ��� �������� �̹� �κ��丮 �ȿ� �����ϸ� �� �������� SlotClass�� ��ȯ
	public SlotClass Contains(ItemClass item)
	{
		for(int i =0;i< slotsHolder_Inventory.transform.childCount; i++)
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
		for(int i =0; i<slots.Length; i++)
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
		movingSlot = new SlotClass(originalSlot.GetItem(), Mathf.CeilToInt( originalSlot.GetNum() / 2.0f ));
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


		if (originalSlot == null || originalSlot == items[items.Length - 1])
		{
			Add(movingSlot.GetItem(), movingSlot.GetNum());
			movingSlot.Clear();
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
		if (originalSlot == null || originalSlot == items[items.Length - 1])
		{
			Add(movingSlot.GetItem(), movingSlot.GetNum());
			movingSlot.Clear();
			isMovingItem = false;
			return false;
		}
		
		
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
