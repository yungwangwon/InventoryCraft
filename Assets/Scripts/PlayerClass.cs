using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerClass : MonoBehaviour
{

	[SerializeField] private InventoryManager inventoryManager;
   
    void Update()
    {
		//�̵� Ű����
		float h = Input.GetAxisRaw("Horizontal"); //�¿�
		float v = Input.GetAxisRaw("Vertical"); //����


		Vector3 curpos = transform.position; //������ġ ����

		Vector3 movepos = new Vector3(h, v, 0) * 10.0f * Time.deltaTime; //������ġ

		transform.position = curpos + movepos;
	}

	//�浹
	private void OnTriggerEnter2D(Collider2D collision)
	{
		//������ �浹
		if(collision.tag == "Item")
		{
			//Debug.Log("item enter");
			ItemClass itemclass = collision.GetComponent<Item_Infomation>().getItem();
			inventoryManager.Add(itemclass);
			Destroy(collision.gameObject);
		}
	}
}
