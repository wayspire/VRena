using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class HealthWatch : MonoBehaviour
{
	public TextMeshPro text;
	public Color normal;
	public Color dying;

    PhotonView PV;

    HealthObject playerHealth;
	public string playerName;


	void Awake()
    {
        PV = GetComponent<PhotonView>();
        text.text = "100%";
        //playerHealth = GetComponentInParent<HealthObject>();
    }

	[PunRPC]
	void RPC_SetPlayerName(string name)
	{
		playerName = name;
		text.text = "100%";
	}

	// Calculate percentage of health, and display it on the watch
	public void SetHealth(float val, float max)
	{
		float percent = val / max;

        if(val <= 0)
        {
            text.text = "DEAD";
        }
        else
        {
            text.text = Mathf.Ceil(percent * 100) + "%";
        }
		  
		// When below a quarter, change to more alarming color
		if (percent <= .25f)
        {
            text.color = dying;
        }
        else
        {
            text.color = normal;
        }
			

		// When 0 or below,ss player is dead

        NetD.PrintS("Watch set to " + text.text);
    }


    void Update()
    {/*
		text.text = playerHealth.health.ToString() + "%";
		if (playerHealth.health == 0)
			text.text = "DEAD";

		if (transform.rotation.x < 0.62 && transform.rotation.x > -0.2)
			text.color = vis;
		else
			text.color = invis;
    */}
}
