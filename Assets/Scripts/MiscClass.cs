using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "new tool class", menuName = "Item/Misc")]
//기타 아이템class, itemclass 상속
public class MiscClass : ItemClass
{
	public override ItemClass GetItem() { return this; }
	public override ToolClass GetTool() { return null; }
	public override MiscClass GetMisc() { return this; }
	public override ConsumableClass GetConsumable() { return null; }
}
