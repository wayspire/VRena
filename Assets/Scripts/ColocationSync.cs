using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColocationSync : MonoBehaviour
{
	public static ColocationSync CS;

	public Transform syncTransform;

	Transform centerEye;
	Transform oculusPlayer;

	PhotonView photonView;
	
	private void OnEnable()
	{
		if(ColocationSync.CS == null)
		{
			ColocationSync.CS = this;
		}
		else
		{
			Destroy(this);
		}
	}

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
	}

	void Update()
    {
		if(Input.GetKeyDown(KeyCode.Space))
		{
			Debug.Log("Syncing server");
			photonView.RPC("RPC_Sync", RpcTarget.Others);
			Debug.Log("Done syncing server");
		}

		//if(OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch)) //GetDown is a little bitch
		//{
		//	Sync();
		//}
    }

	[PunRPC]
	public void RPC_Sync()
	{
		if(!PhotonNetwork.IsMasterClient)
		{
			Sync();
		}
	}

	void Sync()
	{
		NetD.PrintS(PhotonNetwork.NickName + " is syncing");
		centerEye = FindObjectOfType<CenterCamera>().transform;
		oculusPlayer = GameObject.FindGameObjectWithTag("OculusPlayer").transform;
		float yPos = oculusPlayer.transform.position.y;

		Transform oculusParent = oculusPlayer.parent;
		float saveAngle = oculusPlayer.rotation.eulerAngles.y;
		oculusPlayer.parent = null;
		oculusPlayer.localRotation = Quaternion.identity;
		Vector3 newPos = syncTransform.position;
		newPos.x -= centerEye.localPosition.x;
		newPos.y = yPos;
		newPos.z -= centerEye.localPosition.z;
		oculusPlayer.position = newPos;
		oculusPlayer.parent = oculusParent;
		oculusPlayer.RotateAround(centerEye.position, Vector3.up, syncTransform.rotation.eulerAngles.y);
	}
}
