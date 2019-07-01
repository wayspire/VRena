using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class TouchMeHoweverYouLike : MonoBehaviour
{
	PhotonView photonView;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		var pv = collision.gameObject.GetComponent<PhotonView>();

		if(pv == null)
		{
			return;
		}

		if(pv.Owner == PhotonNetwork.LocalPlayer)
		{
			photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
		}
	}
}
