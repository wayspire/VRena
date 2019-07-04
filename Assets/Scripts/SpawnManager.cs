using OVRTouchSample;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	[Tooltip("The avatar prefab that handles player movement")]
	public GameObject oculusPlayer;
	[Tooltip("allows us to disable the spectator camera when the game starts")]
	public Camera spawnCamera;

	[HideInInspector]
	public static SpawnManager SM;
	[SerializeField]
	public Transform[] team1Spawns;
	[SerializeField]
	public Transform[] team2Spawns;

	private bool isSpawned = false;

	public static int team1Size = 0;
	public static int team2Size = 0;

	private Dictionary<int, Transform> syncTable;

	PhotonView photonView;

	private void OnEnable()
	{
		if(SpawnManager.SM == null)
		{
			SpawnManager.SM = this;
		}
	}

	private void Start()
	{
		syncTable = new Dictionary<int, Transform>();
		photonView = GetComponent<PhotonView>();
	}
	
	public Transform GetSyncTransform(PlayerID id)
	{
		NetD.PrintS("SM.cs: Getting value to sync to...");
		syncTable.TryGetValue(id.playerID, out Transform rtn);
		NetD.PrintS("SM.cs: Got transform to sync to: " + rtn.position);
		return rtn;
	}

	private void Update()
	{
		OVRInput.Update();

		if(PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if(!isSpawned)
		{
			SpawnPlayer();
		}
	}

	[PunRPC]
	public void RPC_SetTeam1Size(int size)
	{
		team1Size = size;
	}

	[PunRPC]
	public void RPC_SetTeam2Size(int size)
	{
		team2Size = size;
	}

	void SpawnPlayer()
	{
		int team = PhotonNetwork.LocalPlayer.ActorNumber%2 + 1;
		isSpawned = true;
		spawnCamera.gameObject.SetActive(false);

		GameObject player = Instantiate(oculusPlayer);
		PlayerID playersID = player.GetComponent<PlayerID>();
        PhotonNetwork.NickName = playersID.PlayerName;

		if(team == 1)
		{
			ColocationSync.CS.syncTransform = team1Spawns[team1Size];
			NetD.PrintS("Team 1 size before: " + team1Size);
			photonView.RPC("RPC_SetTeam1Size", RpcTarget.AllBufferedViaServer, team1Size + 1);
			NetD.PrintS("Team 1 size after: " + team1Size);
		}
		else if(team == 2)
		{
			ColocationSync.CS.syncTransform = team2Spawns[team2Size];
			NetD.PrintS("Team 1 size before: " + team1Size);
			photonView.RPC("RPC_SetTeam2Size", RpcTarget.AllBufferedViaServer, team2Size + 1);
			NetD.PrintS("Team 1 size after: " + team1Size);
		}

		#region Spawn Body Parts
		//player.transform.position = new Vector3(player.transform.position.x, OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea)[0].y, player.transform.position.z);
		GameObject leftHand = PhotonNetwork.Instantiate("Prefabs\\Hand 1", Vector3.zero, Quaternion.identity);
		GameObject rightHand = PhotonNetwork.Instantiate("Prefabs\\Hand", Vector3.zero, Quaternion.identity);
		GameObject body = PhotonNetwork.Instantiate("Prefabs\\Body", Vector3.zero, Quaternion.identity);
		GameObject head = PhotonNetwork.Instantiate("Prefabs\\Head", Vector3.zero, Quaternion.identity);
		GameObject casing = PhotonNetwork.Instantiate("Prefabs\\Casing", Vector3.zero, Quaternion.identity);
		GameObject wheel = PhotonNetwork.Instantiate("Prefabs\\Wheel", Vector3.zero, Quaternion.identity);
		GameObject healthBar = PhotonNetwork.Instantiate("Prefabs\\Health Bar", Vector3.zero, Quaternion.identity);
		#endregion

		#region Turns off body for local player
		SkinnedMeshRenderer[] renderers = body.GetComponentsInChildren<SkinnedMeshRenderer>();
		for(var i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = false;
		}

		renderers = head.GetComponentsInChildren<SkinnedMeshRenderer>();
		for(var i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = false;
		}
		#endregion

		#region Parent Body Parts to avatar
		leftHand.transform.parent = player.GetComponentInChildren<LeftHand>().transform;
		rightHand.transform.parent = player.GetComponentInChildren<RightHand>().transform;
		head.transform.parent = player.GetComponentInChildren<CenterCamera>().transform;
		body.transform.parent = player.GetComponentInChildren<CenterCamera>().transform;
		casing.transform.parent = player.GetComponentInChildren<CenterCamera>().transform;
		wheel.transform.parent = player.GetComponentInChildren<CenterCamera>().transform;
		healthBar.transform.parent = head.transform;
		#endregion

		leftHand.transform.localPosition = new Vector3(2, 0, -.4f);
		
		rightHand.transform.localPosition = new Vector3(0, 2, -.6f);
		healthBar.transform.localPosition = new Vector3(0, .1f, 0);

		leftHand.transform.localRotation = Quaternion.identity;
		rightHand.transform.localRotation = Quaternion.identity;
		body.transform.localRotation = Quaternion.identity;
		head.transform.localRotation = Quaternion.identity;
		casing.transform.localRotation = Quaternion.identity;
		wheel.transform.localRotation = Quaternion.identity;
		healthBar.transform.localRotation = Quaternion.identity;

		#region Lock Position and Rotation
		body.GetComponent<LockPositionAndRotation>().LockRotation();
		casing.GetComponent<LockPositionAndRotation>().LockRotation();
		wheel.GetComponent<LockPositionAndRotation>().LockRotation();

		head.GetComponent<LockPositionAndRotation>().LockPosition(new Vector3(0, -0.187f, 0));
		body.GetComponent<LockPositionAndRotation>().LockPosition(new Vector3(0, -0.737f, 0));
		casing.GetComponent<LockPositionAndRotation>().LockPosition(new Vector3(0, -1.177f, 0));   //these set their own y position
		wheel.GetComponent<LockPositionAndRotation>().LockPosition(new Vector3(0, -1.177f, 0));    //
		#endregion

		#region Set Player Name On BodyParts
		//Used for damage, with a health object holder on each bodypart
		leftHand.GetComponent<PhotonView>().RPC("RPC_SetPlayerName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		rightHand.GetComponent<PhotonView>().RPC("RPC_SetPlayerName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		head.GetComponent<PhotonView>().RPC("RPC_SetPlayerName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
        //head.GetComponent<PhotonView>().RPC("RPC_SetDeathPlayerName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		body.GetComponent<PhotonView>().RPC("RPC_SetPlayerName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		casing.GetComponent<PhotonView>().RPC("RPC_SetPlayerName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		wheel.GetComponent<PhotonView>().RPC("RPC_SetPlayerName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		healthBar.GetComponent<PhotonView>().RPC("RPC_SetPlayerName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		#endregion

		#region HandManagement
		//Flips the hand
		//leftHand.transform.localScale = new Vector3(leftHand.transform.localScale.x * -1, leftHand.transform.localScale.y, leftHand.transform.localScale.z);

		//This way it takes input from the right thing
		leftHand.GetComponent<OVRGrabber>().SetController(OVRInput.Controller.LTouch);
		leftHand.GetComponent<Hand>().SetController(OVRInput.Controller.LTouch);
		leftHand.GetComponent<HealthObjectHolder>().bodyPart = HealthObjectHolder.BodyParts.LeftHand;

		//Disable the hand animation controller if not the one who spawn the hand
		leftHand.GetComponent<PhotonView>().RPC("RPC_SetHandActive", RpcTarget.Others, false);
		rightHand.GetComponent<PhotonView>().RPC("RPC_SetHandActive", RpcTarget.Others, false);

		//Setup watch
		PhotonView[] views = leftHand.GetComponentsInChildren<PhotonView>();
		for(var i = 0; i < views.Length; i++)
		{
			views[i].RPC("RPC_SetPlayerName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		}
		#endregion

		#region Health Bar Management
		//Make healthbar follow player
		healthBar.GetComponent<PhotonView>().RPC("RPC_ParentBarToHead", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		//Disable Left hand watch
		//leftHand.GetComponentInChildren<HealthWatch>().GetComponent<PhotonView>().RPC("RPC_SetActive", RpcTarget.AllBufferedViaServer, false);

		//if (team == 1)
		//    healthBar.GetComponent<UI_OverHead>().TeamBlue();
		//else if (team == 2)
		//    healthBar.GetComponent<UI_OverHead>().TeamPurple();

		//Set healthbars name
		//healthBar.GetComponent<PhotonView>().RPC("RPC_SetName", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName);
		//Makes all healthbars look towards Player
		UI_OverHead.SetControllingPlayer(body.transform);
		//Disables own healthbar Renderer
		healthBar.transform.GetChild(0).gameObject.SetActive(false);
		#endregion

	}

}
