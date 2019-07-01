using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBodyAnimController : MonoBehaviour
{
    public Material mainMaterial;
    public Material dissolveMaterial;
    public Material transparentMaterial;
    public ParticleSystem deathParticleEffects;

    public float speed = .05f;
    public float speed2 = .1f;

    public SkinnedMeshRenderer mr;

    public string playerName;

    private void Start()
    {
        if(!mr)
            mr = GetComponent<SkinnedMeshRenderer>();
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        KillPlayer();
    //    }
    //    if(Input.GetKeyUp(KeyCode.Alpha2))
    //    {
    //        RevivePlayer();
    //    }
    //}

    
    public void KillPlayer()
    {
        
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for(int i = 0; i < colliders.Length; i++)
        {
            colliders[i].gameObject.layer = 2;
        }
        
        StartCoroutine(killPlayerCoroutine());
    }

    
    public void RevivePlayer()
    {
        
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].gameObject.layer = 14;
        }
        
        StartCoroutine(revivePlayerCoroutine());
    }

    IEnumerator killPlayerCoroutine()
    {
        if (deathParticleEffects)
        {
            deathParticleEffects.gameObject.SetActive(true);
        }

        mr.material = dissolveMaterial;
        mr.material.SetFloat("_NScale", .44f);

        if (deathParticleEffects)
        {
            // sparks flying particle system animation
            deathParticleEffects.Play();
        }


        float amt = 0;
        while(amt < .9f) {
            mr.material.SetFloat("_DisAmount", amt);
            amt += speed;
            yield return new WaitForSeconds(speed2);
        }

        mr.material = transparentMaterial;
        mr.material.SetColor("_Color", new Color(1, 1, 1, 0));
        amt = 0;
        while (amt < .3f)
        {
            mr.material.SetColor("_Color", new Color(1,1,1,amt));
            amt += speed/2;
            yield return new WaitForSeconds(speed2);
        }
    }

    IEnumerator revivePlayerCoroutine()
    {
        mr.material = transparentMaterial;
        mr.material.SetColor("_Color", new Color(1, 1, 1, .3f));
        float amt = .3f;
        while (amt > 0)
        {
            mr.material.SetColor("_Color", new Color(1, 1, 1, amt));
            amt -= speed / 2;
            yield return new WaitForSeconds(speed2);
        }
        mr.material = dissolveMaterial;
        mr.material.SetFloat("_NScale", .44f);
        amt = 1;
        while (amt > 0)
        {
            mr.material.SetFloat("_DisAmount", amt);
            amt -= speed;
            yield return new WaitForSeconds(speed2);
        }

        mr.material.SetFloat("_NScale", 0);
        mr.material = mainMaterial;

    }

    [PunRPC]
    private void RPC_SetPlayerName(string name)
    {
        gameObject.name = gameObject.name + name;
        playerName = name;
    }
}
