// This was copied from Gun.cs so I could make edits without messing anything up. -Wired

using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Gun_Burst : OVRGrabbable
{
	OVRGrabbable grabScript;
	public bool isProjectile;
	public float raycastDistance;
	public LayerMask hitMask;
	public Rigidbody bulletPrefab;
	public OVRGrabber leftHand;
	public OVRGrabber rightHand;
	public float bulletSpeed;
	public float fireRate;
	bool canFire = true;
	float timeSinceLastFire = 0;
	public Transform barrelTip;
	AudioSource fireSound;
	public GameObject bulletHolePrefab;
	public LineRenderer rayEffectPrefab;
	public float rayWidth;
	public float rayCastHitForce;
	OVRInput.RawAxis1D triggerFinger;
	bool isRightHand;
	readonly OVRHapticsClip hapticsClip;
	[Range(0, 1)]
	public float hapticsStrength = 1f;
	public float damage = 1f;
	public bool serverFiring = false;
	PhotonView photonView;
	GameObject grabbedGun;

	public bool hasAmmoDisplay;             // Check if gun has a physical ammo display for the player
	public Text tens, ones;                 // Each digit in the physical ammo display

	public float burstRate;					// Time (in seconds) between each bullet in a burst

	public bool unlimitedAmmo;              // Check if gun cannot run out of ammo
	public int ammoStart = 1;               // Starting amount of ammo if any
	int currentAmmo;                        // The gun's current ammo count

	Animator anims;                         // Controls each gun's animation

	protected override void Start()
	{
		base.Start();
		fireSound = GetComponent<AudioSource>();
		grabScript = GetComponent<OVRGrabbable>();
		photonView = GetComponent<PhotonView>();
		anims = GetComponent<Animator>();

		currentAmmo = ammoStart;                        // Initialize current ammo to start at maximum
		if (hasAmmoDisplay)
			ChangeAmmo(currentAmmo);
	}

	void Update()
	{
		if (Input.GetButtonDown("Fire1"))               // This is for testing without VR
		{
			serverFiring = true;
		}

		if (serverFiring && canFire && (currentAmmo > 0 || unlimitedAmmo))
		{
			StartCoroutine(FireBurst());
		}
		else
		{
			// TODO: play unable-to-fire sound
		}

		if (grabScript.isGrabbed)
		{
			if (OVRInput.Get(triggerFinger) > 0 && canFire)
			{
				photonView.RPC("FireBullet", PhotonNetwork.MasterClient);
				if (isRightHand)
				{
					PlayHaptics(hapticsStrength, 0.9f / fireRate, OVRInput.Controller.RTouch);
				}
				else
				{
					PlayHaptics(hapticsStrength, 0.9f / fireRate, OVRInput.Controller.LTouch);
				}
			}
			else
			{
				Debug.Log("Cant fire");
			}
		}
	}

	public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
	{
		base.GrabBegin(hand, grabPoint);
		rightHand = GameObject.FindGameObjectWithTag("RightHand").GetComponent<OVRGrabber>();
		leftHand = GameObject.FindGameObjectWithTag("LeftHand").GetComponent<OVRGrabber>();
		grabbedGun = grabPoint.gameObject;
		isRightHand = hand == rightHand ? true : false;
		triggerFinger = isRightHand ? OVRInput.RawAxis1D.RIndexTrigger : OVRInput.RawAxis1D.LIndexTrigger;
		//BoltEntity be = grabbedGun.GetComponent<BoltEntity>();
		//be.TakeControl();
	}

	public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.GrabEnd(linearVelocity, angularVelocity);
		//grabbedGun.GetComponent<BoltEntity>().ReleaseControl();
	}

	[PunRPC]
	public IEnumerator FireBurst()              // Coroutine to get the timed burst
	{
		timeSinceLastFire = Time.time;
		canFire = false;
		PlayAnim("Fire");						// Play the firing animation

		for (int i = 0; i < 3; i++)				// Coroutine loop
		{ 
			FireSound();
			currentAmmo -= 1;                   // Use up the ammo when firing

			if (hasAmmoDisplay)
			{
				ChangeAmmo(currentAmmo);
			}

			if (isProjectile)
			{
				Rigidbody bullet = Instantiate<Rigidbody>(bulletPrefab, barrelTip.position, barrelTip.rotation, null);
				bullet.velocity = bullet.transform.forward * bulletSpeed;
			}
			else
			{
				Vector3 endPoint;
				if (Physics.Raycast(barrelTip.position, barrelTip.forward, out RaycastHit hit, raycastDistance, hitMask))
				{
					endPoint = hit.point;
					SpawnBulletHole(bulletHolePrefab, hit.point, hit.normal, hit.transform);
					Rigidbody hitRigidbody = hit.collider.gameObject.GetComponent<Rigidbody>();
					if (hitRigidbody != null)
					{
						hitRigidbody.AddForceAtPosition(rayCastHitForce * barrelTip.forward, hit.point, ForceMode.Impulse);
					}
				}
				else
				{
					endPoint = barrelTip.forward * raycastDistance;
				}
				SpawnLaser(rayEffectPrefab, rayWidth, rayWidth, barrelTip.position, endPoint, barrelTip.forward);
				if (hit.collider != null)
				{
					HealthObject ho = hit.collider.gameObject.GetComponent<HealthObject>();
					if (ho != null)
					{
						//TODO FIX ho.Damage(damage);
					}
				}
				yield return new WaitForSeconds(burstRate);		// Wait for next bullet in burst
			}
		}
	}

	void SpawnBulletHole(GameObject bulletHolePrefab, Vector3 position, Vector3 forward, Transform parent)
	{
		GameObject bulletHoleDecal = Instantiate<GameObject>(bulletHolePrefab, position, Quaternion.identity);
		bulletHoleDecal.transform.forward = forward;
		bulletHoleDecal.transform.Rotate(90, 0, 0);
		bulletHoleDecal.transform.parent = parent;
		bulletHoleDecal.transform.Translate(new Vector3(0f, 0.0001f, 0f), Space.Self);
	}

	void SpawnLaser(LineRenderer rayEffectPrefab, float startWidth, float endWidth, Vector3 startPosition, Vector3 endPosition, Vector3 forward)
	{
		LineRenderer rayEffect = Instantiate<LineRenderer>(rayEffectPrefab, startPosition, Quaternion.Euler(forward));
		StartCoroutine(FadeRay(rayEffect));
		rayEffect.startWidth = startWidth;
		rayEffect.endWidth = endWidth;
		rayEffect.SetPosition(0, startPosition);
		rayEffect.SetPosition(1, endPosition);
	}

	IEnumerator FadeRay(LineRenderer rayEffect)
	{
		Destroy(rayEffect.gameObject, 1f);
		Color fromColor = rayEffect.material.color;
		Color toColor = new Color(1, 1, 1, 0);
		float t = 0;
		float duration = 1;
		while (rayEffect.material.color.a > 0.01f)
		{
			rayEffect.material.color = Color.Lerp(fromColor, toColor, t);
			t += Time.deltaTime / duration;
			yield return new WaitForSeconds(Time.deltaTime);
			if (rayEffect == null)
				break;
		}
		yield return null;
	}

	private void FixedUpdate()
	{
		if (Time.time - timeSinceLastFire > (1f / fireRate))
		{
			serverFiring = false;                   // Added so user can't hold the trigger
			canFire = true;
		}
	}

	void PlayHaptics(float amplitude, float duration, OVRInput.Controller controller)
	{
		OVRInput.SetControllerVibration(0.5f, amplitude, controller);
		StartCoroutine(StopHaptics(duration, controller));
	}

	IEnumerator StopHaptics(float duration, OVRInput.Controller controller)
	{
		yield return new WaitForSeconds(duration);
		OVRInput.SetControllerVibration(0.5f, 0, controller);
	}

	void FireSound()
	{
		fireSound.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
		fireSound.pitch *= fireRate * fireSound.clip.length;
		fireSound.Play();
	}

	void PlayAnim(string stateName)                                     // Added for animation handling
	{                                                                   //
		if (anims.HasState(0, Animator.StringToHash(stateName)))        // Hash is faster searching than string
			anims.Play(stateName, 0, 0);                                      //
		else                                                            //
			Debug.Log("No animation state named " + stateName);         //
	}                                                                   // Each animation should repeat

	void ChangeAmmo(int newAmmo)                                        // Copied from Alex's ChangeAmmoCount code
	{                                                                   //
		string intToString = "";                                        // This was easier to just copy rather than
																		//   set up all the necessary references to
		if (newAmmo > 99) intToString = "99";                           //   call a single function
		else if (newAmmo > 9) intToString = newAmmo.ToString();         //
		else if (newAmmo > -1) intToString = "0" + newAmmo.ToString();  //
		else intToString = "00";                                        //
																		//
		tens.text = intToString[0].ToString();                          //
		ones.text = intToString[1].ToString();                          //
	}                                                                   // Thanks Alex!
}
