using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Requirements for using the script (won't operate without the specified components)
[RequireComponent (typeof(CharacterController))]
[RequireComponent (typeof(UnityEngine.AI.NavMeshAgent))]

public class NPC_SCRIPT : MonoBehaviour {
	

	// Stats
	public float health;       // actor's health
	public byte teamIndex;     // 0 = player's team
	public float fireRate;     // how often NPC fires. (Every x seconds)
	public float fireDuration; // how long NPC fires for. (seconds)
	public float detectionRange; // how far NPC can detect a target at 100% visibility
	public bool ignorePlayer = false;   // should the AI ignore the player?

	// Default behavior
	public enum DefaultBehavior{ Patrol, Follow, Attack, Chase}
	public DefaultBehavior defaultBehavior;
	private DefaultBehavior behavior;

	// Inventory
	public List<GameObject> weaponArray = new List<GameObject>(3); // weapon inventory
	public GameObject equippedWeapon; // currently equipped weapon
	private Gun_SCRIPT gun;           // active weapon's script

	// Actor Graphic variables
	public GameObject actorGraphic;
	public GameObject actor;
	public GameObject weaponSpawnNode;
	public GameObject muzzleSocket;

	// Navigation waypoint array
	public List<Transform> navigationQueue = new List<Transform> (0);
	byte index = 0;     // index for navigationQueue
	public GameObject target; // active target. Defaulted to Player in inspector

	// velocity vars
	public float patrolSpeed;     // how fast actor moves while patrolling
	public float chaseSpeed;      // how fast actor moves while chasing an actor

	// private variables
	private bool isAttacking = false;   // Is AI attacking?




	public void Equip(byte index){

		if (equippedWeapon){
			Destroy (equippedWeapon);
		}
		
		// defaults to first weapon in inventory if null
		equippedWeapon = weaponArray [index];

		gun = equippedWeapon.GetComponent<Gun_SCRIPT> ();
		// assigns actor
		equippedWeapon.GetComponent<Gun_SCRIPT> ().actor = actor;
		
		// Spawns the physical weapon
		equippedWeapon = GameObject.Instantiate (equippedWeapon, Vector3.zero, Quaternion.identity) as GameObject;
		equippedWeapon.transform.position = weaponSpawnNode.transform.position;
		equippedWeapon.transform.rotation = weaponSpawnNode.transform.rotation;
		equippedWeapon.transform.parent = weaponSpawnNode.transform;
		
		// Runs 'equip' function of gun script
		equippedWeapon.GetComponent<Gun_SCRIPT> ().Equip ();
		
		// assigns muzzle socket to gun script
		equippedWeapon.GetComponent<Gun_SCRIPT> ().muzzleSocket = muzzleSocket;
		
		// assigns index
		equippedWeapon.GetComponent<Gun_SCRIPT> ().index = index;

		
	}





	// Use this for initialization
	void Start () {

		// sets behavior to the default
		behavior = defaultBehavior;

		// Runs Equip
		Equip (1);

		// Patrol
		if (behavior == DefaultBehavior.Patrol) {

			StartPatrol();

		} // end of if behavor is patrol

		// Follow
		else if (behavior == DefaultBehavior.Follow){

			// sets target destination as the player
			GetComponent<UnityEngine.AI.NavMeshAgent> ().destination = GameObject.FindWithTag ("Player").transform.position;

		} // end of if behavior is Follow

		// Attack
		else if (behavior == DefaultBehavior.Attack){

			//Calls Attack function
			Attack();
			
		} // end of if behavior is patrol


		// Attack
		else if (behavior == DefaultBehavior.Chase){
			
			//Calls Attack function
			StartChase();
			
		} // end of if behavior is patrol

		// equips weapon
	
	}


	
	// Update is called once per frame
	void Update () {

		// Keeps actor rotated towards player
		if (behavior == DefaultBehavior.Attack && target) {
		
			LookYAxis(actor.transform, target.transform.position);

		} // end of if Attack



		// if chasing
		if (behavior == DefaultBehavior.Chase && target) {

			GetComponent<UnityEngine.AI.NavMeshAgent>().destination = target.transform.position;

			// checks to see if actor is within attack range
			if (attackRangeCheck()){

				GetComponent<UnityEngine.AI.NavMeshAgent>().Stop ();

				behavior = DefaultBehavior.Attack;
				Attack ();

			}

		} // end of if chasing and there's a target

		// if no target and chasing, checks for threat
		else if (behavior == DefaultBehavior.Chase && !target){

			CheckForThreat();

		}



		if (Input.GetButton ("Crouch")) {

			GetComponent<UnityEngine.AI.NavMeshAgent>().Stop();

			GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(GameObject.FindWithTag ("Player").transform.position);

			GetComponent<UnityEngine.AI.NavMeshAgent>().Resume();

		}
			
	} // end of Update





	// Rotation method  for rotating to face character. || Ryan Miller
	public void LookYAxis(Transform looker, Vector3 targetPos) {
		looker.LookAt(targetPos);
		looker.rotation = Quaternion.Euler(0, looker.eulerAngles.y, 0);
		
	}





	void StartPatrol(){

		behavior = DefaultBehavior.Patrol;

		// resets index
		index = 0;

		// AI is not attacking
		isAttacking = false;

		// sets speed to patrol parameters
		GetComponent<UnityEngine.AI.NavMeshAgent> ().speed = patrolSpeed;

		// assigns first waypoint in queue
		GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(navigationQueue[index].position);


	} // end of start patrol






	void StartChase(){

		behavior = DefaultBehavior.Chase;

		// is not attacking
		isAttacking = false;

		// sets speed
		GetComponent<UnityEngine.AI.NavMeshAgent> ().speed = chaseSpeed;

		// If no existing target is set, searches for one
		if (!target){
			CheckForThreat();
		}

		// sets destination
		GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (target.transform.position);

	} // end of start chase





	void Attack(){

		// AI is attacking
		isAttacking = true;

		// fires gun
		// equippedWeapon.GetComponent<Gun_SCRIPT> ().amIShooting = true;
		StartCoroutine ("stopShooting");

	}






	// countdown to stop shooting
	IEnumerator stopShooting() {
		
		for (float timer = fireDuration; timer >= 0; timer -= Time.deltaTime)
			yield return 0;

		// fires gun
		equippedWeapon.GetComponent<Gun_SCRIPT> ().ReleaseTrigger ();

		if (isAttacking) {

			if (attackRangeCheck()){
				StartCoroutine ("startShooting");
			}

			else if (!attackRangeCheck ()){
				StartChase ();
			}
		}

	} // end of stopShooting





	// countdown to start shooting again
	IEnumerator startShooting() 
	{
		
		for (float timer = fireRate-fireDuration; timer >= 0; timer -= Time.deltaTime)
			yield return 0;
		
		// fires gun
		equippedWeapon.GetComponent<Gun_SCRIPT> ().PullTrigger (true);

		// gets ready to stop shooting
		StartCoroutine ("stopShooting");
		
	} // end of startShooting






	// NPC scans for threats in area
	void CheckForThreat(){

		// spherecast
		int layerMask = 1 << 8;         // bit shifts to layer 8 (Actors) (I fucking hate bitshifting)

		// Runs an overlapSphere
		Collider[] hits = Physics.OverlapSphere (actor.transform.position, detectionRange, layerMask);

		// converts into a List
		List<GameObject> detectedActors = new List<GameObject> (0);
		for (int i = 0; i <= hits.Length-1; i++){
			detectedActors.Add (hits[i].gameObject);
			Debug.Log ("Added " + detectedActors[i].name + " successfully");
		} 


		// Removes player if option is true
		if (ignorePlayer){
			detectedActors.Remove (GameObject.FindWithTag ("Player"));
		}

		// Removes itself from the list
		detectedActors.Remove (actor);

		// removal queue
		List<GameObject> removalQueue = new List<GameObject> (0);

		// Removes actors of the same team index
		for (int i = 0; i <= detectedActors.Count - 1; i++){

			// checks to make sure script exists
			if (detectedActors[i].GetComponent<NPC_SCRIPT>()){
				// removes if same team index
				if (detectedActors[i].GetComponent<NPC_SCRIPT>().teamIndex == teamIndex){
					removalQueue.Add (detectedActors[i]);
				}
			} // end of if npc script exists

		} // end of for loop 

		// Iterates through queue and removes them from detected actors
		foreach (GameObject queued in removalQueue) {
			detectedActors.Remove (queued);
		}

		Debug.Log (detectedActors.Count + " targets left. Player removed");

		// checks to see if there's anything left
		if (detectedActors.Count > 0)  {
			target = detectedActors[0].gameObject;
			StartChase();
			Debug.Log ("New Target: " + target.name);
		}
		
		else {
			Debug.Log ("No Threat Found");
			StartPatrol ();
		}


		/*Debug.Log (detectedActors.Count);

		// removes itself from detected list 
		/*for (int i = 0, i++, i >= detectedActors.Length){
			Debug.Log ("iteration " + i);
			if (detectedActors[i].gameObject == actor){
				//detectedActors.
			}
		}

		if (detectedActors.Count > 0)  {
			target = detectedActors[0].gameObject;
			StartChase();
			Debug.Log ("New Target: " + target.name);
		}

		else {
			Debug.Log ("No Threat Found");
			StartPatrol ();
		} */


	} // end of check for threat










	// sets destination to next waypoint in queue
	void QueueNextWaypoint(){

		// increments the index
		index ++;

		Debug.Log (index);

		// checks to see if index has exceeded navigation queue size
		if (index > navigationQueue.Count - 1) {
			index = 0;
		}

		// Sets NPC's destination to new waypoint
		GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(navigationQueue[index].position);

	}



	// collision listener
	void OnTriggerEnter(Collider other){

		// tests to see if NPC has reached designated patrol point
		if (other.gameObject == navigationQueue [index].gameObject && behavior == DefaultBehavior.Patrol) {

			QueueNextWaypoint();

		} // end of if trigger is waypoint

	} // end of OnTriggerEnter






	// Activated via SendMessage from a separate script attached to the cone collider 
	public void DetectActor(GameObject suspect){

		// makes sure the detected actor isn't himself 
		if (suspect != actor){

			// temporary container to store enemy index
			byte suspectTeamIndex = 0;

			// gets index
			if (suspect.tag == "Player"){
				suspectTeamIndex = 0;
			}

			else if (suspect.tag != "Player"){
				suspectTeamIndex = suspect.GetComponent<NPC_SCRIPT>().teamIndex;
			}

			//Debug.Log (suspect.name + ": Team " + suspectTeamIndex);

			// layer mask
			int layerMask = 1 << 9;         // bit shifts to layer 9 (Triggers) (I fucking hate bitshifting)
			layerMask = ~layerMask;          // inverts layermask

			// creates temp variables for distance check
			Vector3 direction = suspect.transform.position - actor.transform.position;
		
			RaycastHit hit;
			Ray ray = new Ray (muzzleSocket.transform.position, direction);
		
			// Debug
			Debug.DrawRay (ray.origin, direction*1000, Color.green, 2);
		
			//Checks to see if ray hits and suspect is not of the same team
			if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask) && suspectTeamIndex != teamIndex) {

				Debug.Log ("Detected " + suspect.name);
				// target is asssigned the suspect

				if (suspect != GameObject.FindWithTag ("Player") || !ignorePlayer){
					target = suspect;
					StartChase ();
				}

			} // end of raycast

		} // end of if suspect is not actor
		
	} // end of DetectActor






	// Check to see if target is within Attack range
	bool attackRangeCheck(){

		// layer mask
		int layerMask = 1 << 9;         // bit shifts to layer 9 (Triggers) (I fucking hate bitshifting)
		layerMask = ~layerMask;          // inverts layermask

		// checks to see if target is null
		if (target){

			// creates temp variables for distance check
			Vector3 direction = target.transform.position - actor.transform.position;
		
			RaycastHit hit;
			Ray ray = new Ray (muzzleSocket.transform.position, direction);

			// Debug
			Debug.DrawRay (ray.origin, direction*1000, Color.red, 2);

			//Checks to see if ray hits
			if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)){

				//Checks to see if ray is right length
				if (hit.distance <= equippedWeapon.GetComponent<Gun_SCRIPT>().effectiveRange && hit.collider.gameObject == target){

					return true;
		
				} // end of if distance 

				else {

					return false;
				}

			} // end of if ray hits

			else {
				return false;
			}

		} // end of if target is null

		else {
			return false;
		}

	} // end of attack range check






	// Takes damage (uses sendMessage from other scripts
	void TakeDamage (float damage){
		
		health -= damage;

		// destroys actor if health falls to zero
		if (health <= 0) {
			Die();
		}

		Debug.Log (actor.name + ": " + health + " HP left");
		
	} // end of Take Damage




	// when hit by something, actor is given this new enemy as a target
	void GetPerpetrator(GameObject perpetrator){

		if (perpetrator == null)
						return;
		if (!target){
			target = perpetrator;
			StartChase ();
		}

	} // end of getPerpetrator





	void Die(){

		// unparents weapon from NPC
		equippedWeapon.SendMessage ("DropMe");

		// Destroys NPC entity
		Destroy (gameObject);
		
	} // end of Die



}
