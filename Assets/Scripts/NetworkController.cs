using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class NetworkController : MonoBehaviour
{
    string _room = "Demo";

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    void OnJoinedLobby()
    {
        Debug.Log("joined lobby");

        RoomOptions roomOptions = new RoomOptions() { };
        PhotonNetwork.JoinOrCreateRoom(_room, roomOptions, TypedLobby.Default);
    }

    void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("OculusPlayer", Vector3.zero, Quaternion.identity, 0);
    }
}