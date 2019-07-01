using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeFollow : MonoBehaviour
{
    // Start is called before the first frame update

    Head[] players;
    GameObject closestPlayer;
    private void Start()
    {
        StartCoroutine(GetPeople());
        StartCoroutine(StartFollow());
    }

    // Update is called once per frame
    void Update()
    {
        if (closestPlayer != null)
        {
            Debug.Log("try look at");
            transform.LookAt(closestPlayer.transform);
            //Vector3 euler = transform.rotation.eulerAngles;
            //transform.eulerAngles = new Vector3(0, euler.y, 0);
        }  
    }

    IEnumerator GetPeople()
    {
        while (true)
        {
            //NetD.PrintS("players found");
            players = FindObjectsOfType<Head>();
            //NetD.PrintS("players found amt:" + players.Length);
            if(players.Length > 0)
            {
                yield return new WaitForSeconds(15);
            }
            else
            {
                yield return new WaitForSeconds(1);
            }         
        }    
    }

    IEnumerator StartFollow()
    {
        while (true)
        {
            float distance = float.MaxValue;
            if(players != null)
            {
                for(int i = 0; i < players.Length; i++)
                {
                    if(Vector3.Distance(players[i].transform.position, transform.position) < distance)
                    {
                        //NetD.PrintS("Found closer player");
                        distance = Vector3.Distance(players[i].transform.position, transform.position);
                        closestPlayer = players[i].gameObject;
                    }
                }
            }
            if(closestPlayer != null)
            {
                yield return new WaitForSeconds(5);
            }
            else
            {
                yield return new WaitForSeconds(1);
            }
            
        }     
    }
}
