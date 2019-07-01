using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerRPCS : MonoBehaviour
{
    public AudioSource lobbyMusic;
    public AudioSource gameMusic;

    [PunRPC]
    private void RPC_RoundStart()
    {
        lobbyMusic.Stop();
        gameMusic.Play();
    }
}
