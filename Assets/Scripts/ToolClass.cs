using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "new tool class", menuName = "Item/Tool")]
//도구	itemclass 상속
public class ToolClass : ItemClass
{

	[Header("Tool")]
	//도구 유형 enum정의후 저장 변수
	public ToolType toolType;
	public enum ToolType
	{
		weapon,
		pickaxe,
		axe,
		fishing_rod
	}

	public override ItemClass GetItem() { return this; }
	public override ToolClass GetTool() { return this; }
	public override MiscClass GetMisc() { return null; }
	public override ConsumableClass GetConsumable() { return null; }
	public override EquipmentClass GetEquipment() { return null; }

}
