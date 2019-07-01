using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundTimerManager : MonoBehaviour
{

    public GameObject[] worldScreen;
    static private GameObject[] worldTimer;
    static private TextMesh[] timerText;
    private TimeSpan time;
    private string lastSetTimerText = "";
    private PhotonView PV;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    public void UpdateTextUiLocally(float currentRoundTimer)
    {
        if(PV == null)
        {
            Debug.Log("NULL PV");
            PV = GetComponent<PhotonView>();
        }
        // Tick down the clock on the team's screen
        time = TimeSpan.FromSeconds(currentRoundTimer);

        if (timerText == null) { return; }
        for (int i = 0; i < timerText.Length; i++)
            timerText[i].text = time.ToString("mm\\:ss");

        //If text has changed update ui
        if (timerText[0].text != lastSetTimerText)
        {
           
            PV.RPC("RPC_UpdateTimerUi", RpcTarget.All, timerText[0].text);
            lastSetTimerText = timerText[0].text;
        }
    }

    [PunRPC]
    public void RPC_UpdateTimerUi(string newTime)
    {
        
        for (int i = 0; i < timerText.Length; i++)
            timerText[i].text = newTime;
    }



    [PunRPC]
    public void RPC_SetUpDisplayArrays()
    {
       
        worldScreen = GameObject.FindGameObjectsWithTag("TeamScreen");
        worldTimer = new GameObject[worldScreen.Length];
        timerText = new TextMesh[worldScreen.Length];


        for (int i = 0; i < worldScreen.Length; i++)
        {
            if (worldScreen[i] != null)
            {
                //Gets a timer and adds it to worldtimer array
                worldTimer[i] = worldScreen[i].transform.Find("GameTime").gameObject;
                if (worldTimer[i] != null)
                {
                    //initial setup for timer text
                    timerText[i] = worldTimer[i].GetComponent<TextMesh>();
                }
                else Debug.Log("No child with the name GameTime attached to TeamScreen " + i);
            }
        }
        if (worldScreen.Length == 0)
            Debug.Log("No object tagged TeamScreen in scene.");
    }
}
