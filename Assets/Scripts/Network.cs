using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Network : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1;
        Application.targetFrameRate = 72;
        OVRManager.gpuLevel = 2;
        OVRManager.tiledMultiResLevel = OVRManager.TiledMultiResLevel.LMSMedium;

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Debug.Log("new Player " + newPlayer.NickName + " entered room");
    }
    

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		Debug.Log("Player " + otherPlayer.NickName + " left the room");
	}
}
