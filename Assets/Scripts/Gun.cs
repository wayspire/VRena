using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/**
 *  Author: Daddy Spike/ Ethan Shelton
 * 
 *  Youtube Link To Explanation: Coming soon because this class is a master and in a week ill forget everything
 *  
 *  Ok so Under the trigger handling region is the update function, this will take input and controls the animation state
 *  for the animation tree the gun uses, the animations then use events to call FireBullet() under the Firing Host Side,
 *  
 *  Fire bullet goes through all the transforms that need to shoot for the shoot pattern, for each one it sends a raycast
 *  to try to find a player to damage, it also Sends a rpc to everyone to play sound effects, spawn lasers and so on
 *  
 *  Firing network side handles all those spawning of lasers and playing sounds on everyones end
 * 
 * */
public class Gun : OVRGrabbable
{
    #region Public Vars

    [Header("Shoot Options")]
    public ShootMethod shootMethod;
    public ShootMode shootMode;
    public float bulletsFiredPerSec;
    public AudioMixerGroup shootMixerGroup;
    public AudioMixerGroup objectMixerGroup;

    [Space(5)]
    public ShootEffects[] shootEffects;
    public ShootPattern[] shootPatterns;

    [Space(5)]
    [Header("Raycast info")]
    public float raycastDistance;
    public float laserSpeed;
    [Tooltip("Layers the raycast can effect")]
    public LayerMask hitMask;

    [Space(5)]
    [Header("Instantiate OBJ info")]
    public Rigidbody bulletPrefab;
    public float bulletSpeed;

    [Space(15)]
    public float animationTime;
    public Animator animator;
    public bool stopAnimWhenTriggerReleased;
    public AudioClip noAmmoSound;
    public AudioClip playerHitSound;

    #region Enums
    public enum ShootMode
    {
        Auto,
        Semi
    }

    public enum ShootMethod
    {
        Raycast,
        InstantiatedObject
    }
    #endregion

    #endregion

    #region Private Vars
    //Controls
    OVRInput.Controller hand;
    OVRGrabbable grabScript;

    //Components
    PhotonView photonView;
    AmmoCount ammo;

    //Alogorithm
    float lastFiredTime = 0; //the Time.time when the gun was last fired
    bool firing = false;
    bool triggerHeld = false;
    HealthObject[] healthObjectsCache;
    #endregion

    #region Start() and OVR Grabbing
    protected override void Start()
    {
        base.Start();
        grabScript = GetComponent<OVRGrabbable>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        ammo = GetComponent<AmmoCount>();
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);
        bool isRightHand = hand.tag.Equals("Right Hand");
        this.hand = isRightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

        photonView.RPC("RPC_SetKinematic", RpcTarget.All, true);

