using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeDeathManager : MonoBehaviour
{
    PhotonView PV;
    List<DeathBodyAnimController> deathShaderControllers;
    private string playerName;

    [PunRPC]
    private void RPC_SetPlayerName(string name)
    {

        NetD.PrintS("name set for bodyanimcontroller");
        gameObject.name = gameObject.name + name;
        playerName = name;
    }

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        deathShaderControllers = new List<DeathBodyAnimController>();
    }

    [PunRPC]
    private void RPC_PlayerDie()
    {
        if (PV.IsMine)
        {
            #region Screen Fader
            #region More then one screenfader, only the player controlling this game object should have one
            DeathPostProcessing[] screenFaders = FindObjectsOfType<DeathPostProcessing>();
            if (screenFaders.Length > 1)
            {
                string error = "More then one screen fader:";
                for (int i = 0; i < screenFaders.Length; i++)
                {
                    error += "   Screenfader:" + screenFaders[i].name;
                }
                NetD.PrintS("ERROR:" + error);
                return;
            }
            if (screenFaders.Length < 1)
            {
                NetD.PrintS("ERROR: No death post processing found");
                return;
            }
            #endregion            
            screenFaders[0].DieAnim();
            #endregion
        }

        #region Body Anim
        if (deathShaderControllers.Count == 0)
        {
            FindDeathShaderControllers();
        }

        for (int i = 0; i < deathShaderControllers.Count; i++)
        {
            deathShaderControllers[i].KillPlayer();
        }
        #endregion

    }

    private void FindDeathShaderControllers()
    {
        DeathBodyAnimController[] shaderAnimControllers = FindObjectsOfType<DeathBodyAnimController>();
        for (int i = 0; i < shaderAnimControllers.Length; i++)
        {
            if (shaderAnimControllers[i].playerName == playerName)
            {
                deathShaderControllers.Add(shaderAnimControllers[i]);
            }
        }
    }

    [PunRPC]
    private void RPC_PlayerRevive()
    {
        if (PV.IsMine)
        {
            #region Screen Fader
            #region More then one screenfader, only the player controlling this game object should have one
            DeathPostProcessing[] screenFaders = FindObjectsOfType<DeathPostProcessing>();
            if (screenFaders.Length > 1)
            {
                string error = "More then one screen fader:";
                for (int i = 0; i < screenFaders.Length; i++)
                {
                    error += "   Screenfader:" + screenFaders[i].name;
                }
                Debug.LogError(error);
                return;
            }
            #endregion
            screenFaders[0].ReviveAnim();
            #endregion
        }

        #region BodyAnim
        if (deathShaderControllers.Count == 0)
        {
            FindDeathShaderControllers();
        }

        for (int i = 0; i < deathShaderControllers.Count; i++)
        {
            deathShaderControllers[i].RevivePlayer();
        }
        #endregion
    }
}
