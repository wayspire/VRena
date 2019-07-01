using Photon.Pun;
using UnityEngine;

public class MovingWall : MonoBehaviour
{
	public GameObject[] models;

	public GameObject[] spawnPointsA;
	public GameObject[] spawnPointsB;
	public RandomItem[] spawnItems;

	public float waitTimer = 10.0f;			// in seconds
    public float destroyTimer = 40.0f;		// in seconds
    public float moveSpeed = 3.0f;

	public float collideZ = -2.0f;			// Z position to begin slowdown and play sound
	public Vector3 stopPos;					// Z position to stop moving entirely and begin countdown
    
    private Vector3 wallPosition;
	private float currentWaitTimer;
	private float length;

    private PhotonView PV;
	public AudioSource soundIn, soundStop, soundOut;
	bool[] soundPlayed = new bool[3];
	float  speedup = 0.0f;

	int[] weights;
	int randResult;

    void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (PV.IsMine)
        {
            currentWaitTimer = waitTimer;
            wallPosition = transform.position;
            PV.RPC("RPC_SetActiveModel", RpcTarget.All, Random.Range(0, models.Length));
            
            for (int i = 0; i < soundPlayed.Length; i++)
                soundPlayed[i] = false;

            weights = new int[spawnItems.Length];
            for (int i = 0; i < spawnItems.Length; i++)
                weights[i] = spawnItems[i].randomWeight;

            
        }
    }

    [PunRPC]
    private void RPC_SetActiveModel(int active)
    {
        models[active].SetActive(true);
    }

    [PunRPC]
    private void RPC_PlaySoundIn()
    {
        soundIn.Play();
    }

    [PunRPC]
    private void RPC_PlaySoundOut()
    {
        soundOut.Play();
    }

    [PunRPC]
    private void RPC_PlaySoundStop()
    {
        soundStop.Play();
    }

    [PunRPC]
    private void RPC_DestroyMovingWall()
    {
        Destroy(gameObject, destroyTimer);
    }

    void Update()
	{
        if (PV.IsMine)
        {
			// Stop (happens second)
            if (wallPosition.z <= stopPos.z && wallPosition.z > stopPos.z - 0.01f && currentWaitTimer > 0)
            {
                currentWaitTimer -= Time.deltaTime;
                wallPosition = stopPos;

                if (!soundPlayed[1])
                {
                    PV.RPC("RPC_PlaySoundStop", RpcTarget.All);
                    soundPlayed[1] = true;
                    soundPlayed[2] = true;

                    for (int i = 0; i < spawnPointsA.Length; i++)
                    {
                        randResult = WeightedRandom(weights);
                        PhotonNetwork.Instantiate(spawnItems[randResult].itemName, spawnPointsA[i].transform.position, Quaternion.Euler(0, -90, 0));
                        //PhotonNetwork.Instantiate("Prefabs\\Gun_Holdout", spawnPointsA[i].transform.position, Quaternion.Euler(0, -90, 0));

                        PhotonNetwork.Instantiate(spawnItems[randResult].itemName, spawnPointsB[i].transform.position, Quaternion.Euler(0, 90, 0));
                        //PhotonNetwork.Instantiate("Prefabs\\Gun_Holdout", spawnPointsB[i].transform.position, Quaternion.Euler(0, 90, 0));

                        //PhotonNetwork.Instantiate("Prefabs\\Hand", Vector3.zero, Quaternion.identity);

                        //Instantiate(, spawnPointsA[i].transform.position, Quaternion.Euler(0, -90, 0));
                        //Instantiate(spawnItems[randResult].item, spawnPointsB[i].transform.position, Quaternion.Euler(0, 90, 0));
                    }
                }
            }

			// Slow down (happens first)
            else if (transform.position.z >= collideZ && transform.position.z < stopPos.z)
            {
                if (!soundPlayed[0])
                {
                    length = Vector3.Distance(transform.position, stopPos);
                    PV.RPC("RPC_PlaySoundIn", RpcTarget.All);
                    soundPlayed[0] = true;
                }

                float distMoved = moveSpeed * Time.deltaTime;
                wallPosition = Vector3.Lerp(wallPosition, stopPos, (distMoved / length));
            }

			// Speed up (happens fourth)
            else if (speedup > 0.0f && speedup < moveSpeed)
            {
                speedup = speedup + 0.002f;
                wallPosition.y -= speedup * Time.deltaTime;
                wallPosition.z += speedup * Time.deltaTime;


                if (speedup >= moveSpeed)
                {
                    speedup = moveSpeed;
                }
            }

			// Start moving again (happens third, only one frame)
            else if (soundPlayed[2] && currentWaitTimer <= 0.0f && speedup == 0.0f)
            {
                PV.RPC("RPC_PlaySoundOut", RpcTarget.All);
                soundPlayed[2] = false;
                PV.RPC("RPC_DestroyMovingWall", RpcTarget.All);
                speedup = 0.001f;
            }

			// (happens at beginning and end)
            else
            {
                wallPosition.y -= moveSpeed * Time.deltaTime;
                wallPosition.z += moveSpeed * Time.deltaTime;
            }

            transform.position = wallPosition;
        }
	}

	int WeightedRandom(int[] weights)
	{
		int weightSum = 0;

		for (int i = 0; i < weights.Length; i++)
		{
			weightSum += weights[i];
		}

		int index = 0;
		int value = weights[0];
		int rand = Random.Range(0, weightSum);

		while (rand > value)
			value += weights[++index];

		return index;
	}
}

[System.Serializable]
public class RandomItem
{
	public string itemName;
	public int randomWeight;
}