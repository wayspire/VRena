using UnityEngine;

public class DemoCameraManager : MonoBehaviour
{
	public Camera[] cameras;
    public Camera freeCam;

	int cameraIndex = 0;
	int newIndex = 0;

    public bool usingFreeCam = false;

	void Awake()
	{
		for (int i = 0; i < cameras.Length; i++)
		{
            cameras[i].gameObject.SetActive(false);
		}
		cameras[0].gameObject.SetActive(true);
        UI_OverHead.SetControllingPlayer(cameras[0].transform);
    }


	void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if(!usingFreeCam)
            {
                for (int i = 0; i < cameras.Length; i++)
                {
                    cameras[i].gameObject.SetActive(false);
                }
                freeCam.gameObject.SetActive(true);
                UI_OverHead.SetControllingPlayer(freeCam.transform);
            }else
            {
                freeCam.gameObject.SetActive(false);
                for (int i = 0; i < cameras.Length; i++)
                {
                    cameras[i].gameObject.SetActive(false);
                }
                cameras[0].gameObject.SetActive(true);
            }
            usingFreeCam = !usingFreeCam;          
        }


        if (!usingFreeCam)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                newIndex = cameraIndex + 1;
                if (newIndex >= cameras.Length)
                    newIndex = 0;

                cameras[cameraIndex].gameObject.SetActive(false);
                cameraIndex = newIndex;
                cameras[cameraIndex].gameObject.SetActive(true);
                UI_OverHead.SetControllingPlayer(cameras[cameraIndex].transform);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                newIndex = cameraIndex - 1;
                if (newIndex < 0)
                    newIndex = cameras.Length - 1;

                cameras[cameraIndex].gameObject.SetActive(false);
                cameraIndex = newIndex;
                cameras[cameraIndex].gameObject.SetActive(true);
                UI_OverHead.SetControllingPlayer(cameras[cameraIndex].transform);

            }
        }     
	}
}
