using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCount : MonoBehaviour
{
    public int ammo;
    public int ammoMax;

    public Canvas canvas;

    public bool visibleCounter;
    public Text[] counterDigits;

    public bool visibleMeter;
    public Slider meter;
    public bool canReload = false;

    private PhotonView PV;

    private bool startedReload = false;


    void Awake()
    {
        if (canvas != null)
            //canvas.worldCamera = FindObjectOfType<CenterCamera>().GetComponent<Camera>();

            PV = GetComponent<PhotonView>();
        ChangeAmmo(ammo, true);
    }

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        ChangeAmmo(ammo, true);
    }

    public bool ChangeAmmo(int newAmmo, bool isExact)       // Return true if the gun can actually fire
    {
        if (isExact)
        {
            if (newAmmo < 0)
                newAmmo = 0;

            if (PV)
                PV.RPC("PUN_UpdateAmmo", RpcTarget.All, newAmmo);

            return false;
        }
        else if (ammo == 0 && newAmmo < 0)
        {
            if (!startedReload && canReload)
            {
                StartCoroutine(Reload());
            }
            return false;
        }
        else
        {
            PV.RPC("PUN_UpdateAmmo", RpcTarget.All, (ammo + newAmmo));
        }
        return true;
    }

    private IEnumerator Reload()
    {
        yield return new WaitForSeconds(2);
    }

    [PunRPC]
    private void PUN_UpdateAmmo(int amt)
    {
        ammo = amt;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (canvas != null && canvas.worldCamera == null)           // In case the camera doesn't exist on awake
            //canvas.worldCamera = FindObjectOfType<CenterCamera>().GetComponent<Camera>();

        if (visibleCounter)
        {
            string intToString = ammo.ToString();

            if (intToString.Length > counterDigits.Length)
            {
                //Debug.Log("Too big, converting.");
                foreach (Text digit in counterDigits)
                    digit.text = "9";
            }

            else if (ammo < 0)
            {
                //Debug.Log("Too small, converting.");
                foreach (Text digit in counterDigits)
                    digit.text = "0";
            }

            else
            {
                int diff = (counterDigits.Length - intToString.Length);

                for (int i = counterDigits.Length - 1; i >= 0; i--)
                {
                    //Debug.Log(i - diff);
                    if (i - diff < 0)
                        counterDigits[i].text = "0";
                    else
                        counterDigits[i].text = intToString[i - diff].ToString();
                    //Debug.Log(counterDigits[i].text);
                }
            }
        }

        if (visibleMeter)
        {
            meter.value = (float)ammo / (float)ammoMax;
        }
    }
}
