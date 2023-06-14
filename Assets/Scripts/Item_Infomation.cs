using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Infomation : MonoBehaviour
{
	[SerializeField] private ItemClass item;

	public ItemClass getItem() { return item; }
}
