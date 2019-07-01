using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
	public GameObject gameManager;
	public GameObject soundManager;

    public string workingScene;         // Dev tool

    void Start()
	{
        if (PhotonNetwork.IsMasterClient)
        {
            //if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName(workingScene))
                //SceneManager.LoadScene(workingScene);

            if (GameManager.instance == null)
            {
                gameManager = Instantiate(gameManager);
                DontDestroyOnLoad(soundManager);
            }
            if (SoundManager.instance == null)
            {
                soundManager = Instantiate(soundManager);
                DontDestroyOnLoad(soundManager);
            }
        }
	}
}
