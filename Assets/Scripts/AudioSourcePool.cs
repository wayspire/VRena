using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSourcePool : MonoBehaviour
{
    private static AudioSourcePool pool;

    public List<AudioSource> unActiveSources;
    public List<AudioSource> activeSources;
    [Space(5)]
    public int minAmt = 15;
    public int maxAmt = 30;
    public AudioMixerGroup defaultMixerGroup;
    

    //Vol goes from 0 to 1
    //pitch goes from -3 to 3
    public static void Play(AudioClip clip, Transform newParent, AudioMixerGroup mixerGroup, float vol = 1, float pitch = 1, bool attachToParent = true)
    {

        CheckInstantiated();
        pool.PostPlay(clip, newParent, mixerGroup, vol, pitch, attachToParent);
    }

    public void PostPlay(AudioClip clip, Transform newParent, AudioMixerGroup mixerGroup, float vol = 1, float pitch = 1, bool attachToParent = true)
    {

        AudioSource sourceToUse;
        if (unActiveSources.Count == 0)
        {
            sourceToUse = CreateSource();
        }
        else
        {
            sourceToUse = unActiveSources[0];
            sourceToUse.gameObject.SetActive(true);
        }


        //change parenting
        if (attachToParent)
        {
            sourceToUse.transform.parent = newParent;
            sourceToUse.transform.localPosition = new Vector3(0, 0, 0);
        }
        else
        {
            sourceToUse.transform.position = newParent.position;
        }

        //change audio settings
        sourceToUse.clip   = clip;
        sourceToUse.volume = vol;
        sourceToUse.pitch  = pitch;
        sourceToUse.outputAudioMixerGroup = mixerGroup;

        //Switch pool
        activeSources.Add(sourceToUse);
        unActiveSources.Remove(sourceToUse);

        //Play
        sourceToUse.Play();
    }

    private AudioSource CreateSource()
    {
        AudioSource sourceToUse;
        GameObject newSourceObj = new GameObject();

        //Audio Source stuff
        sourceToUse = newSourceObj.AddComponent<AudioSource>();
        sourceToUse.playOnAwake = false;
        sourceToUse.spatialBlend = 1;

        unActiveSources.Add(sourceToUse);

        //pool stuff
        ASPoolObj poolObj = newSourceObj.AddComponent<ASPoolObj>();
        poolObj.SetPool(pool);

        return sourceToUse;
    }

    public void ReturnToPool(AudioSource source)
    {

        if(unActiveSources.Count < maxAmt)
        {
            activeSources.Remove(source);
            unActiveSources.Add(source);

            source.gameObject.SetActive(false);
            source.transform.SetParent(pool.transform);
        }
        else
        {
            Destroy(source.gameObject);
        }
    }


    private static void CheckInstantiated()
    {
        if (pool == null)
        {
            pool = new GameObject("SoundPool").AddComponent<AudioSourcePool>();
            pool.SetUp();
            DontDestroyOnLoad(pool.gameObject);       
        }
    }

    //Start wouldnt get called in time so we have our own setup
    public void SetUp()
    {
        activeSources = new List<AudioSource>();
        unActiveSources = new List<AudioSource>();

        while(unActiveSources.Count < minAmt)
        {
            CreateSource();
        }
    }
}
