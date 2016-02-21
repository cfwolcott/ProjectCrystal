using UnityEngine;
using System.Collections;

public class RandomRotator : MonoBehaviour 
{
    public float tumble;
	public float force;

	// Use this for initialization
	void Start () 
    {
		GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * tumble;
		GetComponent<Rigidbody>().AddForce (Random.insideUnitSphere * force);
	}
}
