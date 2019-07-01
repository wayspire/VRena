using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollide : MonoBehaviour
{
	SphereCollider handCollider;
	public Rigidbody basketball;

	private void Start()
	{
		handCollider = GetComponent<SphereCollider>();
	}

	void Update()
    {
		if(OVRInput.GetDown(OVRInput.RawButton.A, OVRInput.Controller.RTouch))
		{
			handCollider.enabled = true;
		}
		if (OVRInput.GetUp(OVRInput.RawButton.A, OVRInput.Controller.RTouch))
		{
			handCollider.enabled = false;
		}
		if (OVRInput.GetDown(OVRInput.RawButton.B, OVRInput.Controller.RTouch))
		{
			basketball.transform.position = transform.position;
			basketball.velocity *= 0f;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if("Basketball".Equals(collision.gameObject.tag))
		{
			handCollider.enabled = false;
		}
	}
}
