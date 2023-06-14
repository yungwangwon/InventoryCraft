using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "new tool class", menuName = "Item/Consumable")]
// �Һ������ 
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
