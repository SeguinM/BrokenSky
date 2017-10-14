using UnityEngine;
using System.Collections;

public class DynamicScenery_SCRIPT : MonoBehaviour {


	public float health;   // HP of the object if appliccable.
	public bool isDestroyable; // is the object destroyable?





	void TakeDamage (int damage){

		health -= damage;

	}



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (health <= 0 && isDestroyable) {

			Destroy(gameObject);

		}
	
	}
}
