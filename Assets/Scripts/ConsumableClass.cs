using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "new tool class", menuName = "Item/Consumable")]
// 소비아이템 
public class ConsumableClass : ItemClass
{
	[Header("Consumable")]
	public float healthAdded;

	public override ItemClass GetItem() { return this; }
	public override ToolClass GetTool() { return null; }
	public override MiscClass GetMisc() { return null; }
	public override ConsumableClass GetConsumable() { return this; }
	public override EquipmentClass GetEquipment() { return null; }

}
