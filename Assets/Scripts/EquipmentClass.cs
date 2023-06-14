using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//¿Â∫Ò
[CreateAssetMenu(fileName = "new tool class", menuName = "Item/Equipment")]
public class EquipmentClass : ItemClass
{
	[Header("Equipment")]
	public Type type;
	public enum Type
	{
		head,
		top,
		bottom,
		shoes
	}


	public override ItemClass GetItem() { return this; }
	public override ToolClass GetTool() { return null; }
	public override MiscClass GetMisc() { return null; }
	public override ConsumableClass GetConsumable() { return null; }
	public override EquipmentClass GetEquipment() { return this; }

}
