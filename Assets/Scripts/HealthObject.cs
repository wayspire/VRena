using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Audio;

public class HealthObject : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;

	float timeSinceLastDamage = 0;
    public bool canRegen = true;
	public float regenDelay = 5f;
	public float regenSpeed = 0.2f;

    public string playerName;

    public AudioClip[] playerDamageSounds;
    public AudioMixerGroup mixerGroup;

    PhotonView PV;

	private void Start()
	{
        PV = GetComponent<PhotonView>();
		//Instead of using this we call stuff and set health when spawning
	}

    //Only call on owner for player
    [PunRPC]
    public void RPC_Damage(float damage)
	{
        if (PV.IsMine)
        {
            timeSinceLastDamage = 0f;
            health -= damage;

            UpdateHealthUi();
            if (health <= 0f)
            {
                StartCoroutine(DeathWait());
                PV.RPC("RPC_PlayerDie", RpcTarget.All);
            }
            PlayDamageSound(UnityEngine.Random.Range(0, playerDamageSounds.Length));
        }       
	}

    public void PlayDamageSound(int sound)
    {
        AudioSourcePool.Play(playerDamageSounds[sound], transform, mixerGroup, 1, UnityEngine.Random.Range(.9f, 1.1f), true);
    }

    [PunRPC]
    public void RPC_SetPlayerName(string name)
    {
 
        playerName = name;
        gameObject.name = "Health Bar:" + name;
    }

    IEnumerator DeathWait()
    {
        canRegen = false;
        yield return new WaitForSeconds(5);
        canRegen = true;
 
        PV.RPC("RPC_PlayerRevive", RpcTarget.All);
        PV.RPC("RPC_SetHealth", RpcTarget.All, maxHealth);
    }

    //Only call on owner for player
    [PunRPC]
    public void RPC_Heal(float heal)
    {
        health = Mathf.Min(maxHealth, health + heal);
        UpdateHealthUi();
    }

    //Only call on owner for player
    [PunRPC]
    public void RPC_SetHealth(float value)
    {
        if (value < 0) { value = 0; }
        if (value > maxHealth) { value = maxHealth; }

        health = value;
        UpdateHealthUi();
    }

    private void FixedUpdate()
	{

        //Timer
		timeSinceLastDamage += Time.deltaTime;

        //Regen
        if (health < maxHealth && timeSinceLastDamage >= regenDelay && canRegen)
		{
			Regen(regenSpeed);
		}
	}

    void Regen(float regenHealth)
    {
        health += regenHealth;
        UpdateHealthUi();
    }

    void UpdateHealthUi()
    {
        PV.RPC("RPC_SetHealthUi", RpcTarget.All, health);

    }
}
