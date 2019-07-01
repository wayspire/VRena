using Photon.Pun;
using UnityEngine;

public class LockPositionAndRotation : MonoBehaviour
{
	[Header("Lock Rotation:")]
	[SerializeField]
	bool x = true;
	[SerializeField]
	bool y = false;
	[SerializeField]
	bool z = true;

	public bool isLockingPos = true;
	public bool isLockingRot = true;
	public bool ignoreYPos = false;
	Vector3 savedPosition;
	Vector3 savedRotation;

	bool mine;

	private void Start()
	{
		mine = GetComponent<PhotonView>().IsMine;
	}

	public void LockPosition(Vector3 position)
	{
		transform.localPosition = position;
		Transform parent = transform.parent;
		transform.parent = parent.parent;
		savedPosition = transform.localPosition;
		transform.parent = parent;
		isLockingPos = true;
	}

	public void LockRotation()
	{
		savedPosition = transform.rotation.eulerAngles;
		isLockingRot = true;
	}

	void LateUpdate()
	{
		if(!mine)
		{
			return;
		}

		if(isLockingPos)
		{
			Transform parent = transform.parent;
			transform.parent = parent.parent;
			var newPos = new Vector3
			{
				x = parent.localPosition.x + savedPosition.x,
				z = parent.localPosition.z + savedPosition.z
			};

			if(!ignoreYPos)
			{
				newPos.y = parent.localPosition.y + savedPosition.y;
			}

			transform.localPosition = newPos;
			transform.parent = parent;
		}

		if(isLockingRot)
		{
			Vector3 newAngle = transform.rotation.eulerAngles;
			newAngle.x = x ?
							savedRotation.x : newAngle.x;
			newAngle.y = y ?
							savedRotation.y : newAngle.y;
			newAngle.z = z ?
							savedRotation.z : newAngle.z;
			transform.rotation = Quaternion.Euler(newAngle);
		}
	}
}