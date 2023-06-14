using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerClass : MonoBehaviour
{

	[SerializeField] private InventoryManager inventoryManager;
   
    void Update()
    {
		//이동 키보드
		float h = Input.GetAxisRaw("Horizontal"); //좌우
		float v = Input.GetAxisRaw("Vertical"); //상하


		Vector3 curpos = transform.position; //현재위치 저장

		Vector3 movepos = new Vector3(h, v, 0) * 10.0f * Time.deltaTime; //다음위치

		transform.position = curpos + movepos;
	}

	//충돌
	private void OnTriggerEnter2D(Collider2D collision)
	{
		//아이템 충돌
		if(collision.tag == "Item")
		{
			//Debug.Log("item enter");
			ItemClass itemclass = collision.GetComponent<Item_Infomation>().getItem();
			inventoryManager.Add(itemclass);
			Destroy(collision.gameObject);
		}
	}
}
