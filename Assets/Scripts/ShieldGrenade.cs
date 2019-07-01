using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Original Author: Ethan Shelton
 * Date: 5-25-19
 * 
 * Spawns a shield where the grenade lands, YEET
 * 
 * */

public class ShieldGrenade : MonoBehaviour
{
    [Header("Radius it effects around grenade")]
    public float radius = 5.0F;

    
    public float waitAfterHittingFloor = 2;
    public Vector3 shieldOffset;
    public GameObject shieldPrefab;

    [Space(10)]
    public Vector3 enemyBasePos;//TODO :Have it get from some sort of static variables holder for each level

    ShieldGrenadeState currentState = ShieldGrenadeState.Idle;//To see current state go into debug mode

    enum ShieldGrenadeState
    {
        Idle,//If not suppose to explode on impact
        Thrown, //if being thrown
        Exploding //If in the middle of exploding that way it doesnt explode multiple times
    }

    //Activated by the thrower in fps mode
    //Used to indicate wether or not to explode on impact or if its just an item lying idle on the ground
    public void Thrown()
    {
        Debug.Log("Thrown");
        currentState = ShieldGrenadeState.Thrown;
    }

    public void OnCollisionEnter(Collision collision)
    {
        //wait 2 seconds then EXPLOSION
        if (currentState == ShieldGrenadeState.Thrown)
        {
            StartCoroutine(Explode());

            //TODO: in the future have it based off an animation most likely or immidieate
        }
    }

    public IEnumerator Explode()
    {
        Debug.Log("Exploding");
        currentState = ShieldGrenadeState.Exploding;
        yield return new WaitForSeconds(waitAfterHittingFloor);

        GameObject newShield = Instantiate(shieldPrefab, this.transform.position + shieldOffset, Quaternion.identity);
        newShield.transform.LookAt(enemyBasePos);
        Vector3 correctedRot = new Vector3(0, newShield.transform.rotation.eulerAngles.y, 0);

        newShield.transform.rotation = Quaternion.Euler(correctedRot);
        Destroy(this.gameObject);
    }
}
