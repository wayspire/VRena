using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{

	Renderer grenadeRenderer;
	OVRGrabbable grabbableScript;
	public float detonateTime = 3f;
	public bool thrown = false;
	float startTime;
	bool grabbedByRightHand = false;
	bool grabbedByLeftHand = false;
	public float explosionRadius = 10f;
	public float explosionForce = 10f;
	void Start()
    {
		grenadeRenderer = GetComponent<Renderer>();
		grabbableScript = GetComponent<OVRGrabbable>();
	}

	private void Update()
	{
		if(grabbableScript.isGrabbed)
		{
			Debug.Log("Grabbed");
			OVRGrabber grabbyMitt = grabbableScript.grabbedBy;
			if(grabbyMitt.name == "AvatarGrabberRight")
			{
				Debug.Log("Right hand");
				grabbedByRightHand = true;
			}
			else if(grabbyMitt.name == "AvatarGrabberLeft")
			{
				Debug.Log("Left hand");
				grabbedByLeftHand = true;
			}
			else
			{
				Debug.LogError("Grabby mitt isn't a hand??!!11!!");
			}
		}
		else
		{
			grabbedByRightHand = false;
			grabbedByLeftHand = false;
		}

		if (grabbedByRightHand)
		{
			Debug.Log("Testing right hand");
			Debug.Log(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch));
			if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
			{
				Debug.Log("Tick tock tick tock");
				ActivateGrenade();
			}
		}
		if (grabbedByLeftHand)
		{
			Debug.Log("Testing left hand");
			Debug.Log(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch));
			if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
			{
				ActivateGrenade();
			}
		}
	}

	void ActivateGrenade()
	{
		startTime = Time.time;
		thrown = true;
	}

	void FixedUpdate()
	{
		if(thrown)
		{
			grenadeRenderer.material.color = new Color(0.9f, 0.1f, 0f, 1f);
			if(Time.time >= startTime + detonateTime)
			{
				grenadeRenderer.material.color = Color.red;
				transform.localScale *= 1.01f;
				if(transform.localScale.sqrMagnitude >= 100f)
				{
					foreach(Collider c in Physics.OverlapSphere(transform.position, explosionRadius))
					{
						if(c.GetComponent<Rigidbody>() != null)
						{
							c.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, c.transform.position, explosionRadius);
						}
					}
					Debug.Log("Boom!");
					Destroy(gameObject);
				}
			}
		}
	}
	public bool isStuck = false;
	private void OnCollisionEnter(Collision collision)
	{
		if(collision.collider.tag == "Tables" || collision.collider.tag == "Toys" && !isStuck)
		{
			var stickyBomb = gameObject.AddComponent<FixedJoint>();
			stickyBomb.connectedBody = collision.rigidbody;
			isStuck = true;
		}
	}
}
