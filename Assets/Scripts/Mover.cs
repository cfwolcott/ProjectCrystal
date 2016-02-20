using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour 
{
    public float speed;

	//-------------------------------------------------------------------------
	public void SetParentSpeed( Vector3 parentSpeed )
	{
		GetComponent<Rigidbody>().velocity = (transform.forward * speed) + parentSpeed;
	}
}
