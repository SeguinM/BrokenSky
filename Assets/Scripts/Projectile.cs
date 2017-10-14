using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
	
	// INTERNAL VARIABLES
	
	private float dmg;        // how much damage to inflict
	private Vector3 dir;        // direction of travel
	private float impct;      // impact force
	private float spd;     // speed
	
	
	
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//                                                     PUBLIC FUNCTIONS                                                       //
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void setData(float damage, float impactForce, Vector3 direction, float speed)
	{
		dmg = damage;
		impct = impactForce;
		dir = direction;
		spd = speed;
		Debug.Log ("set data");
	} // end of function setData
	
	
	
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//                                                     PRIVATE FUNCTIONS                                                      //
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// in case it gets lost in the abyss, this projectile will self-kill after five seconds.
	IEnumerator autokill()
	{
		
		for (float timer = 5; timer >= 0; timer -= Time.deltaTime)
			yield return 0;
		Destroy (gameObject);
	} // end of IEnumerator autokill
	
	
	
	
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//                                                     UNITY-CALLED FUNCTIONS                                                 //
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Update()
	{	 // RUN ONCE PER FRAME
		transform.position += Time.deltaTime * (spd) * dir;
	} // end of function Update
	
	public void OnTriggerEnter(Collider node)
	{
		GameObject hitObject = node.transform.gameObject;
		
		if (hitObject == null)
			return;
		
		hitObject.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);
		
		if (hitObject.GetComponent<Rigidbody>())
			hitObject.GetComponent<Rigidbody>().AddForce(dir * impct);
		
		// destroy this object
		Destroy (gameObject);
	} // end of function OnCollisionEnter

	public void Start()
	{
		StartCoroutine ("autokill");
	} // end of function start
	
	
} // end of class ArtilleryProjectile
