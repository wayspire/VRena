using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetD : MonoBehaviour
{
    private static NetD console;
    private bool ignoreMessage = false;

    private void Start()
    {
        Application.logMessageReceived += HandlerMethodName;   
    }

    public void HandlerMethodName(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Warning)
        {
            console.gameObject.GetComponent<PhotonView>().RPC("RPC_ConsolePrint", RpcTarget.All, ("Player:" + PhotonNetwork.NickName + "\n" + logString + "\n" + stackTrace));
        }
    }

    public static void PrintS(string message)
    {
        if(console == null)
        {
            console = PhotonNetwork.Instantiate("Prefabs\\Network Console", Vector3.zero, Quaternion.identity).GetComponent<NetD>();
            DontDestroyOnLoad(console.gameObject);
        }

        console.gameObject.GetComponent<PhotonView>().RPC("RPC_ConsolePrint", RpcTarget.All, ("Player:" + PhotonNetwork.NickName + "\n" + message));
    }

    [PunRPC]
    private void RPC_ConsolePrint(string message)
    {
        ignoreMessage = true;
        Debug.Log(message);
        ignoreMessage = false;
    }
}
