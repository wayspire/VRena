using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Author: Ethan Shelton and Unity Documentation
 * Why reinvent the wheel just modify it
 * 
 * https://docs.unity3d.com/ScriptReference/Rigidbody.AddExplosionForce.html
 * 
 * Purpose: This handles the potato grenade yeet
 * 
 * Date:5-25-19
 * */

public class PotatoGrenade : OVRGrabbable
{
    [Header("Radius it effects around grenade")]
    public float radius = 5.0f;
    [Header("WE NEEED MORE POWER")]
    public float power = 10.0f;
	public float damage = 10.0f;
    public float timeToPrime = 1f;
	public float timeToExplode = 5f;

	private float timer;
	HealthObject[] healthObjectsCache;
	List<string> damagedNames;

    [Space(10)]
    [Header("Controller stuff")]
    public float hapticsStrength = 1f;
	public float hapticsTime = 0.7f;

    PhotonView photonView;
    private OVRInput.Controller hand;
    private OVRGrabbable grabScript;

    private float activatedOn = 1f;	//Holds time trigger was activated so that grenade primes in selected amt;
    private bool buttonHeld = false;
    private bool primed = false;

    [Space(10)]
    public AudioSource audioSource;
	public AudioSource audioSourceLoop;
    public AudioClip[] sounds;

    protected override void Start()
    {
        photonView = GetComponent<PhotonView>();
        grabScript = GetComponent<OVRGrabbable>();

		

		timer = timeToExplode;
        damagedNames = new List<string>();
    }

    private void Update()
    {
		#region DevTest
		/*
		if (Input.GetKey(KeyCode.G) && !buttonHeld)
		{
			GetComponent<Animator>().SetTrigger("IntoPriming");
			photonView.RPC("RPC_PlaySound", RpcTarget.All, 0);
			activatedOn = Time.time;
			buttonHeld = true;
		}
		else if (buttonHeld && !Input.GetKey(KeyCode.G) && !primed)
		{
			GetComponent<Animator>().SetTrigger("Reset");
			photonView.RPC("RPC_PlaySound", RpcTarget.All, 1);
			buttonHeld = false;
		}*/
		#endregion
		
		if (photonView.IsMine)
        {
			if ((OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, hand) > 0.9 || Input.GetKey(KeyCode.G)) && !buttonHeld)
			{
				GetComponent<Animator>().SetTrigger("IntoPriming");
				photonView.RPC("RPC_PlaySound", RpcTarget.All, 0);
				activatedOn = Time.time;
				buttonHeld = true;
			}
			else if (buttonHeld && !(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, hand) > 0.9 || Input.GetKey(KeyCode.G)) && !primed)
            {
                GetComponent<Animator>().SetTrigger("Reset");
				photonView.RPC("RPC_PlaySound", RpcTarget.All, 1);
				buttonHeld = false;
            }
        }


        //If button has been held for a second
        if(buttonHeld && !primed && Time.time - activatedOn > timeToPrime)
        {
            PlayHaptics(hapticsStrength, hapticsTime, hand);
            primed = true;
        }


		// Once the weapon is primed, begin countdown to explosion
		if (primed)
		{
			if (timer == timeToExplode)
			{
				GetComponent<Animator>().SetTrigger("IntoPrimed");
				photonView.RPC("RPC_PlaySound", RpcTarget.All, 2);
				photonView.RPC("RPC_PlayLoop", RpcTarget.All);
			}

			timer -= Time.deltaTime;

			#region EXPLOSION
			if (timer <= 0f)
			{
				GetComponent<Animator>().SetTrigger("Explode");

				photonView.RPC("RPC_StopLoop", RpcTarget.All);
				photonView.RPC("RPC_PlaySound", RpcTarget.All, 3);
				
				Vector3 explosionPos = transform.position;
				Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);

				foreach (Collider hit in colliders)
				{
					ApplyDamage(hit.gameObject);
					Rigidbody rb = hit.GetComponent<Rigidbody>();
					PhotonView pv = hit.GetComponent<PhotonView>();

                    if (rb != null && pv != null) 
					{
						pv.TransferOwnership(PhotonNetwork.LocalPlayer);
						rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
					}
				}
				
				primed = false;
				photonView.RPC("RPC_FinishExplosion", RpcTarget.All);
			}
			#endregion
		}
	}

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);
        bool isRightHand = hand.tag.Equals("Right Hand");
        this.hand = isRightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

        photonView.RPC("RPC_SetKinematic", RpcTarget.All, true);
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity * 3, angularVelocity);
        //photonView.TransferOwnership(PhotonNetwork.MasterClient);

        photonView.RPC("RPC_SetKinematic", RpcTarget.All, false);
    }

    /*//Activated by animation
    public IEnumerator Prime()
    {
        #region Animation Stuff
        
        yield return new WaitForSeconds(.5f);
        GetComponent<Animator>().SetTrigger("IntoPrimed");

        yield return new WaitForSeconds(2.2f);
        photonView.RPC("RPC_PlayExplodingSound", RpcTarget.All);

        yield return new WaitForSeconds(2.8f);

        GetComponent<Animator>().SetTrigger("Explode");

        #endregion

        #region EXPLOSION
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            PhotonView pv = hit.GetComponent<PhotonView>();

            if (rb != null)
            {
                pv.TransferOwnership(PhotonNetwork.LocalPlayer);
                rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
            }                
        }
        #endregion

        
        yield return new WaitForSeconds(.25f);

        photonView.RPC("RPC_FinishExplosion", RpcTarget.All);
    }*/

    [PunRPC]
    void RPC_PlaySound(int index)
    {
        audioSource.PlayOneShot(sounds[index]);
    }

	[PunRPC]
	void RPC_PlayLoop()
	{
		audioSourceLoop.Play();
	}

	[PunRPC]
	void RPC_StopLoop()
	{
		audioSourceLoop.Stop();
	}

	[PunRPC]
    void RPC_FinishExplosion()		// Spawn particle effects and destroy gameobject
    {
		//TODO: particles and other explosion visuals

        //Destroy object after explosion
        Destroy(this.gameObject, 0.25f);
	}


	bool ApplyDamage(GameObject toDamage)
	{
 
        if (!toDamage.layer.Equals(LayerMask.NameToLayer("PlayerAvatar")))
        {
            NetD.PrintS("PotatoGrenade.cs: Layer is not PlayerAvatar");
            return false;
        }
        NetD.PrintS("Hit with grenade: " + toDamage.gameObject);

        toDamage = toDamage.transform.parent.gameObject;
            
        healthObjectsCache = FindObjectsOfType<HealthObject>();

        bool appliedDamage = false;
        HealthObjectHolder holder = null;
        holder = toDamage.GetComponent<HealthObjectHolder>();
        if(holder == null)
        {
            NetD.PrintS("No health object holder on game object:" + toDamage.gameObject.name);
            return false;
        }
        string newName = holder.playerName;

        
		foreach (string name in damagedNames)
		{
			// If the name has already been damaged, ignore it
			if (newName == name)
				return appliedDamage;
		}

       
        // Add the name to the list
        damagedNames.Add(newName);


		for (int j = 0; j < healthObjectsCache.Length; j++)
		{
			if (healthObjectsCache[j].playerName == newName)
			{

				appliedDamage = true;
				healthObjectsCache[j].GetComponent<PhotonView>().RPC("RPC_Damage", RpcTarget.All, damage);
			}
		}
		return appliedDamage;
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
}
