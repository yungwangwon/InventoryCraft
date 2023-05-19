using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "new tool class", menuName = "Item/Tool")]
//����	itemclass ���
public class ToolClass : ItemClass
{

	[Header("Tool")]
	//���� ���� enum������ ���� ����
	public ToolType toolType;
	public enum ToolType
	{
		weapon,
		pickaxe,
		axe
	}

	public override ItemClass GetItem() { return this; }
	public override ToolClass GetTool() { return this; }
	public override MiscClass GetMisc() { return null; }
	public override ConsumableClass GetConsumable() { return null; }
}