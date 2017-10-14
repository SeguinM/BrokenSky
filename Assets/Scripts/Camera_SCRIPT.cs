using UnityEngine;
using System.Collections;

public class Camera_SCRIPT : MonoBehaviour {


	// GUI setup
	float originalWidth = 1920f;          // workiing horizontal resolution
	float originalHeight = 1080f;         // working vertical resolution
	private Vector3 scale;               // scale vector
	public GUISkin guiSkin;              // plug in in the inspector


	// Variables
	public Texture2D lensFX;      // lens effect to be applied to camera






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
	

		if (lensFX) 
		{
			// draws lens effect
			GUI.DrawTexture (new Rect (0, 0, originalWidth, originalHeight), lensFX);
		} // end of if lensFX


		
		
		
		
		
		
		
		
		// Restores old GUI matrix
		GUI.matrix = svMat;
		
	} // End of OnGUI






	// Use this for initialization
	void Start () 
	{
	
	} // end of function start
	

	// Update is called once per frame
	void Update () {
	
	}
}
