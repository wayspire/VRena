using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public AudioSource soundSource;
	public AudioSource musicSource;
	public static SoundManager instance = null;

	public float lowPitch = 0.9f;
	public float highPitch = 1.1f;

	void Awake()
	{
		// Singleton
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
	}

	public void PlayOnce(AudioClip clip)
	{
		soundSource.clip = clip;
		soundSource.Play();
	}

	public void PlayMusic(AudioClip loop)
	{
		if (musicSource.clip != loop)
		{
			musicSource.Stop();
			soundSource.clip = loop;
			musicSource.loop = true;
			musicSource.Play();
		}
	}

	// Randomly choose between predetermined audio clips, and slightly adjust pitch for further variation
	public void RandomizeSounds(params AudioClip[] clips)
	{
		int randIndex = Random.Range(0, clips.Length);
		float randPitch = Random.Range(lowPitch, highPitch);

		soundSource.pitch = randPitch;
		soundSource.clip = clips[randIndex];
		soundSource.Play();
	}
}