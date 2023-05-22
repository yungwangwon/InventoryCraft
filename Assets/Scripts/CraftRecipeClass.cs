using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newCraftRecipe", menuName = "Crafting/Recipe")]
public class CraftRecipeClass : ScriptableObject
{
	[Header("Crafting Recipe")]
	public SlotClass[] inputItems;
	public SlotClass outputItem;
}