        //For applying damage
        if (healthObjectsCache == null)
        {
            healthObjectsCache = FindObjectsOfType<HealthObject>();
        }

    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        photonView.RPC("RPC_SetKinematic", RpcTarget.All, false);
        base.GrabEnd(linearVelocity * 3, angularVelocity);
        //photonView.TransferOwnership(PhotonNetwork.MasterClient);

    }
    #endregion

    #region Trigger handling / Update()
    void Update()
    {
        if (grabScript.isGrabbed)
        {
            if (shootMode == ShootMode.Auto)
            {
                //Trigger pressed
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, hand) > 0.9 && firing == false && ammo.ammo > 0)
                {
                    PlayAnimation();
                    firing = true;
                }

                //let go of trigger
                else if ((!(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, hand) > 0.9) && firing == true) || ammo.ammo <= 0)
                {
                    StopAnimation();
                }

                else if (ammo.ammo <= 0 && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, hand) > 0.9 && triggerHeld == false)
                {
                    triggerHeld = true;
                    PlayNoAmmoSound();
                }

                if (triggerHeld && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, hand) < 0.1)
                {
                    triggerHeld = false;
                }

            }
            else if (shootMode == ShootMode.Semi)
            {
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, hand) > 0.9 && firing == false && triggerHeld == false && ammo.ammo > 0)
                {
                    PlayAnimation();
                    stopAnimWhenTriggerReleased = false;
                    StopAnimation();
                    firing = true;
                    triggerHeld = true;

                } else if (ammo.ammo <= 0 && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, hand) > 0.9 && triggerHeld == false)
                {
                    triggerHeld = true;
                    PlayNoAmmoSound();
                }

                if (triggerHeld && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, hand) < 0.1)
                {
                    triggerHeld = false;
                }
            }


        }

    }

    public void PlayAnimation()
    {
  
        lastFiredTime = Time.time;
        animator.SetBool("Fire", true);
    }

    public void StopAnimation()
    {
    
        if (stopAnimWhenTriggerReleased)
        {
            animator.StopPlayback();
            animator.Play("Idle");
            animator.SetBool("Fire", false);
            firing = false;
        }
        else //Lets it go through the rest of the animation
        {
            //print("Anim length:" + animator.GetCurrentAnimatorStateInfo(0).length);
            //print("Playback time:" + (animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1));

            StartCoroutine(WaitStopAnimation());
        }
    }

    IEnumerator WaitStopAnimation()
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Fire"))
        {
            yield return null;
        }

        //Time for animator to end
        float time = animator.GetCurrentAnimatorStateInfo(0).length - (animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1);
 
        yield return new WaitForSeconds(time);

        animator.StopPlayback();
        animator.Play("Idle");
        animator.SetBool("Fire", false);
        firing = false;
    }
    #endregion

    #region Firing Host Side
    public void FireBullet(int patternToUse)
    {
        //If we dont have enough ammo -> do stuff than return    
        if (!ammo.ChangeAmmo(-1, false))
        {
            PlayNoAmmoSound();
            return;
        }

        //Get stuff
        ShootPattern curPattern = shootPatterns[patternToUse];
        ShootEffects curShootEff = GetEffect(curPattern.effectToUse);

        //tell all players about the shot sound
        //Makes it louder if more then 1 is being shot at the same time

        for (int i = 0; i < curPattern.transforms.Length; i++)
        {

            //RPC Method  , Target Type,  Vol,   Pitch,          , effect To use,          SoundToUse          
            photonView.RPC("RPC_FireSound", RpcTarget.All, (float)1, (float)Random.Range(.9f, 1.1f), curPattern.effectToUse, Random.Range(0, curShootEff.audioClips.Length));

            PlayHaptics(curShootEff.hapticForce, curShootEff.hapticDurration, hand);

            //Current position shooting from
            Transform curTrans = curPattern.transforms[i];

            #region Instantiated Object Mode
            if (shootMethod == ShootMethod.InstantiatedObject)
            {

                //TODO, Fix dis
                //Rigidbody bullet = Instantiate<Rigidbody>(bulletPrefab, barrelTip.position, barrelTip.rotation, null);
                //bullet.velocity = bullet.transform.forward * bulletSpeed;

            }
            #endregion
            #region Raycast Mode
            else //ShootMethod == Raycast
            {
                //Used for the laser
                Vector3 endPoint;
                //Raycast hit
                RaycastHit hit;

                #region Raycast and getInfo
                if (Physics.SphereCast(curTrans.position, .02f, curTrans.forward, out hit, raycastDistance, hitMask))
                {
                    //Used for LASER
                    endPoint = hit.point;

                    //Spawns a BULLET HOLE
                    //SpawnBulletHole(hit.point, hit.normal, hit.transform);

                    //Apply FORCE
                    Rigidbody rb = hit.collider.gameObject.GetComponent<Rigidbody>();
                    PhotonView pv = hit.collider.gameObject.GetComponent<PhotonView>();

                    if (rb != null)
                    {
                        pv.TransferOwnership(PhotonNetwork.LocalPlayer);
                        rb.AddForceAtPosition(curShootEff.force * curTrans.forward, hit.point, ForceMode.Impulse);
                    }

                    #region Damage       

                    //The layer for player avatars == 14
                    if (hit.collider.gameObject.layer.Equals(14))
                    {
                        //A player collider is supposed to have a parent with the health object holder parent
                        if (hit.collider.transform.parent == null)
                        {
                            NetD.PrintS("ERROR: Player collider with null parent");
                        } else if (!ApplyDamage(hit.collider.transform.parent.gameObject, curShootEff))
                        {
                            healthObjectsCache = FindObjectsOfType<HealthObject>();
                            ApplyDamage(hit.collider.transform.parent.gameObject, curShootEff);
                        }
                    }else if (hit.collider.gameObject.layer.Equals(15)){
                        if (hit.collider.transform.parent == null)
                        {
                            NetD.PrintS("ERROR: Player collider with null parent");
                        }
                        else
                        {
                            NetD.PrintS("Sending damage to destroyable object");
                            hit.collider.transform.parent.gameObject.GetComponent<PhotonView>().RPC("RPC_Damage", RpcTarget.All, curShootEff.damage);
                        }
                    }
                    #endregion
                }
                else //If no object is hit
                {
                    endPoint = curTrans.forward * raycastDistance;
                }
                #endregion

                photonView.RPC("RPC_SpawnLaser", RpcTarget.All, curPattern.effectToUse, curShootEff.rayWidth, curShootEff.rayWidth, curTrans.position, endPoint, curTrans.forward);
            }
            #endregion
        }
    }

    bool ApplyDamage(GameObject toDamage, ShootEffects curShootEff)
    {
        bool appliedDamage = false;

        for (int j = 0; j < healthObjectsCache.Length; j++)
        {
            if (healthObjectsCache[j].playerName == toDamage.GetComponent<HealthObjectHolder>().playerName)
            {
 
                appliedDamage = true;
                healthObjectsCache[j].GetComponent<PhotonView>().RPC("RPC_Damage", RpcTarget.All, curShootEff.damage);
                PlayPlayerHitSound();
            }
        }
        return appliedDamage;
    }
    #endregion

    #region Firing Network Side (Everyone calculates for themselves) / Spawning Effects

    [PunRPC]
    void RPC_FireSound(float vol, float pitch, int effectToUse, int soundToUse)
    {

        AudioSourcePool.Play
            (
            GetEffect(effectToUse).audioClips[soundToUse], //Audio Clip
            transform,      //Position To Play at
            shootMixerGroup,               //AudioMixerGroup To Play On
            vol,              //Sound
            pitch,              //Pitch
            false           //Wether the audioSource Attaches to this object                   
            );
    }

    #region Spawn Bullet Hole, Depracitated
    //Spawns it over the network so doesnt need rpc
    //   void SpawnBulletHole(Vector3 position, Vector3 forward, Transform parent)
    //{
    //       //Spawn bulletHole
    //       GameObject bulletHoleDecal = PhotonNetwork.Instantiate("Prefabs\\Bullet Hole", position, Quaternion.identity);
    //       PhotonView holePV = bulletHoleDecal.GetComponent<PhotonView>();

    //       //take ownership
    //       holePV.TransferOwnership(PhotonNetwork.LocalPlayer);

    //       //Effect positon
    //       bulletHoleDecal.transform.forward = forward;
    //	bulletHoleDecal.transform.Rotate(90, 0, 0);
    //	bulletHoleDecal.transform.parent = parent;
    //	bulletHoleDecal.transform.Translate(new Vector3(0f, 0.0001f, 0f), Space.Self);
    //}
    #endregion

    [PunRPC]
    void RPC_SpawnLaser(int effectToUse, float startWidth, float endWidth, Vector3 startPosition, Vector3 endPosition, Vector3 forward)
    {
        LineRenderer rayEffect = Instantiate<LineRenderer>(GetEffect(effectToUse).rayEffectPrefab, startPosition, Quaternion.Euler(forward));
        StartCoroutine(FadeRay(rayEffect, endPosition));
        rayEffect.startWidth = startWidth;
        rayEffect.endWidth = endWidth;
        rayEffect.SetPosition(0, startPosition);
        rayEffect.SetPosition(1, endPosition);
    }

    //Gets called by RPC_SpawnLaser to fade the laser out
    IEnumerator FadeRay(LineRenderer rayEffect, Vector3 toPosition)
    {
        //Destroy(rayEffect.gameObject, laser);
        Color fromColor = rayEffect.startColor;
        Color toColor = new Color(1, 1, 1, 0);

        //Vector3 toPosition = rayEffect.GetPosition(1);
        //float t = 0;
        ///float duration = laserDuration;

        while (rayEffect.GetPosition(0) != toPosition)
        {
            //Color 
            //rayEffect.material.color = Color.Lerp(fromColor, toColor, t);

            //Movement
            rayEffect.SetPosition(0, Vector3.MoveTowards(rayEffect.GetPosition(0), toPosition, laserSpeed * Time.deltaTime));// Vector3.Lerp(fromPosition, toPosition, t)
            rayEffect.SetPosition(1, Vector3.MoveTowards(rayEffect.GetPosition(1), toPosition, laserSpeed * Time.deltaTime));

            yield return null;

            if (rayEffect == null)
                break;
        }
        Destroy(rayEffect.gameObject);
        yield return null;
    }

    #endregion

    #region Controller Haptics

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

    #endregion

    //Helper Function To make things more readable
    private ShootEffects GetEffect(int effect)
    {
        return shootEffects[effect];
    }

    private void PlayNoAmmoSound()
    {
        AudioSourcePool.Play(noAmmoSound, transform, shootMixerGroup, 1, Random.Range(.9f, 1.1f), true);
    }

    private void PlayPlayerHitSound()
    {
        AudioSourcePool.Play(playerHitSound, transform, shootMixerGroup, 1, 1, true);
    }
}

[System.Serializable] //Effects what happens when each bullet itself is shot
public class ShootEffects
{
    [Header("Gameplay")]
    public float damage = 1f;
    [Tooltip("Force applied to object raycast hit")]
    public float force = 1f;

    [Header("Effects")]
    public AudioClip[] audioClips;
    public GameObject particleEffect;
    public LineRenderer rayEffectPrefab;
    public float rayWidth;

    [Space(5)]
    [Header("Controls")]
    [Range(0, 1)]
    public float hapticForce;
    public float hapticDurration;
}

[System.Serializable] //Gets chosen through an animation, holds which bullet points will fire and which effect they will use
public class ShootPattern
{
    public Transform[] transforms;
    public int effectToUse;
}