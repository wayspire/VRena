using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASPoolObj : MonoBehaviour
{
    AudioSourcePool pool;
    // Start is called before the first frame update
    AudioSource source;
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!source.isPlaying)
        {
            pool.ReturnToPool(source);
        }
    }

    public void SetPool(AudioSourcePool pool)
    {
        this.pool = pool;
    }
}
