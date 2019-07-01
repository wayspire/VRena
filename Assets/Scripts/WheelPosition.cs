using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelPosition : MonoBehaviour
{
	public LayerMask mask;
	public float offset;
	RaycastHit hit;

	bool mine;

	private void Start()
	{
		mine = GetComponent<PhotonView>().IsMine;
	}

	private void LateUpdate()
	{
		if(mine)
		{
			Physics.Raycast(new Ray(transform.parent.position, Vector3.down), out hit, 10f, mask);
			transform.position = new Vector3(transform.position.x, hit.point.y + offset, transform.position.z);
		}
	}
}
