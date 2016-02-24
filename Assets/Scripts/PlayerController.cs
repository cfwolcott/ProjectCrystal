using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour 
{

    // Our forward speed. Acceleration I guess.
    public float speed;
    // Our rotate speed.
    public float rotationSpeed;

    // Weapons
    public string ourWeaponTag;
    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;
    private float nextFire;
    
    // Engines
    public GameObject engines;
	private int roll;
    public GameObject smokeTrail;
    // The ships smoke trail particle emitter
    private ParticleSystem pe;

    // Ship life parameters
    public GameObject explosion;
	public GameObject spark;
    public float hitsToDestroy;

    // Cargo Pickups
    public int maxCargoLoadCount;
    private int cargoLoadCount = 0;
    public GameObject cargoObject;
    public Transform cargoDumpSpawn;

    private Rigidbody rigidBody;
    private float speedModifier;

    // Sound clips
    private AudioSource audioWeapon;
    private AudioSource audioCrystalPickup;

    // Main game controller
    private GameController gGameController;


	private int eController = 0;
	private Transform targetPlayer; 
	private Transform targetCrystal; 
	public float maxDetectRange = 20;
	public float maxWeaponRange = 15;

	private Transform myTransform;
	private bool bAIFire = false;
	private bool bAIFire2 = false;
	private bool bAIDump = false;
	private float AI_yaw = 0;
	private float AI_thrust = 0;


	//-------------------------------------------------------------------------
	void Awake()
	{
		myTransform = transform;
	}

    //-------------------------------------------------------------------------
    // Use this for initialization
    void Start () 
    {
        GameObject gameControllerObject = GameObject.FindGameObjectWithTag("GameController");
        if (gameControllerObject != null)
        {
            gGameController = gameControllerObject.GetComponent<GameController>();
        }

        if (gGameController == null)
        {
            Debug.Log("Cannot find 'GameController' script");
        }

        rigidBody = GetComponent<Rigidbody>();
		if (smokeTrail)
			pe = smokeTrail.GetComponent<ParticleSystem> ();
		else
			pe = null;
		roll = 0;
		
        gGameController.UI_SetSheildLevelMax(hitsToDestroy);
        gGameController.UI_SetSheildLevel(hitsToDestroy);

        gGameController.UI_SetCargoLevelMax(maxCargoLoadCount);
        gGameController.UI_SetCargoLevel(0);

        // Assign audio sources
        AudioSource[] audioClips = GetComponents<AudioSource>();

        // Load clips. These are in order that they appear in the inspector
        audioWeapon = audioClips[0];
        audioCrystalPickup = audioClips[1];

		// Set controller entity for this player
		if (CompareTag ("Enemy"))
			eController = 1;
		else
			eController = 0;
		targetPlayer = null;
		targetCrystal = null;
    }

    //-------------------------------------------------------------------------
    void Update()
    {
		bool bFire = false;
		bool bFire2 = false;
		bool bCargoDump = false;

		switch (eController) 
		{
			case 0:
				bFire = Input.GetButton ("Fire1");
				bFire2 = Input.GetButton ("Fire2");
				bCargoDump = Input.GetKeyDown (KeyCode.C);
				break;

			case 1:
				bFire = bAIFire;
				bFire2 = bAIFire2;
				bCargoDump = bAIDump;
				bAIFire = bAIFire2 = bAIDump = false;
				break;
		}

		// Shoot a bolt
        if (bFire && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
			GameObject shotObject = (GameObject)Instantiate(shot, shotSpawn.position, shotSpawn.rotation); // as GameObject;

			shotObject.GetComponent<Mover> ().SetParentSpeed (transform.GetComponent<Rigidbody> ().velocity);

            audioWeapon.Play();
        }

		// Shoot out the mining laser
        if (Input.GetButton("Fire2"))
        {
            Debug.Log("Fire2");
        }

        // Defined ship key controls
        // C - Cargo, dump crystals that are in the cargo hold
        if (bCargoDump)
        {
            DumpCrystal(cargoDumpSpawn);
        }
    }

    //-------------------------------------------------------------------------
	void FixedUpdate () 
    {
		float yaw = 0;
		float thrust = 0;
		float thrustVector;
		float thrustScale;

		switch (eController) 
		{
			case 0: //Player1
				yaw = Input.GetAxisRaw ("Horizontal"); 
				thrust = Input.GetAxisRaw ("Vertical");
				break;

			case 1: // AI
				AI_Controller(out yaw, out thrust);
				break;
		}

		// max roll angle is a function of yaw and speed
		int roll_max = (int)(-yaw * rigidBody.velocity.magnitude * 8);

        // Show engine thrust only when thrusting forward or turning
        //Debug.Log("thrust: " + thrust);
		if (engines)
			engines.SetActive((thrust > 0) || (yaw != 0));

		if (thrust > 0) 
		{
			// thrusting...
			// Turn on smoke
			if (pe)
				pe.maxParticles = 50;
			// Turn off drag
			speedModifier = 1.0f;
			// Calculate flame angle and size
			thrustVector = yaw * -45.0f;
			thrustScale = thrust * 4.0f;
		}
		else 
		{
			// no thrust (or reverse thrust)...
			// Reduce smoke until gone
			if (pe)
			{
				if (pe.maxParticles > 0)
					pe.maxParticles--;
			}
			// Apply drag
			speedModifier = 0.25f;
			// Flame (if any) will be pointed 90 deg left or right
			thrustVector = (yaw > 0) ? -90 : 90;
			// Calculate flame size based on yaw
			thrustScale = 0.5f + ((yaw > 0) ? yaw : -yaw);
		}

		if (engines) 
		{
			// point and scale engine flames
			engines.transform.localEulerAngles = new Vector3 (90, 90 + thrustVector, 0);
			engines.transform.localScale = new Vector3 (thrustScale, thrustScale, 1);
		}

		// remove previous roll component
		transform.Rotate(0, 0, (float)-roll);

		// Apply turning as a rotation.
		transform.Rotate(0, yaw * Time.deltaTime * rotationSpeed, 0);

		// Add in new roll component
		roll += (roll < roll_max) ? 1 : (roll > roll_max) ? -1 : 0;
		transform.Rotate(0, 0, (float)roll);

       // Apply thrust as a force.
		rigidBody.AddForce(transform.TransformDirection(0, 0, thrust * speed * speedModifier));
	}

    //-------------------------------------------------------------------------
    // This function does the same as the "DestroyByContact" but is less generic. It is needed here
    // so the health bar can be updated and player objects can be respawned
    void OnTriggerEnter(Collider other)
    {
        //if (other.tag == "Bolt")
        //    Debug.Log("OnTriggerEnter: Bolt");
        //if (other.tag == "BoltEnemy")
        //    Debug.Log("OnTriggerEnter: BoltEnemy");

        if ((other.tag == "Bolt" || other.tag == "BoltEnemy") && other.tag != ourWeaponTag)
        {
            hitsToDestroy--;

            gGameController.UI_SetSheildLevel(hitsToDestroy);

			if (hitsToDestroy == 0) {
				if (explosion != null) {
					Instantiate (explosion, transform.position, transform.rotation);
				}

				Destroy (gameObject);
				if (CompareTag("Player"))
					gGameController.PlayerDead ();
			}
			else if (spark != null)
			{
				Instantiate (spark, other.gameObject.transform.position, other.gameObject.transform.rotation);
			}
			Destroy(other.gameObject);
       }
    }

    //-------------------------------------------------------------------------
    void OnCollisionEnter(Collision collision)
    {
		Debug.Log("Player: OnCollisionEnter called on tag: " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Crystal")
        {
            if (cargoLoadCount < maxCargoLoadCount)
            {
                cargoLoadCount++;
                gGameController.UI_SetCargoLevel(cargoLoadCount);
                Destroy(collision.gameObject);
                audioCrystalPickup.Play();
            }
        }
    }

    //-------------------------------------------------------------------------
    void DumpCrystal(Transform spawnTransform)
    {
        if (cargoLoadCount > 0 && cargoObject != null)
        {
            cargoLoadCount--;
            gGameController.UI_SetCargoLevel(cargoLoadCount);
            GameObject xtal = (GameObject)Instantiate(cargoObject, spawnTransform.position, spawnTransform.rotation);
            xtal.GetComponent<Rigidbody>().velocity = transform.forward * -2.0f;
        }
    }

	void AI_Controller(out float yaw, out float thrust)
	{
		Transform target = null;
		float distanceToTarget = 0;
		bool bArmed = false;
		yaw = 0;
		thrust = 0;

		if (targetPlayer != null) {
			distanceToTarget = Vector3.Distance (targetPlayer.position, myTransform.position);
			if (distanceToTarget < (maxDetectRange * 1.2f)) {
				target = targetPlayer.transform;
				bArmed = true;
			}
		}
		if ((target == null) && (targetCrystal != null) && (cargoLoadCount < maxCargoLoadCount)) {
			target = targetCrystal.transform;
			distanceToTarget = Vector3.Distance (targetCrystal.position, myTransform.position);
		}

		if (null != target)
		{
			Debug.DrawLine(target.position, myTransform.position, Color.red);

			// Detect and Follow logic
			Vector3 targetVector = target.position - myTransform.position;
			Quaternion q = Quaternion.LookRotation (targetVector);
			yaw = (int)(q.eulerAngles.y - transform.localRotation.eulerAngles.y);
			// normalize from +/-360 to +/-180;
			if (yaw > 180)
				yaw -= 360;
			if (yaw < -180)
				yaw += 360;

			if ((yaw < 5) && (yaw > -5))
				bAIFire = bArmed;
			
			yaw = yaw / 30;
			if (yaw > 1)
				yaw = 1;
			else if (yaw < -1)
				yaw = -1;

			thrust = distanceToTarget / 10;
			if (bArmed && (thrust < 0.5f))
				thrust = 0;
			if (thrust > 1)
				thrust = 1;
		}
		else
		{
			AI_yaw += Random.Range (-.01f, .01f);
			if (AI_yaw < 0)
				AI_yaw = 0;
			if (AI_yaw > 1)
				AI_yaw = 1;
			AI_thrust += Random.Range (-.02f, .02f);
			if (AI_thrust < 0)
				AI_thrust = 0;
			if (AI_thrust > 0.5f)
				AI_thrust = 0.5f;
			//wander here
			yaw = AI_yaw;
			thrust = AI_thrust;
		}
	}

	public void SetTarget(GameObject newTarget)
	{
		if (newTarget.CompareTag ("Player")) {
			targetPlayer = newTarget.transform;
		}
		else if (newTarget.CompareTag ("Crystal")){
			targetCrystal = newTarget.transform;
		}
	}
}
