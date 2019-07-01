using Photon.Pun;
using UnityEngine;
using UnityEngine.Audio;

public class CollisonSoundHandler : MonoBehaviour
{
	public AudioClip[] sounds;
	public AudioMixerGroup collideMixerGroup;
	public float multiplier = .2f;

	public float volume;

	private PhotonView photonView;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
	}

	private void OnCollisionEnter(Collision col)
    {
        float amt = Vector3.Dot(col.contacts[0].normal, col.relativeVelocity) * GetComponent<Rigidbody>().mass;
		if (amt > 0.2)
		{
			float randPitch = Random.Range(0.9f, 1.1f);
			float randSound = Random.Range(0, sounds.Length);
			photonView.RPC("RPC_CollideSound", RpcTarget.All, randSound, volume, randPitch);
		}
			
    }

	[PunRPC]
	void RPC_CollideSound(int soundToUse, float vol, float pitch)
	{
		AudioSourcePool.Play
			(sounds[soundToUse], transform,	collideMixerGroup, vol, pitch, true);
	}
}
