using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class SlotClass 
{
	[SerializeField]
	private ItemClass item;	//item°´Ã¼
	[SerializeField]
	private int num;	//item°³¼ö

	public SlotClass() { item = null; num = 0; }
	public SlotClass(ItemClass _item, int _num)
	{
		this.item = _item;
		this.num = _num;
	}
	public SlotClass(SlotClass _slot)
	{
		this.item = _slot.item;
		this.num = _slot.num;

	}

	public void Clear()
	{
		this.item = null;
		this.num = 0;
	}

	public ItemClass GetItem() { return item; }
	public int GetNum() { return num; }
	public void AddNum(int _num) { num += _num; }
	public void SetNum(int _num) { num = _num; }
	public void SubNum(int _num) { num -= _num; }
	public void AddItem(ItemClass _item, int _num)
	{
		this.item = _item;
		this.num = _num;
	}
}
