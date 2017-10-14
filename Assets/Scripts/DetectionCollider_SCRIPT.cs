using UnityEngine;
using System.Collections;

public class DetectionCollider_SCRIPT : MonoBehaviour {







	void OnTriggerEnter(Collider other){

		if (other.gameObject.layer == 8){
			SendMessageUpwards ("DetectActor", other.gameObject);
		}

	} // end of OnTriggerEnter







	// Use this for initialization
	void Start () {
	
	} // end of Start
	
	// Update is called once per frame
	void Update () {
	
	}
}
