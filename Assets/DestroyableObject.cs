using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    public DestroyableObjectInfo[] states;
    public float health;
    public float maxHealth;

    public ParticleSystem hitParticles;
    public ParticleSystem destroyParticles;

    public Collider[] colliders;

    private PhotonView PV;

    private void Start()
    {
        PV = GetComponent<PhotonView>();

        if (PV.IsMine)
        {
            //NetD.PrintS("Sending update states");
            PV.RPC("RPC_UpdateStates", RpcTarget.AllBufferedViaServer, maxHealth);
        }
    }

    //Only call on owner for player
    [PunRPC]
    public void RPC_Damage(float damage)
    {
        //NetD.PrintS("Damage is recived");
        if (PV.IsMine)
        {
            //NetD.PrintS("PV mine damage recived");
            health -= damage;
            //NetD.PrintS("New health is:" + health + "   Damage done is:" + damage);
            PV.RPC("RPC_UpdateStates", RpcTarget.All, health);
       
        }
    }

    [PunRPC]
    void RPC_UpdateStates(float newHealth)
    {
        

        //NetD.PrintS("Recived update states with health of:" + newHealth);
        health = newHealth;

        if(health <= 0)
        {
            //NetD.PrintS("Object ded");
            SetActiveState(-1);
            destroyParticles.Play();
            for(int i = 0; i < colliders.Length; i++)
            {
                colliders[i].gameObject.SetActive(false);
            }
            Destroy(gameObject, 10);
            return;
        }

        hitParticles.Play();

        int stateToSetActive = -1;
        for(int i = 0; i < states.Length; i++)
        {
            if(health <= states[i].healthToBeBelow)
            {
                stateToSetActive = i;
            }
        }

        //NetD.PrintS("Setting active state:" + stateToSetActive);
        SetActiveState(stateToSetActive);
    }

   

    void SetActiveState(int state)
    {
        for(int i = 0; i < states.Length; i++)
        {
            if(i == state)
            {
                //NetD.PrintS("Setting state true:" + states[i].state.name);
                states[i].state.SetActive(true);
            }
            else
            {
                //NetD.PrintS("Setting state false:" + states[i].state.name);
                states[i].state.SetActive(false);
            }
        }
    }
}

[System.Serializable]
public class DestroyableObjectInfo
{
    public GameObject state;
    public float healthToBeBelow;
}
