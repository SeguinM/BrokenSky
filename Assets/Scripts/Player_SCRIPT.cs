using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Requirements for using the script (won't operate without the specified components)
[RequireComponent (typeof(CharacterController))]

public class Player_SCRIPT : MonoBehaviour {

	// Game Variables
	public float health;
	public float stamina;

	// Inventory
	public List<GameObject> weaponArray = new List<GameObject>(3); // weapon inventory
	public GameObject equippedWeapon; // currently equipped weapon
	private Gun_SCRIPT gun;           //  weapon script reference

	// Movement Variables
	private float movementSpeed; // active movement speed
	public float standSpeed;    // standing speed
	public float crouchSpeed;   // crouched speed
	public float crawlSpeed;
	public float mouseSensitivity = 2.0f;
	public float upDownRange = 60.0f;
	public float verticalRotation = 0;
	float verticalVelocity = 0;
	public float jumpSpeed = 500.0f;
	public float weight = 0f;
	public float zoomFactor = 10000;

	// Player Graphic variables
	public GameObject playerGraphic;
	public GameObject player;
	public GameObject weaponSpawnNode;
	public GameObject muzzleSocket;
	public GameObject cameraIsoSocket;    // third-person camera socket
	public GameObject cameraFPSocket;     // first-person camera socket
	public GameObject thirdPersonGunSocket; // socket that muzzle_socket is reassigned to once back to third-person
	public GameObject firstPersonGunSocket; // socket that muzzle_socket is reassigned to once in first person

	// Camera settings
	public enum CameraMode{ ThirdPerson, FirstPerson}
	public CameraMode cameraType;

	// Player position
	public byte stance = 0;
	/* 0 = standing
	 * 1 = crouching
	 * 2 = prone */

	// Booleans
	public bool isMovable = true;
	
	CharacterController characterController;
	public BoxCollider proneCollider;







	void ToggleCamera(){

		// Assigns camera
		GameObject camera = GameObject.FindWithTag ("MainCamera");

		if (cameraType == CameraMode.ThirdPerson){

			// temporarily rotates character to fix shitty gun position
			Quaternion tempRot = playerGraphic.transform.rotation;
			playerGraphic.transform.rotation = Quaternion.LookRotation(Vector3.zero);


			camera.transform.parent = cameraFPSocket.transform;
			camera.transform.rotation = cameraFPSocket.transform.localRotation;
			camera.transform.position = cameraFPSocket.transform.position;
			cameraType = CameraMode.FirstPerson;

			equippedWeapon.transform.position = weaponSpawnNode.transform.position;
			equippedWeapon.transform.position.Normalize();

			// locks the mouse cursor so you don't see it in-game. 
			Screen.lockCursor = true;


			/* ties gun to camera movement
			equippedWeapon.transform.parent = camera.transform;
			equippedWeapon.transform.rotation = Quaternion.Euler (0, -90, 0);
			equippedWeapon.transform.position = firstPersonGunSocket.transform.position; */

			// Assigns muzzle socket position to the camera
			muzzleSocket.transform.position = camera.transform.position;
			muzzleSocket.transform.rotation = camera.transform.rotation;
			muzzleSocket.transform.parent = camera.transform;

			// ties gun location
			//	equippedWeapon.transform.position = weaponSpawnNode.transform.position;
			equippedWeapon.transform.rotation = Quaternion.Euler(Vector3.forward);
			equippedWeapon.transform.parent = camera.transform;
			equippedWeapon.transform.Rotate (0, -90, 0);
			equippedWeapon.transform.localScale.Normalize();

			// resets character rotation
			playerGraphic.transform.rotation = tempRot;

		} // end of if camera type is third person


		else if (cameraType == CameraMode.FirstPerson){

			// ties gun to player
			equippedWeapon.transform.parent = weaponSpawnNode.transform;
			equippedWeapon.transform.rotation = weaponSpawnNode.transform.rotation;
			equippedWeapon.transform.position = weaponSpawnNode.transform.position;
			equippedWeapon.transform.localScale.Normalize();

			
			camera.transform.parent = cameraIsoSocket.transform;
			camera.transform.position = cameraIsoSocket.transform.position;
			camera.transform.rotation = Quaternion.Euler(45, 0, 0);
			// camera.transform.localRotation = Quaternion.Euler(45, 0, 0);
			cameraType = CameraMode.ThirdPerson;


			// Assigns muzzle socket position to the 3rd person mode
			muzzleSocket.transform.parent = thirdPersonGunSocket.transform;
			muzzleSocket.transform.position = thirdPersonGunSocket.transform.position;
			muzzleSocket.transform.rotation = thirdPersonGunSocket.transform.rotation;
			
			// locks the mouse cursor so you don't see it in-game. 
			Screen.lockCursor = false;

			// resets vertical rotation
			verticalRotation = 0;

			
		} // end of if camera type is third person


	} // end of toggle camera








	public void Equip(byte index){

		if (equippedWeapon)
			Destroy (equippedWeapon);

		// defaults to first weapon in inventory if null
		equippedWeapon = weaponArray [index];

		// assigns actor
		equippedWeapon.GetComponent<Gun_SCRIPT> ().actor = player;

		// Spawns the physical weapon
		equippedWeapon = GameObject.Instantiate (equippedWeapon, Vector3.zero, Quaternion.identity) as GameObject;
		equippedWeapon.transform.position = weaponSpawnNode.transform.position;
		equippedWeapon.transform.rotation = weaponSpawnNode.transform.rotation;
		equippedWeapon.transform.parent = weaponSpawnNode.transform;

		// sets reference to gun's script
		gun = equippedWeapon.GetComponent<Gun_SCRIPT> ();

		// Runs 'equip' function of gun script
		equippedWeapon.GetComponent<Gun_SCRIPT> ().Equip ();

		// assigns muzzle socket to gun script
		equippedWeapon.GetComponent<Gun_SCRIPT> ().muzzleSocket = muzzleSocket;

		// Assigns gui image
		if (GameObject.FindWithTag("GameController")){
			GameObject.FindWithTag ("GameController").GetComponent<GUI_SCRIPT> ().equippedWep_gui = equippedWeapon.GetComponent<Gun_SCRIPT>().guiImage;
		}

		// assigns index
		equippedWeapon.GetComponent<Gun_SCRIPT> ().index = index;

		// assigns gun script to GUI
		GameObject.FindWithTag ("GameController").GetComponent<GUI_SCRIPT> ().equippedWepScript = equippedWeapon.GetComponent<Gun_SCRIPT> ();

	}


	// Use this for initialization
	void Start () {

		characterController = GetComponent<CharacterController>();

		// Runs Equip
		Equip (0);

		movementSpeed = standSpeed;
		// Debug.Log (movementSpeed);

		// locks the mouse cursor so you don't see it in-game. 
		// Screen.lockCursor = true;

		// sets player stats
		health = 100.00f;
		stamina = 100.00f;
	
	}





	// checks player input
	private void CheckPlayerInput()
	{
		if (Input.GetButton ("Fire1") && gun != null)
			gun.PullTrigger();
		if (Input.GetButtonUp ("Fire1") && gun != null)
			gun.ReleaseTrigger();

		// Reloads if reload key is pressed
		if (Input.GetButton ("Interact"))			
			gun.Reload ();

	} // end of function CheckPlayerInput

	// Update is called independently of framerate per frame
	void Update () {

		CheckPlayerInput();

		// Listens for Aim button to toggle camera view
		if (Input.GetButtonDown ("Aim")){
			ToggleCamera();
		}

		// only happens if camera type is third person
		if (cameraType == CameraMode.ThirdPerson){
			// Movement
			float forwardSpeed = Input.GetAxis("Vertical");
			float strafeSpeed = Input.GetAxis("Horizontal");

			// adjusts vertical velocity based on gravity
			verticalVelocity += Physics.gravity.y * (weight / 5) * Time.deltaTime;

			// Only jumps if character is on ground
			if(characterController.isGrounded && Input.GetButtonDown("Jump") && stance == 0) {
			
				verticalVelocity = jumpSpeed;
			
			}
		
			Vector3 speed = new Vector3(strafeSpeed * movementSpeed, verticalVelocity, forwardSpeed * movementSpeed);
	
			characterController.Move(speed * Time.deltaTime);


			// Making character face direction of travel
			Vector3 moveDirection = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
			if (moveDirection != Vector3.zero && isMovable == true){
				Quaternion newRotation = Quaternion.LookRotation(moveDirection);
				playerGraphic.transform.rotation = Quaternion.Slerp(playerGraphic.transform.rotation, newRotation, Time.deltaTime * 8);
			}



			// Handles camera zoom

			// Assigns camera and min/max
			GameObject camera = GameObject.FindWithTag ("MainCamera");
			Vector3 cameraMin = new Vector3 (player.transform.position.x, player.transform.position.y + 220, player.transform.position.z-200);
			Vector3 cameraMax = new Vector3 (player.transform.position.x, player.transform.position.y + 620, player.transform.position.z - 600);

			// lerps camera position 
			if (Input.GetAxis ("Mouse ScrollWheel")!=0 && isMovable == true){

				float zoom = Input.GetAxis ("Mouse ScrollWheel") * zoomFactor;
				float newXPosition = camera.transform.localPosition.z + zoom;

				camera.transform.localPosition = new Vector3 (camera.transform.localPosition.x, Mathf.Lerp (camera.transform.localPosition.y, (zoom * -1), Time.deltaTime * 2), Mathf.Lerp (camera.transform.localPosition.z, zoom, Time.deltaTime * 2));

				// Checks to make sure camera is within zoom constraints
				if (camera.transform.position.y > cameraMax.y){
					camera.transform.position = cameraMax;
				}
				else if (camera.transform.position.y < cameraMin.y){
					camera.transform.position = cameraMin;
				} // end of else if
			} // end of if Mouse Scrollwheel

		} // end of if camera type is third person


		// handles first-person camera rotation
		if (cameraType == CameraMode.FirstPerson){
			
			// Rotation
			float rotLeftRight = Input.GetAxis("Mouse X") * mouseSensitivity;
			playerGraphic.transform.Rotate(0, rotLeftRight, 0);
			
			verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
			verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
			Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
			
		} // end of if camera type is first person




		// Stance Changes

		// Standing to Crouch
		if (Input.GetButtonDown ("Crouch") && stance == 0 && characterController.isGrounded){

			// Sets stance to 1 (crouched)
			stance = 1;
			characterController.center = new Vector3(0, 21.6f, 0);
			characterController.height = 43.2f;
			// sets movement speed to crouch speed
			movementSpeed = crouchSpeed;

			// temporary visual interpretation of crouch
			playerGraphic.transform.localScale = new Vector3 (1, 0.5f, 1);
			playerGraphic.transform.localPosition = new Vector3 (0, 0, 0);

		}

		// Crouch to Standing
		else if (Input.GetButtonDown ("Crouch") && stance == 1 && characterController.isGrounded){

			// Sets stance to 0 (standing)
			stance = 0;
			characterController.center = new Vector3 (0, 43.2f, 0);
			characterController.height = 86.4f;
			movementSpeed = standSpeed;

			// temporary visual interpretation of crouch
			playerGraphic.transform.localScale = new Vector3 (1, 1, 1);
			playerGraphic.transform.localPosition = new Vector3 (0, 0, 0);

		}

		// prone to crouch
		else if (Input.GetButtonDown ("Crouch") && stance == 2){

			// Sets stance to crouched
			stance = 1;
			characterController.center = new Vector3 (0, 21.6f, 0);
			characterController.height = 43.2f;
			movementSpeed = crouchSpeed;

			// temporary visual interpretation of crouch
			playerGraphic.transform.localScale = new Vector3 (1, 0.5f, 1);
			playerGraphic.transform.localPosition = new Vector3 (0, 0, 0);

			// Disables prone collider
			proneCollider.enabled = false;

		}

		// prone to stand
		else if (Input.GetButtonDown ("Jump") && stance == 2){

			// sets stance to standing
			stance = 0;
			characterController.center = new Vector3 (0, 43.2f, 0);
			characterController.height = 86.4f;
			movementSpeed = standSpeed;

			// temporary visual interpretation of crouch
			playerGraphic.transform.localScale = new Vector3 (1, 1f, 1);
			playerGraphic.transform.localPosition = new Vector3 (0, 0, 0);

			// Disables prone collider
			proneCollider.enabled = false;

		}

		// standing to prone
		if (Input.GetButton ("Crouch") && stance != 2){
			StartCoroutine ("holdToProne");
		}
		// Cancels prone if button is released
		if (Input.GetButtonUp ("Crouch")){
			StopCoroutine ("holdToProne");
		}


		#region WEAPON_EQUIP_LISTENER
		// Listens for weapon equip buttons

		// Assigns Unarmed
		if (Input.GetButtonDown ("Weapon Hotkey 0")){
			Equip (0);
		}

		// Assigns Weapon 1
		if (Input.GetButtonDown ("Weapon Hotkey 1")){
			Equip (1);
		}

		// Assigns Weapon 2
		if (Input.GetButtonDown ("Weapon Hotkey 2")){
			Equip (2);
		}

		// Assigns Weapon 3
		if (Input.GetButtonDown ("Weapon Hotkey 3")){
			Equip (3);
		}

		// Assigns Weapon 4
		if (Input.GetButtonDown ("Weapon Hotkey 4")){
			Equip (4);
		}

		#endregion
	
	} // end of Update





	IEnumerator holdToProne() {
		
		for (float timer = 0.5f; timer >= 0; timer -= Time.deltaTime)
			yield return 0;
		
		// Initiates prone
		stance = 2;
		//characterController.radius = 8;
		characterController.center = new Vector3 (0, 10.8f, 0);
		characterController.height = 21.6f;
		movementSpeed = crawlSpeed;
		
		// temporary visual interpretation of prone
		playerGraphic.transform.localScale = new Vector3 (1, 0.25f, 1);
		playerGraphic.transform.localPosition = new Vector3 (0, 0, 0);
		player.transform.position = new Vector3 (player.transform.position.x, player.transform.position.y + 2, player.transform.position.z);

		// Enables box collider
		proneCollider.enabled = true;

	} // end of holdToProne




	// Takes damage (uses sendMessage from other scripts
	void TakeDamage (float damage){
		
		health -= damage;
		
	}



} // End of Player_SCRIPT
