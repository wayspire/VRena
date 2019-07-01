using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Made by Spike Felion/ Ethan Shelton
 * Date: 6-7-19
 * This is to sync the is kinematic part of the rigidbody
 * 
 * */

public class CommonRPCs : MonoBehaviour
{
    [PunRPC]
    void RPC_SetKinematic(bool isKinemtatic)
    {
        //Debug.Log("RPC Set Kinematic called on object:" + gameObject.name);
        GetComponent<Rigidbody>().isKinematic = isKinemtatic;
    }

    [PunRPC]
    void RPC_SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
