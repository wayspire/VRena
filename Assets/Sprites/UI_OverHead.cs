using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_OverHead : MonoBehaviour
{

    #region Static Variables

    static Color TEAMBLUE = new Color(0, 0, 1, 1); // team colors for the username display
    static Color TEAMPURPLE = new Color(1, 0, 1, 1);
    static Color TRANSBLACK = new Color(0, 0, 0, 0.5f); // translucent black (possible bar bg color)
    static Color GLOW = new Color(0, 0.5f, 1, 1); // color of the glowy pulsating thingy

    static int Users = 0; // count number of players in-game
    static int PulseFrames = 45;
    static float PulseDiv = 1f / PulseFrames;    // do this calculation ahead of time to save div calls
    #endregion

    // public
    public Image sprite_healthBar;
    public Image sprite_healthBarBG;
    public Image sprite_healthBarGlow;
    public Text text_name;
    public float maxHealth = 100f; // health bar is full at this value

    [Space(10)]
    [Header("Options")]
    public bool drawUsername = true; // draw username above head?
    public bool drawHealthbar = true; // draw health bar above head?
    public bool drawHealthGradient = true; // change health bar color based on % health?
    public bool drawHealthGlow = true;
    public bool autoFill = true; // automatically calculate health bar fill amount based on % health?

    private static Transform gameControllingPlayer;
    private bool pulse;

    [Space(10)]
    [Header("Hover over Head")] 
    public Vector3 offsetFromHead = new Vector3(0, .1f, 0);
    public Transform headTrans;


    // private
    private int Pulsen; // counter for glowing pulsating health bar
	private HealthWatch watch;
	int index;
	string thisPlayerName;


    private void Start()
    {
        Pulsen = 0;
        Defaults();

        sprite_healthBarGlow.GetComponent<RectTransform>().SetAsFirstSibling(); // ordering
        sprite_healthBar.GetComponent<RectTransform>().SetAsLastSibling(); // ordering
		

        //Example for setting fill amount manually...
        /*SetFillAmount(0.2f);
        ToggleAutoFill();*/

        // TESTING
        //TeamPurple();
    }
    
    void Update()
    {
        if (headTrans)
        {
            //Position
            transform.position = headTrans.transform.position + offsetFromHead;

            //Rotation
            transform.LookAt(gameControllingPlayer);
            Vector3 euler = transform.rotation.eulerAngles;
            transform.eulerAngles = new Vector3(0, euler.y, 0);
        }

        Pulse();
    }

    private void Pulse()
    {
        if (!drawHealthGlow)
        {
            return;
        }
            
        else if (sprite_healthBar.fillAmount <= 0.25f)
        {
            Pulsen = (Pulsen + 1) % UI_OverHead.PulseFrames;
            float pulseSize = 2.5f;
            float invAlpha = Pulsen * UI_OverHead.PulseDiv; // 1 - alpha
            float sizeX = 1 + pulseSize * (invAlpha * 0.1f);
            float sizeY = 1 + pulseSize * (invAlpha);
            sprite_healthBarGlow.color = new Color(
                UI_OverHead.GLOW.r,
                UI_OverHead.GLOW.g,
                UI_OverHead.GLOW.b,
                1 - invAlpha
                );
            sprite_healthBarGlow.transform.localScale = new Vector3(sizeX, sizeY, 1);
        }

        else
        {
            sprite_healthBarGlow.transform.localScale = new Vector3(0, 0, 1);
        }
    }

    public void TeamBlue()
    {
        text_name.color = UI_OverHead.TEAMBLUE;
    }

    public void TeamPurple()
    {
        text_name.color = UI_OverHead.TEAMPURPLE;
    }

    [PunRPC]
    private void RPC_SetHealthUi(float amt)
    {
        if (autoFill)
            sprite_healthBar.fillAmount = amt / maxHealth;

		//inserted by Wired
        if(watch == null) { FindHealthWatch(thisPlayerName); }
		watch.SetHealth(amt, maxHealth);

        if (amt < .25f)
            pulse = true;
    }

    [PunRPC]
    private void RPC_SetPlayerName(string name)
    {
        text_name.text = name;
    }


    // settings
    public void Defaults()
    {
        text_name.color = Color.white;
        //sprite_healthBar.color = UI_OverHead.GREEN;
        //sprite_healthBarBG.color = UI_OverHead.RED;

    }


    // setters
    public void SetFillAmount(float value) // value 0 to 1
    {
        // this function will not work unless autoFill == false
        sprite_healthBar.fillAmount = value;
    }

    public static void SetControllingPlayer(Transform playerTransform)
    {
        gameControllingPlayer = playerTransform;
    }

    //Should be called after names are set, "parents" health bar to head
    [PunRPC]
    public void RPC_ParentBarToHead(string playerName)
    {
        
        HealthObjectHolder[] holders =FindObjectsOfType<HealthObjectHolder>();
        for (int i = 0; i < holders.Length; i++)
        {
            if (holders[i].playerName == playerName && holders[i].bodyPart == HealthObjectHolder.BodyParts.Head)
            {
                headTrans = holders[i].transform;
            }
        }

        //Get watch
        FindHealthWatch(playerName);
    }

	//Should be called after names are set, links to health watch, inserted by Wired
	
	public void FindHealthWatch(string playerName)
	{
        NetD.PrintS("watch: this palyer name:" + playerName);
		HealthWatch[] watches = FindObjectsOfType<HealthWatch>();
		for (int i = 0; i < watches.Length; i++)
		{
            NetD.PrintS("watch: watch player name:" + watches[i].playerName);
            if (watches[i].playerName == playerName)
			{
				watch = watches[i];
			}
		}
	}
}
