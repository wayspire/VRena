using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class DeathPostProcessing : MonoBehaviour
{
    public float speed = .5f;
    public Image image;

    float t = 0;

    public void DieAnim()
    {    
        StartCoroutine(Die());
    }

    public void ReviveAnim()
    {     
        StartCoroutine(Revive());
    }                    

    IEnumerator Die()
    {
        image.gameObject.SetActive(true);
        //grading.brightness = new FloatParameter() { value = 0 };
        t = 0;
        while (t < 1)
        {
            yield return null;
            t += speed;

            //volume.profile.GetSetting<ColorGrading>().colorFilter.value = newColor;
            image.color = new Color(0, 0, 0, (t) * .7f); 
        }
    }

    IEnumerator Revive()
    {
        t = 0;
        while (t < 1)
        {
            yield return null;
            t += speed;

            //volume.profile.GetSetting<ColorGrading>().colorFilter.value = newColor;
            image.color = new Color(0, 0, 0, (1 - t) * .7f);
        }
        image.gameObject.SetActive(false);
        
    }

}
