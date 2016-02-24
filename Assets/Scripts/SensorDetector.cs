using UnityEngine;
using System.Collections;

public class SensorDetector : MonoBehaviour 
{
	// Main game controller
	private PlayerController gAiCode;
	private enemyAI gAiCodeEnemy;

	//-------------------------------------------------------------------------
	void Start () 
	{
		// Get the parent script so we can call its functions from here
		gAiCode = transform.parent.gameObject.GetComponent<PlayerController>();
		gAiCodeEnemy = transform.parent.gameObject.GetComponent<enemyAI>();

		if (gAiCode != null) 
		{
			// Set our detection sphere's range
			GetComponent<SphereCollider> ().radius = gAiCode.maxDetectRange;
		}
		if (gAiCodeEnemy != null) 
		{
			// Set our detection sphere's range
			GetComponent<SphereCollider> ().radius = gAiCodeEnemy.maxDetectRange;
		}
	}
	
	//-------------------------------------------------------------------------
	void OnTriggerEnter(Collider other)
	{
		Debug.Log("SensorDetector: OnTriggerEnter called on tag: " + other.tag);

		if (gAiCode != null) 
		{
			if (other.gameObject.tag == "Player")
			{
				Debug.Log ("Player Detected!");
				gAiCode.SetTarget (other.gameObject);
			}
			else if (other.gameObject.CompareTag("Crystal"))
			{
				Debug.Log ("Crystal Detected!");
				gAiCode.SetTarget (other.gameObject);
			}
		}
		if (gAiCodeEnemy != null) 
		{
				gAiCodeEnemy.SetTarget (other.gameObject);
		}
	}
}