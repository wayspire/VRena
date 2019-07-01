using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //SingleTon
    public static GameManager instance = null;

    [Header("Prefabs / used")]
    public GameObject teamManagerPrefab; //Gets instantiated then set
	public string destroyedModelName; //used to detect which scene models are broken or unbroken
	public AudioClip crashSound;
    public AudioClip gameMusic;
    public AudioSource thisSource;
    public AudioSource lobbyMusicSource;
	[Space(5)]

    // TODO: make sure these cannot be 0
    public int numTeams;
    public int numPlayers;
    // end of TODO

    [Header("Timing Settings")]
    public float crashTime = 3.0f;		// in seconds
    public float initTimer = 30.0f;     // in seconds
	public float countdownTimer = 3.0f;	// in seconds
    public float roundTimer = 420.0f;   // in seconds
    public float wallTimer = 60.0f;     // in seconds
    public float warningTimer = 4.0f;   // in seconds

    [Space(5)]
    [Header("Moving Wall Settings")]
    public GameObject movingWall, movingWallEnd;
    public Vector3 wallStartPos = new Vector3(0.0f, 1.0f, -35.0f);
	public float endWallTime = 10.0f;

    private bool isRunning = false;
    private bool isStarted = false;

    private GameManagerRPCS gmRPCS;

    #region Private variables / Game Logic
    private PhotonView PV;
    private RoundTimerManager roundTimerManager;

    private bool roundStarted = false;
    private bool roundPlaying = false;
    private bool wallWarning = false;
	private bool endWarning = false;
	private float currentInitTimer;
    private float currentRoundTimer;
    private float currentWallTimer;

    GameObject[] arenaForms;
    #endregion

    void Awake()
    {
        // Singleton
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        gmRPCS              = FindObjectOfType<GameManagerRPCS>();
        PV                  = GetComponent<PhotonView>();
        roundTimerManager   = PhotonNetwork.Instantiate("Prefabs\\Round Timer Manager", Vector3.zero, Quaternion.identity).GetComponent<RoundTimerManager>();

        InitGame();
    }


    // Divide player number evenly, and spread remainder (if uneven) appropriately across all teams.
    void InitGame()
    {
        // TODO: add a menu for the user to select these numbers upon game setup

        int teamSize = numPlayers / numTeams;
        int remain = numPlayers % numTeams;

        #region Setup Teams : No networking

        for (int i = 1; i <= numTeams; ++i)
        {
            Instantiate(teamManagerPrefab);
            TeamManager teamScript = teamManagerPrefab.GetComponent<TeamManager>();

            if (remain > 0)
            {
                teamScript.SetupTeam(i, teamSize + 1, true);
                remain--;
            }
            else
            {
                teamScript.SetupTeam(i, teamSize, false);
            }
        }
        #endregion
        #region SetupArenas : Send RPCS
        arenaForms = GameObject.FindGameObjectsWithTag("Arena");
        if (arenaForms[0] == null)
            NetD.PrintS("No object tagged Arena in scene.");

        foreach (GameObject arena in arenaForms)
        {
            if (arena.name == destroyedModelName)       // FindGameObjectsWithTag cannot search inactive objects, so leave it active and deactivate it on game start
            {
                arena.GetComponent<PhotonView>().RPC("RPC_SetActive", RpcTarget.AllBufferedViaServer, false);
     
            }
                
                

            //arena.SetActive(false);
        }
        #endregion



        //Tell each instance to setup their displays
        roundTimerManager.GetComponent<PhotonView>().RPC("RPC_SetUpDisplayArrays", RpcTarget.AllBufferedViaServer); 

        //Set startTime
        roundTimerManager.UpdateTextUiLocally(currentRoundTimer);
        
        currentInitTimer = initTimer;
    }


    public IEnumerator RoundStart()
    {
        gmRPCS.GetComponent<PhotonView>().RPC("RPC_RoundStart", RpcTarget.AllBufferedViaServer);
        //so it doesnt activate again
		roundStarted = true;

        StartCoroutine(DestroyScene());

        // TODO: count down from 3(?) notifying all players, and then start the game
        yield return new WaitForSeconds(countdownTimer);

        //Setup variables
        currentRoundTimer = roundTimer;
        currentWallTimer = wallTimer;

        roundPlaying = true;

		// TODO: start music
		StopCoroutine("RoundStart");
    }

    public IEnumerator DestroyScene()
    {
        GetComponent<AudioSource>().Play();                                         // demo placeholder sound

        //Waits a small bit then changes state
        yield return new WaitForSeconds(crashTime);

        //Turns destroyed objects on and undestroyed off
        foreach (GameObject arena in arenaForms)
        {
            arena.GetComponent<PhotonView>().RPC("RPC_SetActive", RpcTarget.AllBufferedViaServer, !arena.activeInHierarchy);
        }
    }


    void Update()
    {
        #region Computer Control
        if (roundPlaying)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentRoundTimer += 30f;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                roundStarted = !roundStarted;
            }
        }
        else if (!roundPlaying)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                StartCoroutine("RoundStart");
            }
        }
        
        #endregion

        #region If game is started
        if (roundPlaying)
        {

            #region Checks if wall can be instantiated : Local
            // Call warning sound when the timer reaches the predetermined time ( >0 )
            if (currentWallTimer <= warningTimer && wallWarning == false)
            {
				// As long as there is enough time in the round for the next wall to come through, create it moving across the middle
				if (currentWallTimer + warningTimer + movingWall.GetComponent<MovingWall>().destroyTimer <= currentRoundTimer)
				{
					wallWarning = true;     // so it's not called every frame
					// TODO: play warning sound
				}
            }
            #endregion

            #region Instiates moving wall: Local with network.instantiate
            // Create moving wall when timer hits 0, then reset timer and flag
            if (currentWallTimer <= 0.0f && wallWarning == true)
            {
                
                PhotonNetwork.Instantiate("Prefabs\\Moving Wall", wallStartPos, Quaternion.identity);

                currentWallTimer = wallTimer;
                wallWarning = false;
            }
			#endregion

			#region Instantiate end of round wall
			if (currentRoundTimer <= endWallTime && endWarning == false)
			{
				NetD.PrintS("Try spawn end wall");
				PhotonNetwork.Instantiate("Prefabs\\Moving Wall End", wallStartPos, Quaternion.identity);
				
				endWarning = true;
			}
			#endregion

			#region End Round: Local
			// End round when time time runs out
			if (currentRoundTimer <= 0.0f)
            {
                roundPlaying = false;
                currentRoundTimer = 0.0f;       // so displays aren't showing negative values
                RoundEnd();
            }
            #endregion

            #region Update Timer floats
            currentRoundTimer -= Time.deltaTime;
            currentWallTimer -= Time.deltaTime;
            #endregion

            roundTimerManager.UpdateTextUiLocally(currentRoundTimer);
        }
        #endregion
        #region Else, timer for beggining round wait timer then start round : Local
        else
        {
			//currentInitTimer -= Time.deltaTime;
			//if (currentInitTimer <= 0.0f && !roundStarted)
				//StartCoroutine("RoundStart");
		}
        #endregion
    }



    public void RoundEnd()
    {
		// TODO: stop music, remove guns, notify players
    }
}
