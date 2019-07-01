using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthObjectHolder : MonoBehaviour
{
    public enum BodyParts
    {
        Wheel,
        WheelHolder,
        LeftHand,
        RightHand,
        Body,
        Head
    }

    public BodyParts bodyPart;
    public string playerName;


    [PunRPC]
    private void RPC_SetPlayerName(string name)
    {
        gameObject.name = gameObject.name + name;
        playerName = name;
    }
}
