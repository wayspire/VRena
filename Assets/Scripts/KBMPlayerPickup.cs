using UnityEngine;

public class KBMPlayerPickup : MonoBehaviour
{
	public float raycastDistance = 100f;
	public LayerMask raycastLayerMask;
	public GameObject weaponPos;
	Gun heldWeapon;

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (heldWeapon == null)
			{
				if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0)), out RaycastHit hit, raycastDistance, raycastLayerMask))
				{
					heldWeapon = hit.transform.GetComponent<Gun>();
					hit.transform.parent = weaponPos.transform;
					hit.transform.localPosition = Vector3.zero;
					hit.transform.localRotation = Quaternion.identity;
					hit.transform.GetComponent<Rigidbody>().isKinematic = true;
				}
			}
			else
			{
				//eldWeapon.serverFiring = true;
			}

		}

		if (Input.GetMouseButtonUp(0) && heldWeapon != null)
		{
			//heldWeapon.serverFiring = false;
		}
	}
}
