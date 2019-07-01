using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public int teamLives = 0;
    public int teamScore = 0;
    public string teamName;

    public int number = 0;
    public int players = 0;


    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public void SetupTeam(int teamNum, int numPlayers, bool isLarger)
    {
        teamName = "Team " + teamNum;
        number = teamNum;
        players = numPlayers;

        if (isLarger)
        {
            teamLives = 2 + (numPlayers*3);
        }
        else
        {
            teamLives = (numPlayers*3);
        }
    }
}
