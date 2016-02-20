using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
    // Our forward speed. Acceleration I guess.
    public float speed;
    // Our rotate speed.
    public float rotationSpeed;

    public GameObject shot;
    public GameObject engines;
    public GameObject smokeTrail;
    public Transform shotSpawn;
    public float fireRate;
    private float nextFire;

    private Rigidbody rigidBody;
    private float speedModifier;
    // The ships smoke trail particle emitter
    private ParticleSystem pe;
	private int roll;

    //-------------------------------------------------------------------------
    // Use this for initialization
    void Start () 
    {
        rigidBody = GetComponent<Rigidbody>();
        pe = smokeTrail.GetComponent<ParticleSystem>();
		roll = 0;
    }

    //-------------------------------------------------------------------------
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            //GameObject clone = 
			GameObject shotObject = (GameObject)Instantiate(shot, shotSpawn.position, shotSpawn.rotation); // as GameObject;

			shotObject.GetComponent<Mover> ().SetParentSpeed (transform.GetComponent<Rigidbody> ().velocity);

            GetComponent<AudioSource>().Play();
        }
    }

    //-------------------------------------------------------------------------
	void FixedUpdate () 
    {
        float yaw = Input.GetAxisRaw("Horizontal");
        float thrust = Input.GetAxisRaw("Vertical");
		float thrustVector;
		float thrustScale;
		// max roll angle is a function of yaw and speed
		int roll_max = (int)(-yaw * rigidBody.velocity.magnitude * 8);

        // Show engine thrust only when thrusting forward or turning
        //Debug.Log("thrust: " + thrust);
		engines.SetActive((thrust > 0) || (yaw != 0));

		if (thrust > 0) 
		{
			// thrusting...
			// Turn on smoke
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
			if (pe.maxParticles > 0)
				pe.maxParticles--;
			// Apply drag
			speedModifier = 0.25f;
			// Flame (if any) will be pointed 90 deg left or right
			thrustVector = (yaw > 0) ? -90 : 90;
			// Calculate flame size based on yaw
			thrustScale = 0.5f + ((yaw > 0) ? yaw : -yaw);
		}

		// point and scale engine flames
		engines.transform.localEulerAngles = new Vector3 (90, 90 + thrustVector, 0);
		engines.transform.localScale = new Vector3 (thrustScale, thrustScale, 1);

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
}
