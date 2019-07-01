using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HostOrJoinRoomManager : MonoBehaviourPunCallbacks
{
	public string gameRoomSceneName;
	public string roomName;

	public float timeUntilAutoJoin = 5f;
	float startTime;

	public Text statusText;
	public Slider loadingProgressBar;
	bool isLoadingLevel;

	public void Start()
	{
		startTime = Time.time;
		PhotonNetwork.ConnectUsingSettings();
	}

	public void JoinRoomButton()
	{
		statusText.text = "Joining room...";
		PhotonNetwork.NickName = "default";
		StartCoroutine(JoinRoom());
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		statusText.text = "Joining room failed.\nTrying again...";
		StartCoroutine(JoinRoom());
	}

	public override void OnJoinedRoom()
	{
		if(!PhotonNetwork.IsMasterClient)
		{
			statusText.text = "Joining room successful.\nLoading scene...";
			isLoadingLevel = true;
			PhotonNetwork.LoadLevel(gameRoomSceneName);
		}
	}

	IEnumerator JoinRoom()
	{
		yield return new WaitForSeconds(1f);
		PhotonNetwork.JoinRoom(roomName);
	}
	
	public override void OnConnectedToMaster()
	{
		statusText.text = "Connection established.";
		Debug.Log(PhotonNetwork.SendRate);
	}

	public void CreateRoomButton()
	{
		statusText.text = "Hosting...";
		PhotonNetwork.NickName = "Host";
		CreateRoom();
	}

	void CreateRoom()
	{
		statusText.text = "Creating new room...";
		var roomOpts = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 7 };
		PhotonNetwork.CreateRoom(roomName, roomOpts);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		statusText.text = "Creating room failed.\nTrying again...";
		CreateRoom();
	}

	public override void OnCreatedRoom()
	{
		statusText.text = "Creating room successul.\nLoading scene...";
		isLoadingLevel = true;
		PhotonNetwork.LoadLevel(gameRoomSceneName);
	}

	private void Update()
	{
		if(isLoadingLevel)
		{
			loadingProgressBar.value = PhotonNetwork.LevelLoadingProgress;
			return;
		}

		if(Time.time > startTime + timeUntilAutoJoin)
		{
			JoinRoomButton();
			return;
		}

		if(Input.GetKeyDown(KeyCode.H))
		{
			Debug.Log("Hosting room.");
			CreateRoomButton();
			return;
		}
	}
}
