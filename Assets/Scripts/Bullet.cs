using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public float damage;
	public GameObject bulletHolePrefab;

	private void OnCollisionEnter(Collision collision)
	{
		SpawnBulletHole(bulletHolePrefab, collision.GetContact(0).point, collision.GetContact(0).normal, collision.gameObject.transform);
		HealthObject ho = collision.gameObject.GetComponent<HealthObject>();
		if (ho != null)
		{
			//TODO FIX ho.Damage(damage);
		}
		Destroy(gameObject);
	}

	void SpawnBulletHole(GameObject bulletHolePrefab, Vector3 position, Vector3 forward, Transform parent)
	{
		GameObject bulletHoleDecal = Instantiate<GameObject>(bulletHolePrefab, position, Quaternion.identity);
		bulletHoleDecal.transform.forward = forward;
		bulletHoleDecal.transform.Rotate(90, 0, 0);
		bulletHoleDecal.transform.parent = parent;
		bulletHoleDecal.transform.Translate(new Vector3(0f, 0.0001f, 0f), Space.Self);
	}

}
