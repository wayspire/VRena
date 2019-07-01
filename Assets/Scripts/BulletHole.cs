using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHole : MonoBehaviour
{
	public float decayDelay = 5f;
	public float fadeDuration = 1f;
    public AudioClip bulletHitAudio;

	private void Start()
	{
        AudioSource AS = GetComponent<AudioSource>();
        if(AS != null)
        {
            AS.PlayOneShot(bulletHitAudio);
        }

		StartCoroutine(FadeBullet(fadeDuration, decayDelay));
	}

	IEnumerator FadeBullet(float fadeDuration, float decayDelay)
	{
		Destroy(gameObject, decayDelay+fadeDuration);
		yield return new WaitForSeconds(decayDelay);
		Material mat = GetComponent<Renderer>().material;
		Color fromColor = mat.color;
		Color toColor = new Color(fromColor.r, fromColor.g, fromColor.b, 0);
		float t = 0;
		while (mat.color.a > 0.01f)
		{
			mat.color = Color.Lerp(fromColor, toColor, t);
			t += Time.deltaTime / fadeDuration;
			yield return new WaitForSeconds(Time.deltaTime);
			if (gameObject == null)
				break;
		}
		yield return null;
	}

}
