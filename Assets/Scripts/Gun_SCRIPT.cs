using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gun_SCRIPT: MonoBehaviour {

	/* to do list:
		-projectiles must send 'getperpetrator'
		-do bullet path vfx
		-Check and see if Projectile has projectile script on it
	 */


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                                                     VARIABLES                                                              //
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region variables

    // STATS
    public string name;          // Name of weapon

	// Enum to get gun behavior
	public enum GunBehavior 
	{ 
		SemiAutomatic, 
		Automatic, 
		ThreeRoundBurst
	} // end of enum GunBehavior
	public GunBehavior gunBehavior; // instantiate an enum menu for the inspector
	
	// Enum to get shot type
	public enum HitDetectionType
	{
		HitScan,
		Projectile,
		Artillery
	} // end of enum HitDetectionType
	public HitDetectionType hitDetectionType;  // instantiate the enum for the inspector

	public int bulletsPerShot;   // Number of 'bullets' fired per shot (ie 1 for most guns, 12 for a shotgun, etc)
	public float fireRate;        // delay (in seconds) between shots
	public int magCapacity;      // How many rounds are loaded at once
	public int totalCapacity;    // How many rounds can be carried in total
	public float damage;         // How much damage is dealt (per collision)
	public float accuracy;       // % out of 100 that is weapon's accuracy rating
	public float projectileSpeed;// Speed of projectile (if projectile weapon)
	public float impactForce;     // how much force is applied to impacted objects
	public int loadedAmmo;        // how many rounds are currently loaded in the mag
	public int reserveAmmo;       // how many extra rounds are available
	public int roundsPerShot;     // how many rounds are used per shot (separate from bullets per shot-- if bullets were the pellets, rounds would be the shell spent)
	public float reloadTime;      // how long it takes to reload the weapon
	public byte index;            // index of the weapon. Externally assigned
	public byte ignoreCollisionLayer; // index of the layer where you'd like to ignore collisions (like trigger volumes for example) 

	// NPC STUFF
	//public string npcTag = "Enemy";
	public float effectiveRange;    // effective range (in world units) for NPC to attack with this weapon
    


    // GAME OBJECTS
    public Rigidbody artilleryProjectile;  // Physical projectile that is launched (if projectile weapon)
	public GameObject projectile;          // regular projectile 
	public AudioClip shotSound;   // sound of gunshot
	public GameObject muzzleSocket; // Socket where gun is 'fired' from. This is where the shot trajectory is actually launched from.
	public GameObject muzzleFlashSocket; // socket where effect to play is located from. Generally put this right at the end of the barrel facing forwards
	public GameObject actor;      // actor weapon is equipped to
	public ParticleSystem muzzleFlash;   // muzzle flash particle effect to play

	// GUI stuff. Plug in if you'd like, otherwise ignore
	public Texture2D guiImage; // the weapon's gui image representation. 
	public Texture2D crosshairImage;  // weapon's crosshair to display. Currently only supports a static (non-bloom) image.

	// ANIMATIONS. Plug them in here if you'd like this script to handle them, but this is optional.
	public Animation fireAnim;    // Firing animation.
	public Animation meleeAnim;   // melee animation
	public Animation reloadAnim;  // reload animation


	// INTERNAL VARIABLES
	//--------------------------------------------------------------------------
	private float nextFire = 0.0f;    // time until gun can fire again
	private float minSpread = 0.000f;     // Minimum amount of spread (in degrees)
	private float maxSpread = 2.000f;    // maximum amount of spread (in degrees) -- feel free to play with this, but not a good idea to go more than a degree higher or lower
	private float shotSpread;         // This number is 100 - accuracy
	private Vector3 shotPlacement;    // shot placement
	private GameObject hitObject;     // object that is hit
	private bool isActive = false;            // is this the currently active weapon?
	private bool isTouchable = false;      // is weapon touchable? (pick-up)
	private bool amIShooting = false;        // is gun shooting
	private bool isFirstShot = true;        // is this the first shot since the trigger was pulled
	private bool overrideSemiAuto = false;  // override semi-auto for NPC?

	private ParticleSystem particleInstance; // instantiated instance of particle instance.

	// Debug stuff
	private int numberOfShots = 0;

	// GUI stuff
	float originalWidth = 1920f;
	float originalHeight = 1080f;
	private Vector3 scale;

	#endregion




    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                                                     PUBLIC FUNCTIONS                                                       //
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region public

	// Spam this as much as you wish, gun will only fire when it is supposed to. (ie. cooldown after each shot)
    public void PullTrigger(bool _overrideSemiAuto = false)
	{
		overrideSemiAuto = _overrideSemiAuto;
		amIShooting = true;
	} // end of function Shoot

	public void ReleaseTrigger()
	{
		overrideSemiAuto = false;
		amIShooting = false;
		isFirstShot = true;
	} // end of function ReleaseTrigger

	// Makes weapon the currently active weapon
    public void Equip()
    {
        if (actor.tag == "Player")
        {
            isActive = true;
        }

        // removes rigid body if attached
        if (GetComponent<Rigidbody>())
        {
            Destroy(GetComponent<Rigidbody>());
        }
        // disables physics collider on gun while equipped
		if (GetComponent<Collider>())
        	GetComponent<Collider>().enabled = false;
    } // end of Equip

    public void Reload()
    {
		if (Time.time < nextFire || loadedAmmo >= magCapacity)
						return;

        nextFire = Time.time + reloadTime;

        // play reload animation here
        StartCoroutine("loadRounds");

		// plays Reload animation if it's present.
		if (reloadAnim != null)
			reloadAnim.Play ();
    }

    // happens when the fire button is pressed
    public void Fire()
    {

        // drops loadedAmmo count by roundsPerShot
        loadedAmmo = loadedAmmo - roundsPerShot;

		// plays 'fire' anim if present
		if (fireAnim != null)
			fireAnim.Play ();

		PlayParticleEffects ();

	    // Filters based on hitType
	    if (hitDetectionType == HitDetectionType.HitScan)
				FireHitscan();

	    else if (hitDetectionType == HitDetectionType.Artillery)
				FireArtillery();

		else if (hitDetectionType == HitDetectionType.Projectile)
			FireProjectile();

		isFirstShot = false; // keeps track of whether or not it was the first shot since trigger pull.

        // plays the attached gunshot sound if one is present
        if (shotSound)
        {
            AudioSource.PlayClipAtPoint(shotSound, muzzleSocket.transform.position, 1f);
            // animation.Play ("Fire_ANIM");
        }

    } // end of void Fire

	public void Melee()
	{
		// DO STUFF
		if (meleeAnim != null)
			meleeAnim.Play ();
	} // end of function Melee

    public void DropMe()
    {

        // unparents from actor
        transform.parent = null;

        // enables colliders and rigidbody
        GetComponent<Collider>().enabled = true;

        if (!GetComponent<Rigidbody>())
        {
            gameObject.AddComponent<Rigidbody>();
        }

        // enables pick-up
        isTouchable = true;

    } // end of void drop me

	#endregion




    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                                                    PRIVATE FUNCTIONS                                                       //
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region private

    // Delays gunshots based on the fire rate
    private void FireDelay()
    {

        if (gunBehavior == GunBehavior.Automatic)
        {
            if (amIShooting && Time.time > nextFire&& loadedAmmo >= roundsPerShot)
            {

                // resets the firing delay and fires a bullet.
                nextFire = Time.time + fireRate;
                Fire();

            } // end of fire if ready

            else if (amIShooting && Time.time > nextFire && loadedAmmo < roundsPerShot)
                Reload();

        } // end of if is automatic

        // semi-auto
        else if (gunBehavior == GunBehavior.SemiAutomatic)
        {
            // override semi-auto?
            if ((overrideSemiAuto || isFirstShot) && amIShooting && loadedAmmo >= roundsPerShot && Time.time > nextFire)
            {
                // resets the firing delay and fires a bullet.
                nextFire = Time.time + fireRate;
                Fire();
            } // end of if statement

            if (amIShooting && Time.time > nextFire && loadedAmmo < roundsPerShot)
            {
                Reload();
            } // end of reload if empty

        } // end of semi

    } // end of void FireDelay

    // accessed from Reload function. After a delay, sets the magazine to have the max available rounds.
    IEnumerator loadRounds()
    {

        for (float timer = reloadTime; timer >= 0; timer -= Time.deltaTime)
            yield return 0;

        int roundsLoaded = 0;

        if ((magCapacity - loadedAmmo) <= reserveAmmo)
        {
            roundsLoaded = magCapacity - loadedAmmo;
        }

        else if ((magCapacity - loadedAmmo) > reserveAmmo)
        {
            roundsLoaded = reserveAmmo;
        }

        loadedAmmo += roundsLoaded;
        reserveAmmo -= roundsLoaded;

    } // end of loadRounds

	// calculates direction to fire in
	private Vector3 GetBulletPath()
	{
		// return var
		Vector3 ret;

		// creates temp variables for shot variance and placement
		float deviationX = maxSpread / 100 * (Random.Range(-shotSpread, shotSpread));
		float deviationY = maxSpread / 100 * (Random.Range(-shotSpread, shotSpread));
		float randomAngle = Random.Range(0, 2 * Mathf.PI);
		ret = new Vector3(
			deviationX * Mathf.Cos(randomAngle),
			deviationY * Mathf.Sin(randomAngle),
			10);
		
		// normalizes
		ret = muzzleSocket.transform.TransformDirection(ret.normalized);

		return ret;
	} // end of function GetBulletPath

	private void FireHitscan()
	{
		// layer mask
		int layerMask = 1 << ignoreCollisionLayer;         // bit shifts to layer 9 (Triggers) (I fucking hate bitshifting)
		layerMask = ~layerMask;          // inverts layermask

		// calculates each bullet fragment individually. (if this is more than 1, think of it like shotgun blast pellets each firing in different directions)
		for (int i = 0; i < bulletsPerShot; i++)
		{
			Vector3 direction = GetBulletPath ();
			
			RaycastHit hit;
			Ray ray = new Ray(muzzleSocket.transform.position, direction);
			
			// Debug, so you can see bullet path in editor
			Debug.DrawRay(muzzleSocket.transform.position, direction * effectiveRange, Color.cyan, 2);
			
			//Checks to see if ray hits
			if (Physics.Raycast(ray, out hit, effectiveRange, layerMask))
			{
				// checks to see if collider hit is not null
				if (hit.collider != null)
				{
					// assigns hitObject
					hitObject = hit.collider.transform.gameObject;
					
					// Temporary debugging
					Debug.Log("Gun hit " + hitObject.name);
					
					// Only does this if struck object is a Dynamic Scenery or Actor
					if (hitObject.transform.root.name == "ACTORS" || hitObject.transform.root.name == "DYNAMIC SCENERY")
					{
						
						// Sends message telling hit object it is to receive damage, and who the perpetrator was
						if (hitObject != null)
						{
							hitObject.SendMessage("GetPerpetrator", actor, SendMessageOptions.DontRequireReceiver);
							hitObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
						} // end of if statement
						
						
						// adds impact force to the hit object if it's a Dynamic Scenery
						if (hitObject.GetComponent<Rigidbody>())
						{
							hitObject.GetComponent<Rigidbody>().AddForce(direction * impactForce);
						}
						
					} // end of if actors or dynamic scenery
					
				} // end of if hit collider isn't null
			} // end of physics raycast

		} // end of for loop
	} // end of function FireHitscan

	private void FireArtillery()
	{

		// null check 
		if (artilleryProjectile == null)
				throw new UnassignedReferenceException (name + " artillery projectile is not assigned. Please assign in the inspector.");

		// repeats for each bullet. 
		for (int i = 0; i < bulletsPerShot; i++)
		{
			Vector3 direction = GetBulletPath ();      // direction to launch projectile

			// instantiates bullet and points in proper direction
			Rigidbody bullet = Instantiate (artilleryProjectile, muzzleSocket.transform.position, muzzleSocket.transform.rotation) as Rigidbody;

			if (bullet.GetComponent<ArtilleryProjectile> ())
					bullet.GetComponent<ArtilleryProjectile> ().setData (damage, impactForce, direction, projectileSpeed);

			// FIRING PHOTON TORPEDOES
			bullet.GetComponent<Rigidbody>().AddForce(direction * projectileSpeed);
		} // end of for loop
	} // end f function FireProjectile

	private void FireProjectile()
	{
		if (projectile == null)
				throw new UnassignedReferenceException (name + " projectile is not assigned. Please assign in the inspector.");

		// repeats for each projectile
		for (int i = 0; i < bulletsPerShot; i++)
		{
			Vector3 direction = GetBulletPath ();

			// instantiate bullet and points in proper direction
			GameObject bullet = Instantiate (projectile, muzzleSocket.transform.position, muzzleSocket.transform.rotation) as GameObject;

			if (bullet.GetComponent<Projectile> ())
				bullet.GetComponent<Projectile> ().setData (damage, impactForce, direction, projectileSpeed);
		} // end of for loop
	}// end of function FireProjectile

	private void PlayParticleEffects()
	{
		GameObject sckt = muzzleFlashSocket != null ? muzzleFlashSocket : muzzleSocket; 

		// no point in continuing if there's no place to launch an effect from.
		if (sckt == null)
				return;

		// muzzle flash
		if (muzzleFlash != null)
		{
			// first run?
			if (particleInstance == null)
				particleInstance = Instantiate (muzzleFlash, sckt.transform.position, sckt.transform.rotation) as ParticleSystem;
			else
			{
				particleInstance.transform.position = sckt.transform.position; // resets position
				particleInstance.transform.rotation = sckt.transform.rotation; // resets rotation
				particleInstance.Play();
			} // end of else statement

		} // end of if statement
	} // end of function PlayParticleEffects

	#endregion




    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                                                     UNITY-CALLED FUNCTIONS                                                 //
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region unity

    // Called on init
    public void Start ()
    {
		// error check the accuracy value
		if (accuracy < 0)
			accuracy = 0;
		if (accuracy > 100)
			accuracy = 100;
		// Sets the shot spread
		shotSpread = 100.000f - accuracy;
	} // end of void Start

	
	// Update is called once per frame
	public void Update () {
		if (amIShooting == true)
			FireDelay ();
	} // end of void update


	public void OnCollisionEnter (Collision collision){

		// checks to see if collided entity is player AND object is touchable
		if (collision.gameObject.tag == "Player" && isTouchable) {

			Debug.Log ("Collided with player");
			// temporarily adds to inventory slot 4
			collision.gameObject.GetComponent<Player_SCRIPT>().weaponArray[3] = GameObject.Instantiate (gameObject) as GameObject;
			Destroy (gameObject);

		} // end of if collider is player and is touchable

	} // end of on collision enter

     //called when this object is destroyed by Unity.
    public void OnDestroy()
    {

        Debug.Log("Weapon destroyed successfully");

		if (actor == null) return;
		if (!gameObject) return;

        // saves weapon info back to inventory if it's the player
        if (actor.tag == "Player")
        {

            // Assigns local variable as indexed weapon's script
            Gun_SCRIPT indexedWeapon = actor.GetComponent<Player_SCRIPT>().weaponArray[index].GetComponent<Gun_SCRIPT>();

            // Debugs name back to ensure it's working
            Debug.Log(indexedWeapon.name);

            // info save
            indexedWeapon.loadedAmmo = loadedAmmo;
            indexedWeapon.reserveAmmo = reserveAmmo;

        }

        // ensures gun is no longer active
        isActive = false;

    } // end of on destroy

	#endregion

} // end of class Gun_CLASS
