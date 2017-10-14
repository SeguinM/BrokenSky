using UnityEngine;
using System.Collections;

public class GUI_SCRIPT : MonoBehaviour {


	// GUI setup
	float originalWidth = 1920f;          // workiing horizontal resolution
	float originalHeight = 1080f;         // working vertical resolution
	private Vector3 scale;               // scale vector
	public GUISkin guiSkin;              // plug in in the inspector

	// camera Variables
	public bool cinemaMode = false;

	// GUI Textures
	public Texture2D healthBarBase_gui;
	public Texture2D healthBar_gui;
	public Texture2D energyBar_gui;
	public Texture2D itemWindow_gui;
	public Gun_SCRIPT equippedWepScript;    // externally assigned via Player_SCRIPT
	public Texture2D equippedWep_gui;       // externally assigned via Gun_SCRIPT
	public Texture2D nameplate_gui;         // nameplate (usually Sera's)

	// Hookups
	public Player_SCRIPT playerScript; 



	// On GUI method
	void OnGUI () {
		
		// Artificially sets the resolution to originalWidth by originalHeight
		scale.x = (Screen.width / originalWidth);
		scale.y = (Screen.height / originalHeight);
		scale.z = 1;
		var svMat = GUI.matrix; // saves current GUI matrix
		
		//substitute matrix - only scale is altered from standard
		GUI.matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, scale);

		// assigns the GUI a custom skin
		GUI.skin = guiSkin;
		
		
		// DO STUFF HERE


		// Only draws if cinematic mode is off.
		if (!cinemaMode) {

			// Draws crosshair if in FP mode
			if (playerScript.cameraType == Player_SCRIPT.CameraMode.FirstPerson){

				// draws crosshair on the screen.
				if (equippedWepScript.crosshairImage != null)
				{
					float xMin = (originalWidth / 2) - (equippedWepScript.crosshairImage.width / 2);
					float yMin = (originalHeight / 2) - (equippedWepScript.crosshairImage.height / 2);
					GUI.DrawTexture (new Rect(xMin, yMin, equippedWepScript.crosshairImage.width, equippedWepScript.crosshairImage.height), equippedWepScript.crosshairImage);
				} // end of if statement

			}


			// HEALTH BAR
			// Draws the health bar base
			GUI.DrawTexture (new Rect (45, 100, healthBarBase_gui.width, healthBarBase_gui.height), healthBarBase_gui);

			// figures out stamina bar's width
			float staminaBarWidth = energyBar_gui.width / 100.00f * playerScript.stamina;
			// Draws the energy bar
			GUI.DrawTexture (new Rect (48, 160, staminaBarWidth, energyBar_gui.height), energyBar_gui);

			// Figures out health bar's width
			float healthBarWidth = healthBar_gui.width / 100.00f * playerScript.health;
			// Draws the health bar
			GUI.DrawTexture (new Rect (48, 142, healthBarWidth, healthBar_gui.height), healthBar_gui);
			// Draws nameplate
			GUI.DrawTexture (new Rect(45, 100, nameplate_gui.width, nameplate_gui.height), nameplate_gui);


			// WEAPON WINDOW
			// Draws weapon graphic
			if (equippedWep_gui != null) {
				// draws window
				GUI.DrawTexture (new Rect (originalWidth - 345, originalHeight - 245, itemWindow_gui.width, itemWindow_gui.height), itemWindow_gui);
				// Draws gun icon
				GUI.DrawTexture (new Rect ( originalWidth - 345, originalHeight - 245, itemWindow_gui.width, itemWindow_gui.height), equippedWep_gui);
				// Draws ammo count
				// changes alignment to the left
				guiSkin.label.alignment = TextAnchor.LowerRight;
				// ammo count
				GUI.Label (new Rect (originalWidth - 345, originalHeight - 245, itemWindow_gui.width-5, itemWindow_gui.height), equippedWepScript.loadedAmmo + " / " + equippedWepScript.reserveAmmo);
			}

			// ITEM WINDOW
			GUI.DrawTexture (new Rect (45, originalHeight - 245, itemWindow_gui.width, itemWindow_gui.height), itemWindow_gui);


		} // end of if !cinemaMode






		
		
		// Restores old GUI matrix
		GUI.matrix = svMat;
		
	} // End of OnGUI


	// Start function
	void Start () {

		playerScript = GameObject.FindWithTag ("Player").GetComponent<Player_SCRIPT>();

	} // end of Start
	
} // End of GUI_SCRIPT
