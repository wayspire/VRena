using Photon.Pun;
using UnityEngine;

// This script is a stripped down version of MovingWall
// It should be instantiated near the end of the round, and coincide with the timer reaching 0:00

public class MovingWallEnd : MonoBehaviour
{
	public float moveSpeed = 3.0f;

	public float collideZ = -2.0f;          // Z position to begin slowdown and play sound
	public Vector3 stopPos;                 // Z position to stop moving entirely

	private Vector3 wallPosition;
	private float currentWaitTimer;
	private float length;

	private PhotonView PV;
	public AudioSource soundIn, soundStop;
	bool[] soundPlayed = new bool[2];

	void Awake()
	{
		PV = GetComponent<PhotonView>();

		if (PV.IsMine)
		{
			wallPosition = transform.position;

			for (int i = 0; i < soundPlayed.Length; i++)
				soundPlayed[i] = false;
		}
	}
	
	[PunRPC]
	private void RPC_PlaySoundIn()
	{
		soundIn.Play();
	}

	[PunRPC]
	private void RPC_PlaySoundStop()
	{
		soundStop.Play();
	}

	void Update()
	{
		if (PV.IsMine)
		{
			// Stop
			if (wallPosition.z <= stopPos.z && wallPosition.z > stopPos.z - 0.01f)
			{
				wallPosition = stopPos;

				if (!soundPlayed[1])
				{
					PV.RPC("RPC_PlaySoundStop", RpcTarget.All);
					soundPlayed[1] = true;
				}
			}

			// Slow down
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

			// Move normally
			else
			{
				wallPosition.y -= moveSpeed * Time.deltaTime;
				wallPosition.z += moveSpeed * Time.deltaTime;
			}

			transform.position = wallPosition;
		}
	}
}