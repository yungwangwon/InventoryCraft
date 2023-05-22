using System.Collections;
using UnityEngine;

//모든 아이템의 기본이 되는 클래스 loot class
public abstract class ItemClass : ScriptableObject
{
	[Header("Item")]
    public string itemName;
    public Sprite itemIcon;
	public int itemCode;
	public bool isStackable = true;

    public abstract ItemClass GetItem();
	public abstract ToolClass GetTool();
	public abstract MiscClass GetMisc();
	public abstract ConsumableClass GetConsumable();

}
